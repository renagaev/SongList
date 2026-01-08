#!/usr/bin/env python3
import argparse
from pathlib import Path

import joblib
import pandas as pd
from sklearn.ensemble import GradientBoostingClassifier
from sklearn.metrics import (
    accuracy_score,
    confusion_matrix,
    precision_recall_fscore_support,
)
from sklearn.model_selection import cross_val_score, train_test_split


def _coerce_bool_series(series):
    def _coerce(val):
        if pd.isna(val):
            return pd.NA
        if isinstance(val, bool):
            return val
        if isinstance(val, (int, float)) and not isinstance(val, bool):
            return bool(val)
        if isinstance(val, str):
            v = val.strip().lower()
            if v in ("true", "t", "1", "yes", "y"):
                return True
            if v in ("false", "f", "0", "no", "n"):
                return False
        return pd.NA

    return series.map(_coerce).astype("boolean")


def load_data(songs_path="song_shows.csv", slides_path="slides.csv"):
    songs = pd.read_csv(songs_path, parse_dates=["start_at", "end_at"])
    song_required = {"id", "start_at", "end_at", "is_service_train"}
    missing = song_required - set(songs.columns)
    if missing:
        raise ValueError(
            f"songs csv missing columns: {sorted(missing)}. Run export_postgres.py to rebuild."
        )

    songs["id"] = pd.to_numeric(songs["id"], errors="coerce")
    songs["start_at"] = pd.to_datetime(songs["start_at"], errors="coerce")
    songs["end_at"] = pd.to_datetime(songs["end_at"], errors="coerce")

    if "length_sec" in songs.columns:
        songs["length_sec"] = pd.to_numeric(songs["length_sec"], errors="coerce")
        if songs["length_sec"].isna().any():
            computed = (songs["end_at"] - songs["start_at"]).dt.total_seconds()
            songs["length_sec"] = songs["length_sec"].fillna(computed)
    else:
        songs["length_sec"] = (songs["end_at"] - songs["start_at"]).dt.total_seconds()

    songs["is_service_train"] = _coerce_bool_series(songs["is_service_train"])
    songs = songs[songs["is_service_train"].notna()].copy()
    songs["is_service_train"] = songs["is_service_train"].astype(bool)
    songs = songs.dropna(subset=["id", "start_at", "end_at", "length_sec"])

    slides = pd.read_csv(slides_path, parse_dates=["showed_at", "hidden_at"])
    slide_required = {"slide_number", "total_slides", "showed_at", "hidden_at", "show_id"}
    missing = slide_required - set(slides.columns)
    if missing:
        raise ValueError(
            f"slides csv missing columns: {sorted(missing)}. Run export_postgres.py to rebuild."
        )

    slides["show_id"] = pd.to_numeric(slides["show_id"], errors="coerce")
    slides["slide_number"] = pd.to_numeric(slides["slide_number"], errors="coerce")
    slides["total_slides"] = pd.to_numeric(slides["total_slides"], errors="coerce")
    slides["showed_at"] = pd.to_datetime(slides["showed_at"], errors="coerce")
    slides["hidden_at"] = pd.to_datetime(slides["hidden_at"], errors="coerce")
    slides = slides.dropna(subset=["show_id", "showed_at", "hidden_at"])
    slides = slides[slides["show_id"].isin(songs["id"])]

    slides = slides.sort_values(["show_id", "showed_at"])
    slides["dur"] = (slides["hidden_at"] - slides["showed_at"]).dt.total_seconds().clip(upper=600)
    slides["order"] = slides.groupby("show_id").cumcount() + 1
    return songs, slides


def _min_dist_to_anchors(ts, anchors):
    """Return minimal absolute distance in minutes from timestamp to any weekly anchor.
    anchors: list of (dow_py, hour, minute), where dow_py: Monday=0.
    We consider anchors in the same week and +/- one week to catch close borders.
    """
    if pd.isna(ts):
        return float("inf")
    ts_naive = ts.to_pydatetime().replace(tzinfo=None)
    res = float("inf")
    for dow, h, m in anchors:
        # anchor in same week
        base_date = ts_naive + pd.to_timedelta(dow - ts_naive.weekday(), unit="D")
        for delta in (-7, 0, 7):
            candidate = base_date + pd.to_timedelta(delta, unit="D")
            candidate = candidate.replace(hour=h, minute=m, second=0, microsecond=0)
            res = min(res, abs((ts_naive - candidate).total_seconds()) / 60.0)
    return res


def _clean_slides_for_show(g):
    """Apply heuristic cleanup reflecting annotation notes:
    - drop the first slide (technical black screen)
    - if a slide was shown twice, keep the later one
    - break on gaps >10 minutes between slides; keep the largest contiguous block
    """
    if g.empty:
        return g
    g = g.sort_values("showed_at")
    # drop first slide entirely
    g = g.iloc[1:] if len(g) > 1 else g.iloc[0:0]
    if g.empty:
        return g
    # deduplicate slide_number, keep the last showing
    g = g.drop_duplicates(subset=["slide_number"], keep="last")
    # segment by long gaps
    gaps = g["showed_at"].diff().dt.total_seconds().fillna(0)
    g = g.assign(segment=(gaps > 600).cumsum())
    if g["segment"].nunique() > 1:
        # keep largest segment (assume it is the relevant service/rehersal)
        top_segment = g["segment"].value_counts().idxmax()
        g = g[g["segment"] == top_segment]
    return g.drop(columns=["segment"])


