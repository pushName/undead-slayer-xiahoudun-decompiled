"""Export every Texture2D out of the Unity 4 build data as PNG, and record the reference
table the material exporter will need.

Verified first: all 7 texture formats present in this data (RGBA4444 611, ETC_RGB4 155,
RGBA32 120, RGB24 12, ARGB32 3, Alpha8 7, RGB565 7) decode through UnityPy's .image, so no
custom decoder is required -- including the ETC1 ones.

Filenames always carry the source path_id. Two reasons: the shipped data contains names
that differ only by case (Windows would collide them), and duplicates with the same name
but different contents are common across AssetBundles.

Emits:
  unity/Assets/Imported/Textures/<bucket>/<name>_<path_id>.png
  tools/migration/texture-index.json    "<file>!<path_id>" -> "Assets/Imported/..."

Run:  <unitypy venv python> tools/migration/export_textures.py [--dry-run]
"""
import glob
import json
import os
import re
import sys

import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP = u4.DUMP
RECON = u4.RECON
OUT_ASSETS = os.path.join(_REPO, "unity", "Assets", "Imported", "Textures")
OUT_INDEX = os.path.join(_REPO, "tools", "migration", "texture-index.json")

DRY = "--dry-run" in sys.argv

BAD = re.compile(r'[<>:"/\\|?*\x00-\x1f]')


def safe(name, path_id):
    n = BAD.sub("_", (name or "").strip()).strip(". ")
    n = n[:60] or "texture"
    return "%s_%d" % (n, path_id)


def bucket_of(basename):
    if basename.startswith("level"):
        return "level"
    if basename.startswith("sharedassets"):
        return "sharedassets"
    return "bundles"


def main():
    index = {}
    counts = {"exported": 0, "failed": 0, "skipped_dup": 0}
    bytes_total = 0
    per_bucket = {}

    for name, path in u4.sources():
        bucket = bucket_of(name)
        try:
            env = u4.load(path)
        except Exception:
            continue
        texs = [o for o in env.objects if o.type.name == "Texture2D"]
        if not texs:
            continue
        dest_dir = os.path.join(OUT_ASSETS, bucket)
        for o in texs:
            try:
                d = o.read()
                img = d.image
                fname = safe(str(getattr(d, "m_Name", "")), o.path_id) + ".png"
                rel = "Assets/Imported/Textures/%s/%s" % (bucket, fname)
                key = "%s!%d" % (name, o.path_id)
                if key in index:
                    counts["skipped_dup"] += 1
                    continue
                index[key] = rel
                if not DRY:
                    os.makedirs(dest_dir, exist_ok=True)
                    abs_path = os.path.join(_REPO, "unity", rel)
                    img.save(abs_path)
                    bytes_total += os.path.getsize(abs_path)
                counts["exported"] += 1
                per_bucket[bucket] = per_bucket.get(bucket, 0) + 1
            except Exception as exc:
                counts["failed"] += 1
                if counts["failed"] <= 5:
                    print("FAIL %s!%d: %r" % (name, o.path_id, exc))

    print("%s textures exported, %d failed, %d duplicate pointers skipped"
          % (counts["exported"], counts["failed"], counts["skipped_dup"]))
    for b, n in sorted(per_bucket.items()):
        print("   %-14s %d" % (b, n))
    if not DRY:
        print("on-disk total: %.1f MB" % (bytes_total / 1048576.0))
        with open(OUT_INDEX, "w", encoding="utf-8") as fh:
            json.dump(index, fh, indent=1, ensure_ascii=False, sort_keys=True)
        print("wrote %s" % os.path.relpath(OUT_INDEX, _REPO).replace(os.sep, "/"))
    else:
        print("dry run -- nothing written")


if __name__ == "__main__":
    main()
