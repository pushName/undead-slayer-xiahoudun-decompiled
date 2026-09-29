"""Read-only: are those four clip names really absent, or did I only fail to look?

The claim under test
    "ride2 / getoff_horse / idle_ridestage / richecoin exist in none of the 409 AnimationClips"

Why it is suspect
The scan that produced those 409 clips loaded only the 12 reassembled sharedassets files plus the
23 level files. dump/assets also holds a few hundred hash-named AssetBundles (and
'unity default resources'), which tools/migration/unity4_files.py already knows to include in
sources() -- so the candidate set was not exhaustive, and a negative conclusion needs the stronger
proof.

Three independent checks, run together
1. BYTE SEARCH over every file in dump/assets and recon for the literal name strings. This does
   not care about parsing, case-mangled renames, or which container a clip lives in.
2. POSITIVE CONTROLS in the same byte pass: names already known to parse as clips (fly_flap,
   idle, horse_run). If a control came back "not found" the scan itself would be broken, and a
   negative result on the four suspects would mean nothing.
3. FULL-SCOPE UnityPy enumeration via u4.sources() -- every loadable serialized file, fragments
   excluded by the shared helper -- reporting AnimationClip counts and names per container.

Also checked, because the same doubt applies to the "15 copies of idle" finding: whether those
same-named clips are 15 distinct animations or one clip counted repeatedly. The curve payload is
hashed, so identical duplicates collapse and the distinct count is measured, not assumed.

Run:  <unitypy venv python> tools/migration/scan_clip_coverage.py
"""
import collections
import hashlib
import json
import os
import re
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

SUSPECT = ["ride2", "getoff_horse", "idle_ridestage", "risecoin"]
CONTROLS = ["fly_flap", "horse_run", "idle", "walk_free", "costume"]

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP = u4.DUMP
RECON = u4.RECON

# Substrings that would show the name exists but was stored differently: a case change, a
# separator, or a suffix our earlier export steps are known to add.
FRAGMENTS = {
    "ride2": ["ride2", "ride_2", "Ride2", "ride2_", "rideStage", "idle_ride", "getoff", "GetOff",
              "risecoin", "riseCoin", "coin", "idle_ridestage", "ridestage", "ride_stage"],
}


def byte_pass(paths, needles):
    """name -> list of files whose raw bytes contain it (ASCII and UTF-16LE both)."""
    hits = collections.defaultdict(list)
    encoded = {}
    for n in needles:
        encoded[n] = [n.encode("ascii"), n.encode("utf-16-le")]
    for name, path in paths:
        try:
            with open(path, "rb") as fh:
                blob = fh.read()
        except OSError as e:
            print("  unreadable %s: %s" % (name, e))
            continue
        for n, encs in encoded.items():
            if any(e in blob for e in encs):
                hits[n].append(name)
    return hits


def all_files():
    """Every candidate file, fragments excluded by the shared helper's rule."""
    out = []
    seen = set()
    for base in (DUMP, RECON):
        if not os.path.isdir(base):
            continue
        for fn in sorted(os.listdir(base)):
            p = os.path.join(base, fn)
            if not os.path.isfile(p) or ".split" in fn:
                continue
            if fn in seen:
                continue
            seen.add(fn)
            out.append((fn, p))
    return out


def curve_signature(d):
    """Stable hash over everything that makes one clip different from another."""
    h = hashlib.md5()
    h.update(str(getattr(d, "m_Name", "")).encode("utf-8"))
    h.update(b"%r" % (getattr(d, "m_SampleRate", None),))
    for attr in ("m_PositionCurves", "m_RotationCurves", "m_ScaleCurves", "m_EulerCurves",
                 "m_FloatCurves"):
        lst = getattr(d, attr, None) or []
        h.update(("%s|%d;" % (attr, len(lst))).encode())
        for c in lst:
            path = str(getattr(c, "path", ""))
            kf = getattr(getattr(c, "curve", None), "m_Curve", None) or []
            h.update(("%s:%d:" % (path, len(kf))).encode())
            for k in kf:
                h.update(repr(getattr(k, "value", None)).encode())
                h.update(repr(getattr(k, "time", None)).encode())
    return h.hexdigest()