def build_features(songs, slides):
    base = songs.copy()
    # Postgres dow: Sunday=0
    base["dow_pg"] = (base["start_at"].dt.dayofweek + 1) % 7
    base["hour"] = base["start_at"].dt.hour
    base["date"] = base["start_at"].dt.date
    base["gap_prev"] = (base["start_at"] - base.groupby("date")["end_at"].shift(1)).dt.total_seconds()
    base["gap_next"] = (base.groupby("date")["start_at"].shift(-1) - base["end_at"]).dt.total_seconds()
    base["shows_in_day"] = base.groupby("date")["id"].transform("count")
    base["order_in_day"] = base.groupby("date")["start_at"].rank("first").astype(int)
    base["delta_to_11m"] = (base["start_at"].dt.hour * 60 + base["start_at"].dt.minute - 11 * 60).abs()
    base["delta_to_19m"] = (base["start_at"].dt.hour * 60 + base["start_at"].dt.minute - 19 * 60).abs()
    base["is_sunday"] = base["dow_pg"] == 0
    base["is_saturday"] = base["dow_pg"] == 6
    base_idx = base.set_index("id")
    # Distances to typical service/rehearsal anchors (minutes, soft)
    service_anchors = [(2, 19, 0), (4, 19, 0), (6, 11, 0), (6, 19, 0)]  # Wed, Fri, Sun 11/19
    rehearsal_anchors = [(5, 20, 0), (6, 9, 30), (6, 20, 30)]  # Sat 20:00, Sun morning, Sun after evening
    base_idx["dist_to_service_min"] = base_idx["start_at"].apply(lambda x: _min_dist_to_anchors(x, service_anchors))
    base_idx["dist_to_rehearsal_min"] = base_idx["start_at"].apply(lambda x: _min_dist_to_anchors(x, rehearsal_anchors))

    cleaned = slides.groupby("show_id", group_keys=False).apply(_clean_slides_for_show)

    def _slide_stats(g):
        total_slides_val = g["total_slides"].max()
        unique_slides = g["slide_number"].nunique()
        shown_ratio = (
            unique_slides / total_slides_val
            if pd.notna(total_slides_val) and total_slides_val > 0
            else 0.0
        )
        return pd.Series(
            {
                "n_slides": len(g),
                "unique_slides": unique_slides,
                "shown_ratio": shown_ratio,
                "total_time": g["dur"].sum(),
                "mean_dur": g["dur"].mean(),
                "median_dur": g["dur"].median(),
                "std_dur": g["dur"].std(),
                "min_dur": g["dur"].min(),
                "max_dur": g["dur"].max(),
                "span": g["slide_number"].max() - g["slide_number"].min(),
                "cv_dur": (g["dur"].std() / g["dur"].mean()) if g["dur"].mean() else 0.0,
                "repeat_ratio": 1 - unique_slides / len(g) if len(g) else 0.0,
                "unique_ratio": unique_slides / len(g) if len(g) else 0.0,
                "first_dur": g.iloc[0]["dur"],
                "last_dur": g.iloc[-1]["dur"],
                "max_pos": g.loc[g["dur"].idxmax(), "order"],
                "max_pos_norm": g.loc[g["dur"].idxmax(), "order"] / len(g),
                "long_tail_mean": g["dur"].nlargest(3).mean() if len(g) >= 3 else g["dur"].mean(),
                "long_tail_ratio": (g["dur"].nlargest(3).sum() / g["dur"].sum()) if g["dur"].sum() else 0.0,
                "total_over_length": (g["dur"].sum() / base_idx.loc[g.name, "length_sec"])
                if base_idx.loc[g.name, "length_sec"]
                else 0.0,
            }
        )

    slide_features = cleaned.groupby("show_id").apply(_slide_stats)
    slide_features.index.name = "id"

    df = base_idx.join(slide_features, how="left")
    # Override labels: 1–2 slides are annotation errors -> treat as non-service during training
    small_slides = df["unique_slides"].fillna(0) <= 2
    y = df["is_service_train"].copy()
    y.loc[small_slides] = False

    feat_cols = [
        "dow_pg",
        "hour",
        "length_sec",
        "n_slides",
        "unique_slides",
        "total_time",
        "total_over_length",
        "mean_dur",
        "median_dur",
        "std_dur",
        "min_dur",
        "max_dur",
        "span",
        "cv_dur",
        "repeat_ratio",
        "unique_ratio",
        "shown_ratio",
        "first_dur",
        "last_dur",
        "max_pos",
        "max_pos_norm",
        "long_tail_mean",
        "long_tail_ratio",
        "gap_prev",
        "gap_next",
        "shows_in_day",
        "order_in_day",
        "delta_to_11m",
        "delta_to_19m",
        "is_sunday",
        "is_saturday",
        "dist_to_service_min",
        "dist_to_rehearsal_min",
    ]
    X = df[feat_cols].fillna(0)
    return X, y, df, feat_cols


