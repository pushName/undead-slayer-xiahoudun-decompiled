"""Read-only: would every Resources.Load in the shipped code actually get something, here?

The referring side decides whether an exported asset is worth anything, and this build asks for
assets by NAME at runtime: Spawn.cs does Resources.Load("bgm_stage" + num), UI_cashshop.cs does
Resources.Load("jewel0" + (n+1) + "a") as Texture2D, Ef_ani_walk.cs does Resources.Load("cc_cha3").

Three separate conditions have to hold, so they are checked separately rather than collapsed into
"does a file with a similar name exist":

  1. a file whose name is EXACTLY the runtime key, because Resources.Load matches on name;
  2. that file is under a `Resources/` folder, because only those are reachable by name at all;
  3. its type is what the call site demands (`as Texture2D`, `as AudioClip`, Instantiate -> GameObject).

Both (1) and (2) are live problems here, and telling them apart decides the fix: export_textures.py
names every file `<source name>_<path_id>.png` unconditionally, so a texture the code asks for as
"jewel01a" sits on disk as "jewel01a_91.png" -- renaming fixes that; whereas Assets/Imported/** is
simply not a Resources folder -- moving fixes that.

The key census is imported from scan_prefabs.py instead of re-grepped, so there is one definition of
what the game asks for. That census already separates whole keys from concatenation heads
("bgm_stage" + num) and records the demanded type per call site. For a head, the runtime names it can
produce are taken from the asset names that start with it, so the check never has to guess numbers.

Case is checked on purpose, and this repository has already mistaken a case difference for a missing
asset once. What the shipped build itself did is now measured rather than assumed: mainData's
resource table stores the keys lower-cased (`cashshop`, `gameno`, `blockname`) while the scripts ask
for "CashShop", "gameNo", "blockName", so the original lookup cannot have been case-sensitive -- see
tools/migration/dump_maindata_resource_index.py. This tool still reports a case difference separately,
because our own file names match the key's exact case and a silent fold would hide it if that ever
stopped being true.

Run:  python tools/migration/check_resources_load_keys.py        (no UnityPy needed)
"""
import argparse
import collections
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from scan_prefabs import call_sites          # the one census of what the game asks for

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ASSETS = os.path.join(_REPO, "unity", "Assets")
OUT = os.path.join(_REPO, "tools", "migration", "resources-load-check.json")

# extension -> the Unity type a call site asking for that type can be satisfied by
EXT_TYPE = {".png": "Texture2D", ".tga": "Texture2D", ".jpg": "Texture2D", ".psd": "Texture2D",
            ".mp3": "AudioClip", ".ogg": "AudioClip", ".wav": "AudioClip",
            ".bytes": "TextAsset", ".txt": "TextAsset", ".mat": "Material",
            ".anim": "AnimationClip", ".fbx": "Mesh"}
# nothing that can satisfy a GameObject/Transform load exists yet: that is task #17's job
PENDING = {"GameObject", "Transform"}


def strip_path_id(stem):
    """`jewel01a_91` -> `jewel01a`; `sk_10_22` -> `sk_10`; `bgm_stage1` -> itself."""
    return stem.rsplit("_", 1)[0] if re.match(r".*_\d+$", stem) else stem


def inventory():
    """-> {exact_stem: rows} plus {source_name: rows}, rows = (type, under Resources, path)."""
    exact, source = collections.defaultdict(list), collections.defaultdict(list)
    for root, dirs, files in os.walk(ASSETS):
        dirs[:] = [d for d in dirs if d not in ("Editor", "Scene", "Plugins", "Packages")]
        for fn in files:
            if fn.startswith(".") or fn.endswith(".meta"):
                continue
            stem, ext = os.path.splitext(fn)
            if ext not in EXT_TYPE:
                continue
            rel = os.path.relpath(os.path.join(root, fn), ASSETS).replace(os.sep, "/")
            row = (EXT_TYPE[ext], "/Resources/" in ("/" + rel), rel)
            exact[stem].append(row)
            src = strip_path_id(stem)
            if src != stem:
                source[src].append(row)
    return exact, source