def main():
    files = all_files()
    needles = SUSPECT + CONTROLS
    print("=== 1+2. byte search over %d files (fragments excluded), with positive controls ==="
          % len(files))
    hits = byte_pass(files, needles)
    for n in CONTROLS:
        print("  control   %-16s found in %d file(s)%s"
              % (n, len(hits.get(n, [])), "" if hits.get(n) else "   <-- SCAN IS BROKEN"))
    for n in SUSPECT:
        print("  suspect   %-16s found in %d file(s)  %s"
              % (n, len(hits.get(n, [])), hits.get(n, [])[:8]))

    print("\n=== 2b. near-miss fragments (was the name renamed / cased differently?) ===")
    frag_needles = sorted({f for v in FRAGMENTS.values() for f in v})
    fh_hits = byte_pass(files, frag_needles)
    for f in frag_needles:
        print("  %-16s %2d file(s)  %s" % (f, len(fh_hits.get(f, [])), fh_hits.get(f, [])[:6]))

    print("\n=== 3. full-scope UnityPy enumeration over u4.sources() ===")
    srcs = u4.sources()
    print("  sources() returns %d loadable files" % len(srcs))
    per_file = {}
    load_fail = []
    name_index = collections.defaultdict(list)
    sig_by_name = collections.defaultdict(set)
    for base, path in srcs:
        try:
            env = UnityPy.load(path)
        except Exception as e:
            load_fail.append((base, type(e).__name__))
            continue
        n = 0
        for o in env.objects:
            if o.type.name != "AnimationClip":
                continue
            try:
                d = o.read()
            except Exception:
                continue
            nm = str(getattr(d, "m_Name", ""))
            per_file[base] = per_file.get(base, 0) + 1
            name_index[nm].append(base)
            sig_by_name[nm].add(curve_signature(d))
            n += 1
    total = sum(per_file.values())
    print("  AnimationClip total across ALL containers: %d in %d files" % (total, len(per_file)))
    print("  load failures: %d %s" % (len(load_fail), collections.Counter(
        k for _b, k in load_fail).most_common(4)))
    print("  containers contributing clips: %s" % sorted(per_file.items(), key=lambda x: -x[1]))

    print("\n=== names requested by the scripts, re-checked against the FULL table ===")
    survey_path = os.path.join(_REPO, "tools", "migration", "animations-survey.json")
    if os.path.exists(survey_path):
        survey = json.load(open(survey_path, encoding="utf-8"))
        req_missing = [n for n in survey["names_missing"]]
        full = set(name_index)
        still = [n for n in req_missing if n not in full]
        found_now = [n for n in req_missing if n in full]
        print("  previously called missing: %d" % len(req_missing))
        print("  now present in the full table (so my earlier scope was too narrow): %s"
              % found_now)
        print("  still absent everywhere: %s" % still)
        print("  NOTE: that earlier list mixed in JSON indexer keys (obj[\"data\"]); the four "
              "names in question here are %s" % SUSPECT)
    for n in SUSPECT + CONTROLS:
        print("  %-16s clips with this name: %d   distinct curve payloads: %d   files: %s"
              % (n, len(name_index.get(n, [])), len(sig_by_name.get(n, set())),
                 sorted(set(name_index.get(n, [])))[:6]))

    print("\n=== is 'idle' really 15 different animations? ===")
    for n in ("idle", "move", "b_attack1", "down"):
        files_n = name_index.get(n, [])
        print("  %-10s occurrences %2d, distinct payloads %2d, per-file %s"
              % (n, len(files_n), len(sig_by_name.get(n, set())),
                 dict(collections.Counter(files_n))))
    return 0


if __name__ == "__main__":
    sys.exit(main())
