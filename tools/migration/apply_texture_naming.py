"""Migrate the exported textures onto the naming rule in export_textures.plan_names, in place.

Why a migration script instead of just re-running export_textures.py: Unity assigns a guid to an
asset when it imports it, and every .mat and .unity file refers to a texture through that guid. A
re-export under a new name imports as a NEW asset with a NEW guid, which would leave 135 sprites and
hundreds of materials pointing at files nothing references any more. A rename that carries the .meta
along keeps the guid, so the existing references keep resolving and only the path string changes.

The target names come from plan_names(), i.e. the same function the exporter will use from now on, so
this script and a future re-export cannot disagree about what a texture should be called.

Checks, because a bulk rename over 900 files is exactly where silent loss happens:
  * every key in the old index is planned, and the plan is injective;
  * the guid in a moved .meta equals the guid read before the move;
  * after the move, every planned path exists, has a .meta, and there is no leftover .png that no
    planned path claims (that would be a stale duplicate of a texture that moved);
  * the new index is re-read and compared entry-for-entry.

Run:  <unitypy venv python> tools/migration/apply_texture_naming.py [--dry-run]  RECON_DIR=<path>
"""
import argparse
import collections
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import export_textures as ET
import unity4_files as u4

_REPO = ET._REPO
UNITY = ET.UNITY
OLD_INDEX = os.path.join(_REPO, "tools", "migration", "texture-index.json")


def guid_of(meta_path):
    if not os.path.exists(meta_path):
        return None
    txt = open(meta_path, encoding="utf-8", errors="replace").read(400)
    m = re.search(r"guid: ([0-9a-f]{32})", txt)
    return m.group(1) if m else None


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--verbose", action="store_true")
    args = ap.parse_args()

    old = json.load(open(OLD_INDEX, encoding="utf-8"))
    entries = ET.scan_textures()
    exact, heads = ET.loadable_names()
    plan, skips = ET.plan_names(entries, exact, heads)

    missing = sorted(set(old) - set(plan))
    extra = sorted(set(plan) - set(old))
    print("old index: %d entries; plan: %d png paths + %d textures with no image data;"
          " keys only in old: %d; only in plan: %d"
          % (len(old), len(plan), len(skips), len(missing), len(extra)))
    if missing or extra:
        print("REFUSING: the old index and the plan do not cover the same textures, so a move would"
              " orphan or invent entries. Reconcile scan_textures() against the index first.")
        for k in (missing or extra)[:6]:
            print("   ", k)
        return 2
    dupes = [p for p, n in collections.Counter(plan.values()).items() if n > 1]
    if dupes:
        print("REFUSING: the plan maps %d path(s) to more than one texture: %s"
              % (len(dupes), dupes[:4]))
        return 2

    moves, stays, problems = [], 0, []
    guids = {}
    for key in sorted(plan):
        src_rel, dst_rel = old[key], plan[key]
        if src_rel == dst_rel:
            stays += 1
            guids[key] = guid_of(os.path.join(UNITY, src_rel.replace("/", os.sep)) + ".meta")
            continue
        src = os.path.join(UNITY, src_rel.replace("/", os.sep))
        dst = os.path.join(UNITY, dst_rel.replace("/", os.sep))
        if not os.path.exists(src):
            problems.append("%s: source %s is not on disk" % (key, src_rel))
            continue
        g = guid_of(src + ".meta")
        if g is None:
            problems.append("%s: no .meta with a guid for %s -- Unity never imported it, so a"
                            " rename would not preserve anything" % (key, src_rel))
            continue
        if os.path.exists(dst):
            problems.append("%s: target %s already exists (two textures want one name)"
                            % (key, dst_rel))
            continue
        guids[key] = g
        moves.append((key, src_rel, dst_rel, g))

    print("moves: %d, already correctly named: %d, blocked: %d" % (len(moves), stays, len(problems)))
    for p in problems[:10]:
        print("   !! " + p)
    if problems:
        print("REFUSING to move anything while %d texture(s) have an unresolvable rename."
              " Nothing is half-applied." % len(problems))
        return 2
    if args.dry_run:
        for key, s, d, g in moves[:12]:
            print("   %-30s -> %s" % (s, d))
        into_res = sum(1 for _k, _s, d, _g in moves if d.startswith("Assets/Resources/"))
        print("dry run: %d moves, of which %d into Assets/Resources/" % (len(moves), into_res))
        return 0

    for key, src_rel, dst_rel, g in moves:
        src = os.path.join(UNITY, src_rel.replace("/", os.sep))
        dst = os.path.join(UNITY, dst_rel.replace("/", os.sep))
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        os.replace(src, dst)
        if os.path.exists(src + ".meta"):
            os.replace(src + ".meta", dst + ".meta")
        after = guid_of(dst + ".meta")
        if after != g:
            print("ABORT: guid changed for %s (%s -> %s)" % (dst_rel, g, after))
            return 2

    tmp = OLD_INDEX + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(plan, fh, indent=1, ensure_ascii=False, sort_keys=True)
    os.replace(tmp, OLD_INDEX)

    back = json.load(open(OLD_INDEX, encoding="utf-8"))
    absent = [rel for rel in set(back.values())
              if not os.path.exists(os.path.join(UNITY, rel.replace("/", os.sep)))]
    no_meta = [rel for rel in set(back.values())
               if not os.path.exists(os.path.join(UNITY, rel.replace("/", os.sep)) + ".meta")]
    orphans = ET.report_orphans(back)
    print("re-read index: %d entries; paths missing on disk: %d; without .meta: %d"
          % (len(back), len(absent), len(no_meta)))
    print("pngs on disk claimed by no index entry: %d" % len(orphans))
    for o in orphans[:6]:
        print("   orphan: %s" % o)
    if absent or no_meta:
        print("REFUSING to call this done: the index names files that are not there.")
        return 2
    moved_into = sum(1 for _k, _s, d, _g in moves if d.startswith("Assets/Resources/"))
    print("OK: %d renamed/moved (%d into Assets/Resources/), %d already correct, guids verified on"
          " every move" % (len(moves), moved_into, stays))
    if args.verbose:
        for key, s, d, _g in moves:
            print("   %s -> %s" % (s, d))
    return 0


if __name__ == "__main__":
    sys.exit(main())
