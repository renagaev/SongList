#!/usr/bin/env python3
"""
Train GradientBoosting (non-future features) and export to ONNX.
Outputs:
  model.onnx        - ONNX model (input: float tensor shaped [*, n_features], output: prob,label)
  onnx_meta.json    - metadata with feature order and postprocess rules
Requirements: skl2onnx, onnx, numpy (install in .venv: pip install skl2onnx onnx)
"""
import argparse
import json
from pathlib import Path

from sklearn.ensemble import GradientBoostingClassifier

from service_model import build_features, load_data

try:
    from skl2onnx import convert_sklearn
    from skl2onnx.common.data_types import FloatTensorType
except ImportError as e:
    raise SystemExit("Please install skl2onnx and onnx in the current venv: pip install skl2onnx onnx") from e


def train_model():
    songs, slides = load_data()
    X_all, y, df, feat_cols = build_features(songs, slides)
    future_cols = {"gap_next", "shows_in_day", "order_in_day"}
    feat_cols = [c for c in feat_cols if c not in future_cols]
    X = df[feat_cols].fillna(0)
    clf = GradientBoostingClassifier(random_state=42)
    clf.fit(X, y)
    print(f"Training on full dataset: {len(X)} rows.")
    return clf, feat_cols


def _sanitize_model(onx, ir_version=None):
    if ir_version is not None:
        onx.ir_version = min(onx.ir_version, ir_version)

    opset_by_domain = {}
    for op in onx.opset_import:
        current = opset_by_domain.get(op.domain, 0)
        if op.version > current:
            opset_by_domain[op.domain] = op.version

    onx.opset_import.clear()
    for domain, version in opset_by_domain.items():
        entry = onx.opset_import.add()
        entry.domain = domain
        entry.version = version

    return onx


def export_onnx(
    clf, feat_cols, onnx_path="model.onnx", meta_path="onnx_meta.json", target_opset=17, ir_version=8
):
    initial_type = [("input", FloatTensorType([None, len(feat_cols)]))]
    onx = convert_sklearn(
        clf,
        initial_types=initial_type,
        target_opset=target_opset,
        options={type(clf): {"zipmap": False}},
    )
    onx = _sanitize_model(onx, ir_version=ir_version)
    Path(onnx_path).write_bytes(onx.SerializeToString())
    meta = {
        "feature_order": feat_cols,
        "postprocess": {
            "force_false_if_unique_slides_le_2": True,
            "note": "Set prediction to False if unique_slides<=2 before/after ONNX inference.",
        },
        "input_name": "input",
        "output_names": ["probabilities", "label"],
    }
    Path(meta_path).write_text(json.dumps(meta, indent=2))
    print(f"Saved ONNX model to {onnx_path} and metadata to {meta_path}")


def main():
    base_dir = Path(__file__).resolve().parent
    default_out_dir = base_dir.parent / "SongList.ServicePredict"

    parser = argparse.ArgumentParser(description="Train GradientBoosting and export ONNX.")
    parser.add_argument(
        "--out-dir",
        default=str(default_out_dir),
        help="Output directory for model.onnx and onnx_meta.json.",
    )
    parser.add_argument(
        "--opset",
        type=int,
        default=15,
        help="Target ONNX opset version for export.",
    )
    parser.add_argument(
        "--ir-version",
        type=int,
        default=8,
        help="Max ONNX IR version to emit (lower for compatibility).",
    )
    args = parser.parse_args()

    out_dir = Path(args.out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)

    clf, feat_cols = train_model()
    export_onnx(
        clf,
        feat_cols,
        onnx_path=out_dir / "model.onnx",
        meta_path=out_dir / "onnx_meta.json",
        target_opset=args.opset,
        ir_version=args.ir_version,
    )


if __name__ == "__main__":
    main()
