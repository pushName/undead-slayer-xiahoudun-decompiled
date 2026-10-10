"""Export every Texture2D out of the Unity 4 build data as PNG, and record the reference
table the material exporter will need.

Census: 915 Texture2D objects, 912 files. Formats RGBA4444 611, ETC_RGB4 155, RGBA32 120, RGB24 12,
Alpha8 7, RGB565 7, ARGB32 3 -- every one of them decodes through UnityPy's .image except three
Alpha8 instances that are 0x0 and raise "No image data found". Those three are all named
"Font Texture" and are the atlases Unity 4 bakes for dynamic fonts (`sharedassets0!27`,
`sharedassets14!23`, `unity default resources!10103`, referenced from `Font.m_Texture` and
`PreloadData.m_Assets`). A 2022 dynamic Font has no baked atlas either -- it rebuilds one from the
font's character data at runtime -- so there is nothing to write for them and nothing should be
pointed at one. They are excluded from the index and recorded with their reason in
texture-skips.json; the exporter that ran before this split counted them as "3 failed" in a summary
line, which is why the naming migration below has to reconcile 912 against 915 rather than assume.

Naming, since 2026-10-10: the file name is the source texture name, and `_<path_id>` is appended
ONLY when that name is already taken (46 of 912 need it). The old rule (always append) made sprites
unreachable by name: check_resources_load_keys.py found 23 of the 63 Resources.Load keys blocked that
way across 135 files, because the code asks for "jewel01a" while the disk held "jewel01a_91.png".
Comparison is case-insensitive, because Windows would collide `Sk_1` with `sk_1` into one file anyway.
After the migration that check reports 25 keys OK and 38 waiting on prefabs (task #17), and no key is
blocked by naming any more.

Placement: a texture whose name the shipped code loads is written under unity/Assets/Resources/,
flat, because that is the only place Resources.Load can see; everything else stays under
Assets/Imported/Textures/<bucket>/, where scene and material references reach it by guid.

Renaming a file and carrying its .meta along keeps the guid, so references survive; re-exporting
under a new name does NOT keep the guid, which is why the one-time migration lives in
apply_texture_naming.py. What this file does is write image CONTENT into the planned paths, so it is
idempotent and cannot create a second name for a texture that has already been migrated. The 907
moves were checked from the referring side afterwards: all 1321 texture references in the project's
.mat/.unity/.prefab files still resolve to a file that exists.

Emits:
  unity/Assets/Resources/<name>.png                     (name-loaded textures only)
  unity/Assets/Imported/Textures/<bucket>/<name>.png
  tools/migration/texture-index.json    "<file>!<path_id>" -> "Assets/..."
  tools/migration/texture-skips.json    "<file>!<path_id>" -> why no file exists

Order matters: apply_texture_naming.py renames in place (guids preserved), THEN this writes any
planned path that is still missing. Run it before the rename and it writes a second PNG for a
texture that already has one under the old name -- a new asset with a new guid, and the orphans
report at the end of this script is what catches that.

Run:  <unitypy venv python> tools/migration/export_textures.py [--dry-run]  RECON_DIR=<path>
"""
import argparse
import collections
import json
import os
import re
import sys

import unity4_files as u4
from scan_prefabs import call_sites

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
UNITY = os.path.join(_REPO, "unity")
OUT_INDEX = os.path.join(_REPO, "tools", "migration", "texture-index.json")
OUT_SKIPS = os.path.join(_REPO, "tools", "migration", "texture-skips.json")

BAD = re.compile(r'[<>:"/\\|?*\x00-\x1f]')


def base_name(name, path_id):
    """The name a texture wants to be called: the source name, sanitized, without a path_id."""
    n = BAD.sub("_", (name or "").strip()).strip(". ")
    return n[:60] or "texture_%d" % path_id


def bucket_of(basename):
    if basename.startswith("level"):
        return "level"
    if basename.startswith("sharedassets"):
        return "sharedassets"
    return "bundles"


def loadable_names():
    """-> (whole-key set, concatenation-head list) from the one census the game is checked with."""
    sites = call_sites()
    exact = {k.lower() for k, v in sites.items() if not v["head"]}
    heads = sorted({k.lower() for k, v in sites.items() if v["head"]})
    return exact, heads