def classify(key, info, exact, source, names_ci):
    want = set(info["wanted"]) - {"any"}
    pending_only = bool(want) and want <= PENDING
    # a head's runtime names are the names the game can actually build, i.e. the un-suffixed source
    # names; taking file stems instead would invent names like "extraimg1001_1" that no call site
    # ever asks for, and would then report a mangled file as merely being in the wrong folder
    src_names = set(source) | {n for n in exact if strip_path_id(n) == n}
    if info["head"]:
        cands = sorted(n for n in src_names if n.startswith(key) and n != key)
    else:
        cands = [key]
    hits = []
    for n in cands:
        rows = exact.get(n) or [(t, u, p) for t, u, p in source.get(n, [])]
        if not want or any(t in want for t, _u, _p in rows):
            hits.append((n, rows))
    # "loadable" means a file whose stem IS the runtime name, sitting under a Resources folder
    loadable = [(n, exact[n]) for n, _r in hits
                if exact.get(n) and any(u for _t, u, _p in exact[n])]
    from_source = [n for n, _r in hits if not exact.get(n)]

    if loadable:
        return "OK (%d file(s) loadable by that exact name)" % len(loadable), loadable
    if hits and not loadable and from_source:
        return ("NAME-MANGLED (%d name(s) only exist as <name>_<path_id>.png, e.g. %s)"
                % (len(from_source), from_source[0]), hits)
    if hits and not loadable:
        return "WRONG-FOLDER (%d exact-name file(s), none under a Resources/ folder)" % len(hits), hits
    if not hits and not info["head"]:
        # an exact-name file may be absent while the un-suffixed source name is present: that is
        # mangling, not a missing asset, and it is a different fix (rename, not export)
        if source.get(key):
            return ("NAME-MANGLED (only %s on disk, never %s.png)"
                    % (os.path.basename(source[key][0][2]), key), [(key, source[key])])
        ci = names_ci.get(key.lower())
        if ci and ci.lower() != key.lower():
            return "CASE-ONLY (on disk as %s)" % ci, []
        if pending_only:
            return "PREFAB PENDING #17 (no prefab named %s exists yet)" % key, []
        return "MISSING (nothing in the project is named %s)" % key, []
    if info["head"] and pending_only:
        return "PREFAB PENDING #17 (no prefab starts with %s)" % key, []
    return "MISSING (no asset name starts with %s)" % key, []


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", default=OUT)
    ap.add_argument("--show", type=int, default=16, help="keys listed per verdict")
    args = ap.parse_args()
    sites = call_sites()
    exact, source = inventory()
    names_ci = {n.lower(): n for n in list(exact) + list(source)}
    print("files on disk: %d exact name(s), of which %d also readable by their un-suffixed source name"
          % (len(exact), len(source)))
    print("literal keys in the shipped scripts: %d" % len(sites))

    tally = collections.Counter()
    verdicts = {}
    for key in sorted(sites):
        v, hits = classify(key, sites[key], exact, source, names_ci)
        tally[v.split(" (")[0]] += 1
        verdicts[key] = {"verdict": v, "head": sites[key]["head"],
                         "wanted": sorted(set(sites[key]["wanted"])), "sites": sites[key]["sites"],
                         "names": sorted({n for n, _r in hits})[:12],
                         "paths": sorted({p for _n, r in hits for _t, _u, p in r})[:3],
                         "example": sites[key]["examples"][:1]}

    print("\n=== verdicts ===")
    for v, n in tally.most_common():
        print("  %-22s %d" % (v, n))
    for want in ["MISSING", "CASE-ONLY", "NAME-MANGLED", "WRONG-FOLDER",
                 "PREFAB PENDING #17", "OK"]:
        ks = sorted(k for k, d in verdicts.items() if d["verdict"].startswith(want))
        if not ks:
            continue
        print("\n--- %s: %d" % (want, len(ks)))
        for k in ks[:args.show]:
            d = verdicts[k]
            print("   %-22s %-58s e.g. %s" % (k, d["verdict"], (d["names"] or [""])[0]))
            if want in ("MISSING", "CASE-ONLY", "NAME-MANGLED", "WRONG-FOLDER") and d["example"]:
                print("        call site: %s" % d["example"][0][:118])
    loadable = tally["OK"]
    print("\n--- 汇总：%d / %d 个键现在真取得到；%d 个等 #17 的 prefab；%d 个卡在名字或目录"
          % (loadable, len(sites), tally["PREFAB PENDING #17"],
             tally["NAME-MANGLED"] + tally["WRONG-FOLDER"] + tally["MISSING"] + tally["CASE-ONLY"]))
    with open(args.json, "w", encoding="utf-8", newline="\n") as fh:
        json.dump({"tally": dict(tally), "verdicts": verdicts}, fh, indent=1, ensure_ascii=False)
    print("wrote %s" % args.json)
    return 0


if __name__ == "__main__":
    sys.exit(main())
