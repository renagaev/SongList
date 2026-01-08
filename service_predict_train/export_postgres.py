#!/usr/bin/env python3
import argparse
import json
import os
from pathlib import Path

import pandas as pd

SONGS_SQL = """
SELECT
    s."Id" AS id,
    s."ShowedAt" AS start_at,
    s."HiddenAt" AS end_at,
    EXTRACT(EPOCH FROM (s."HiddenAt" - s."ShowedAt"))::double precision AS length_sec,
    s."IsServiceTrain" AS is_service_train
FROM public."SongShows" s
WHERE s."IsServiceTrain" IS NOT NULL
  AND s."ShowedAt" IS NOT NULL
  AND s."HiddenAt" IS NOT NULL
ORDER BY s."Id";
"""

SLIDES_SQL = """
SELECT
    sh."SlideNumber" AS slide_number,
    sh."TotalSlides" AS total_slides,
    sh."ShowedAt" AS showed_at,
    sh."HiddenAt" AS hidden_at,
    sh."SongShowId" AS show_id
FROM public."SlideHistory" sh
JOIN public."SongShows" s ON s."Id" = sh."SongShowId"
WHERE s."IsServiceTrain" IS NOT NULL
  AND sh."SongShowId" IS NOT NULL
  AND sh."ShowedAt" IS NOT NULL
  AND sh."HiddenAt" IS NOT NULL
ORDER BY sh."SongShowId", sh."ShowedAt";
"""


def _normalize_connection_string(raw):
    raw = raw.strip()
    if "://" in raw:
        return raw
    if ";" not in raw:
        return raw

    parts = [p for p in raw.split(";") if p.strip()]
    kv = {}
    for part in parts:
        if "=" not in part:
            continue
        key, value = part.split("=", 1)
        kv[key.strip().lower()] = value.strip()

    mapping = [
        ("host", "host"),
        ("server", "host"),
        ("data source", "host"),
        ("port", "port"),
        ("database", "dbname"),
        ("initial catalog", "dbname"),
        ("user id", "user"),
        ("userid", "user"),
        ("user", "user"),
        ("username", "user"),
        ("password", "password"),
        ("sslmode", "sslmode"),
        ("ssl mode", "sslmode"),
        ("options", "options"),
    ]

    dsn_parts = []
    seen = set()
    for key, dest in mapping:
        if key in kv and dest not in seen:
            dsn_parts.append(f"{dest}={kv[key]}")
            seen.add(dest)

    if not dsn_parts:
        raise ValueError("Could not parse connection string")
    return " ".join(dsn_parts)


def _load_connection_string(args):
    if args.connection:
        return args.connection
    env_value = os.getenv("DB_CONNECTION_STRING") or os.getenv("SONGLIST_DB")
    if env_value:
        return env_value
    appsettings_path = Path(args.appsettings)
    if appsettings_path.is_file():
        data = json.loads(appsettings_path.read_text())
        if args.appsettings_key in data:
            return data[args.appsettings_key]
        conn_section = data.get("ConnectionStrings", {})
        if args.appsettings_key in conn_section:
            return conn_section[args.appsettings_key]
    raise ValueError("Connection string not found. Use --connection or set DB_CONNECTION_STRING.")


def _connect(conn_str):
    try:
        import psycopg

        return psycopg.connect(conn_str)
    except ImportError:
        try:
            import psycopg2
        except ImportError as exc:
            raise SystemExit(
                "Please install psycopg or psycopg2-binary in the current venv."
            ) from exc
        return psycopg2.connect(conn_str)


def _fetch_df(conn, query):
    with conn.cursor() as cur:
        cur.execute(query)
        rows = cur.fetchall()
        columns = [desc[0] for desc in cur.description]
    return pd.DataFrame(rows, columns=columns)


def main():
    base_dir = Path(__file__).resolve().parent
    default_appsettings = base_dir.parent / "SongList.Web" / "appsettings.json"

    parser = argparse.ArgumentParser(description="Export training data from Postgres.")
    parser.add_argument("--connection", help="Postgres connection string (Npgsql or libpq).")
    parser.add_argument(
        "--appsettings",
        default=str(default_appsettings),
        help="Path to appsettings.json with DbConnectionString.",
    )
    parser.add_argument(
        "--appsettings-key",
        default="DbConnectionString",
        help="Key name for the connection string in appsettings.json.",
    )
    parser.add_argument(
        "--out-dir",
        default=str(base_dir),
        help="Output directory for song_shows.csv and slides.csv.",
    )
    parser.add_argument("--songs-path", default=None, help="Override song_shows.csv path.")
    parser.add_argument("--slides-path", default=None, help="Override slides.csv path.")
    args = parser.parse_args()

    conn_raw = _load_connection_string(args)
    conn_str = _normalize_connection_string(conn_raw)
    out_dir = Path(args.out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)

    songs_path = Path(args.songs_path) if args.songs_path else out_dir / "song_shows.csv"
    slides_path = Path(args.slides_path) if args.slides_path else out_dir / "slides.csv"

    with _connect(conn_str) as conn:
        songs_df = _fetch_df(conn, SONGS_SQL)
        slides_df = _fetch_df(conn, SLIDES_SQL)

    songs_df.to_csv(songs_path, index=False)
    slides_df.to_csv(slides_path, index=False)
    print(f"Exported {len(songs_df)} songs to {songs_path}")
    print(f"Exported {len(slides_df)} slides to {slides_path}")


if __name__ == "__main__":
    main()