def scan_textures():
    """[(key, bucket, source name, path_id, width, height, format)] for every Texture2D, stable order.

    A container that fails to load raises rather than being skipped. UnityPy returns an env for a
    truncated file and parses phantom objects out of it, and a caller that only sees a shorter list
    reports the smaller world as if it were the world -- see unity4_files.sources().
    """
    entries, unloadable = [], []
    for name, path in u4.sources():
        try:
            env = u4.load(path)
            counted = 0
            for o in env.objects:
                if o.type.name != "Texture2D":
                    continue
                try:
                    d = o.read()
                    raw = str(getattr(d, "m_Name", "") or "")
                    w = int(getattr(d, "m_Width", 0) or 0)
                    h = int(getattr(d, "m_Height", 0) or 0)
                    fmt = str(getattr(d, "m_TextureFormat", "?"))
                except Exception as exc:
                    unloadable.append("%s!%d: header unreadable (%s)" % (name, o.path_id, exc))
                    continue
                entries.append(("%s!%d" % (name, o.path_id), bucket_of(name), raw,
                                o.path_id, w, h, fmt))
                counted += 1
        except Exception as exc:
            unloadable.append("%s: container failed to load (%s)" % (name, exc))
    entries.sort()
    if unloadable:
        raise RuntimeError("%d load failure(s), so this scan is not the whole build. Refusing to "
                           "plan against a partial world:\n   %s"
                           % (len(unloadable), "\n   ".join(unloadable[:8])))
    return entries


def is_baked(o):
    """False when the object carries no pixels at all -- in this build, exactly the 3 font atlases."""
    return o[4] > 0 and o[5] > 0


def plan_names(entries, exact, heads):
    """entries -> ({key: rel-path}, {key: reason}). The single source of the naming rule.

    Collisions are all-or-nothing: when a name is used by more than one texture, every user of it
    keeps the path_id suffix. Leaving exactly one unsuffixed would make Resources.Load return an
    arbitrary member of the group, which is worse than a load that demonstrably cannot resolve --
    and it would make the choice depend on iteration order.
    """
    exportable = [o for o in entries if is_baked(o)]
    used = collections.Counter()
    for o in exportable:
        used[base_name(o[2], o[3]).lower()] += 1

    out, skipped = {}, {}
    for entry in entries:
        key, bucket, raw, pid, w, h, fmt = entry
        if not is_baked(entry):
            skipped[key] = {"name": base_name(raw, pid), "size": "%dx%d" % (w, h),
                            "format": fmt,
                            "reason": "no image data in the build (Unity 4 runtime font atlas); "
                                      ".image raises 'No image data found'"}
            continue
        clean = base_name(raw, pid)
        name = clean if used[clean.lower()] == 1 else "%s_%d" % (clean, pid)
        low = name.lower()
        if low in exact or any(low.startswith(h2) for h2 in heads):
            out[key] = "Assets/Resources/%s.png" % name
        else:
            out[key] = "Assets/Imported/Textures/%s/%s.png" % (bucket, name)
    return out, skipped


def write_missing(plan, skips, dry):
    """Save image data for planned paths that do not exist yet. Existing files are left alone."""
    counts = collections.Counter()
    bytes_total = 0
    problems = []
    for name, path in u4.sources():
        try:
            env = u4.load(path)
        except Exception as exc:
            problems.append("%s: container failed to load (%s)" % (name, exc))
            continue
        for o in env.objects:
            if o.type.name != "Texture2D":
                continue
            key = "%s!%d" % (name, o.path_id)
            rel = plan.get(key)
            if rel is None:
                if key in skips:
                    counts["no image data"] += 1
                else:
                    problems.append("%s: planned by neither the index nor the skip list" % key)
                continue
            abs_path = os.path.join(UNITY, rel.replace("/", os.sep))
            if os.path.exists(abs_path):
                counts["already present"] += 1
                continue
            try:
                img = o.read().image
            except Exception as exc:
                counts["decode failed"] += 1
                problems.append("decode %s: %r" % (rel, exc))
                continue
            counts["written"] += 1
            if not dry:
                os.makedirs(os.path.dirname(abs_path), exist_ok=True)
                img.save(abs_path)
                bytes_total += os.path.getsize(abs_path)
    if not dry:
        print("bytes written this run: %.1f MB" % (bytes_total / 1048576.0))
    return counts, problems