def eval_rule(df):
    rule_pred = ((df["std_dur"] < 11) | (df["hour"].isin([11, 19]))) & ~(
        (df["dow_pg"] == 6) & (df["hour"] >= 17)
    )
    # 1–2 слайда всегда не-служение
    rule_pred[df["unique_slides"].fillna(0) <= 2] = False
    prec, rec, f1, _ = precision_recall_fscore_support(df["is_service_train"], rule_pred, average="binary")
    acc = accuracy_score(df["is_service_train"], rule_pred)
    print("Rule: acc {:.3f} precision {:.3f} recall {:.3f} f1 {:.3f}".format(acc, prec, rec, f1))


def eval_model(X, y, feat_cols):
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )
    clf = GradientBoostingClassifier(random_state=42)
    clf.fit(X_train, y_train)
    y_pred = clf.predict(X_test)
    acc = accuracy_score(y_test, y_pred)
    prec, rec, f1, _ = precision_recall_fscore_support(y_test, y_pred, average="binary")
    print("GB split acc {:.3f} precision {:.3f} recall {:.3f} f1 {:.3f}".format(acc, prec, rec, f1))
    print("Confusion matrix\n", confusion_matrix(y_test, y_pred))
    cv = cross_val_score(clf, X, y, cv=5, scoring="accuracy")
    print("CV accuracy mean {:.3f} scores {}".format(cv.mean(), [round(x, 3) for x in cv]))
    importances = sorted(
        zip(feat_cols, clf.feature_importances_), key=lambda x: -x[1]
    )
    print("Top importances:")
    for name, val in importances[:10]:
        print("  {:20s} {:.3f}".format(name, val))


def train_and_save(X, y, feat_cols, model_path=None, test_size=0.2, random_state=42, do_cv=True):
    """Train model, print metrics, optionally save artifact."""
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=test_size, random_state=random_state, stratify=y
    )
    clf = GradientBoostingClassifier(random_state=random_state)
    clf.fit(X_train, y_train)
    y_pred = pd.Series(clf.predict(X_test), index=X_test.index)
    # 1–2 слайда всегда не-служение
    small = X_test["unique_slides"] <= 2
    y_pred[small] = False
    acc = accuracy_score(y_test, y_pred)
    prec, rec, f1, _ = precision_recall_fscore_support(y_test, y_pred, average="binary")
    print("GB split acc {:.3f} precision {:.3f} recall {:.3f} f1 {:.3f}".format(acc, prec, rec, f1))
    print("Confusion matrix\n", confusion_matrix(y_test, y_pred))

    if do_cv:
        cv = cross_val_score(clf, X, y, cv=5, scoring="accuracy")
        print("CV accuracy mean {:.3f} scores {}".format(cv.mean(), [round(x, 3) for x in cv]))

    importances = sorted(zip(feat_cols, clf.feature_importances_), key=lambda x: -x[1])
    print("Top importances:")
    for name, val in importances[:10]:
        print("  {:20s} {:.3f}".format(name, val))

    if model_path:
        artifact = {
            "model": clf,
            "feat_cols": feat_cols,
        }
        Path(model_path).parent.mkdir(parents=True, exist_ok=True)
        joblib.dump(artifact, model_path)
        print(f"Saved model to {model_path}")


def main():
    parser = argparse.ArgumentParser(description="Train service classifier and/or evaluate rule.")
    parser.add_argument("--songs", default="song_shows.csv", help="Path to song_shows.csv")
    parser.add_argument("--slides", default="slides.csv", help="Path to slides.csv")
    parser.add_argument("--model-path", default=None, help="Where to save trained model (joblib).")
    parser.add_argument("--no-cv", action="store_true", help="Skip cross-validation for speed.")
    parser.add_argument("--test-size", type=float, default=0.2, help="Holdout fraction.")
    parser.add_argument("--seed", type=int, default=42, help="Random seed.")
    args = parser.parse_args()

    songs, slides = load_data(args.songs, args.slides)
    X, y, df, feat_cols = build_features(songs, slides)
    print("Loaded", len(df), "rows. Train share:", round(float(y.mean()), 3))
    eval_rule(df)

    # Use only features доступные сразу после показа (без будущих сведений)
    future_cols = {"gap_next", "shows_in_day", "order_in_day"}
    feat_cols_train = [c for c in feat_cols if c not in future_cols]
    X = df[feat_cols_train].fillna(0)

    train_and_save(
        X,
        y,
        feat_cols_train,
        model_path=args.model_path,
        test_size=args.test_size,
        random_state=args.seed,
        do_cv=not args.no_cv,
    )


if __name__ == "__main__":
    main()