def report_orphans(plan):
    """PNGs on disk that no planned path claims -- stale names left behind by a migration."""
    want = set(plan.values())
    orphans = []
    for root, _d, files in os.walk(os.path.join(UNITY, "Assets")):
        for fn in files:
            if not fn.endswith(".png"):
                continue
            rel = os.path.relpath(os.path.join(root, fn), UNITY).replace(os.sep, "/")
            if rel not in want:
                orphans.append(rel)
    return orphans


def format_census(entries):
    """{format name: count} over every Texture2D object, for the reconciliation line in main()."""
    try:
        from UnityPy.enums import TextureFormat as TF

        def label(v):
            try:
                return TF(int(v)).name
            except Exception:
                return str(v)
    except Exception:
        label = str
    return collections.Counter(label(o[6]) for o in entries)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--show-orphans", type=int, default=5)
    args = ap.parse_args()

    entries = scan_textures()
    exact, heads = loadable_names()
    plan, skips = plan_names(entries, exact, heads)
    census = format_census(entries)
    print("Texture2D objects in the build: %d, formats %s"
          % (len(entries), ", ".join("%s %d" % kv for kv in sorted(census.items(),
                                                                   key=lambda x: -x[1]))))
    into_res = sum(1 for rel in plan.values() if rel.startswith("Assets/Resources/"))
    suffixed = sum(1 for rel in plan.values() if re.search(r"_\d+\.png$", rel))
    print("planned %d png(s) (%d land in Assets/Resources/ because the code loads them by name,"
          " %d keep a _<path_id> suffix because their source name collides), %d carry no image data"
          % (len(plan), into_res, suffixed, len(skips)))
    for key, why in sorted(skips.items()):
        print("   skip %s (%s, %s): %s" % (key, why["name"], why["size"], why["reason"]))
    if len(plan) + len(skips) != len(entries):
        print("REFUSING: %d objects planned %d + skipped %d, which does not add up"
              % (len(entries), len(plan), len(skips)))
        return 2
    dupes = len(plan) - len(set(plan.values()))
    if dupes:
        print("REFUSING: %d texture(s) would share a path -- the naming rule is not injective, so"
              " one would silently overwrite the other" % dupes)
        return 2

    counts, problems = write_missing(plan, skips, args.dry_run)
    print("written %d, already present %d, no image data %d, decode failures %d"
          % (counts["written"], counts["already present"], counts["no image data"],
             counts["decode failed"]))
    for p in problems[:8]:
        print("   !! " + p)
    if problems:
        print("REFUSING to rewrite the index while %d texture/container problem(s) are open." %
              len(problems))
        return 2
    orphans = report_orphans(plan)
    print("pngs on disk that no planned path claims: %d" % len(orphans))
    for o in orphans[:args.show_orphans]:
        print("   orphan: %s" % o)
    if args.dry_run:
        print("dry run -- index untouched")
        return 0
    for path, doc in ((OUT_INDEX, plan), (OUT_SKIPS, skips)):
        tmp = path + ".tmp"
        with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
            json.dump(doc, fh, indent=1, ensure_ascii=False, sort_keys=True)
        os.replace(tmp, path)
        back = json.load(open(path, encoding="utf-8"))
        if len(back) != len(doc):
            print("MISMATCH: %s re-read as %d entries, planned %d"
                  % (os.path.basename(path), len(back), len(doc)))
            return 2
        print("wrote %s (%d entries, re-read ok)"
              % (os.path.relpath(path, _REPO).replace(os.sep, "/"), len(back)))
    missing = [rel for rel in set(plan.values())
               if not os.path.exists(os.path.join(UNITY, rel.replace("/", os.sep)))]
    if missing:
        print("REFUSING to call this done: %d planned path(s) are not on disk, e.g. %s"
              % (len(missing), missing[0]))
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
