"""Read-only inventory of the animation data, before anything is exported.

What has to be established before writing an exporter
1. Is the AnimationClip parse real? Unity 4 player builds carry no TypeTree, so read() can
   return an object whose fields are misaligned garbage. Plausibility is checked per clip
   (sample rate in a sane set, at least one curve, non-empty printable name, increasing times)
   and the pass rate is reported -- "it did not raise" is not evidence.
2. Where does the LEGACY flag live? 2022 has AnimationClip.legacy; Unity 4.5 serialized
   m_AnimationType instead, and UnityPy exposes m_Legacy as None on these objects. This prints
   both so the exporter does not silently assume.
3. Do the names the shipped scripts ask for actually exist? 253 call sites do
   animation.Play("x") / myanimation["x"].speed / GetClip("x"). A clip list that does not
   contain those names cannot animate anything no matter how faithfully it is exported, so the
   overlap is the go/no-go for task #14.
4. How does an Animation component in a level reach its clips? Legacy Animation resolves names
   through the model's AnimationContainer, so the container -> clip bindings are counted; if
   they are not resolvable, the exporter has to bind by name at rebuild time instead.

Run:  <unitypy venv python> tools/migration/scan_animations.py
"""
import collections
import json
import os
import re
import struct
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS = os.path.join(_REPO, "unity", "Assets", "Scripts")
DUMP = u4.DUMP
RECON = u4.RECON

# Animation APIs that take a clip/state name as a string literal. Stop() and Rewind() take no
# name in Unity's API, so they are not matched -- including them manufactured fake clip names.
NAME_USE = re.compile(
    r'(?:Play|PlayQueued|CrossFade|CrossFadeQueued|SampleAnimation|IsPlaying)\s*\(\s*"([^"]+)"'
    r'|(?:\banimation\b|\bmyanimation\b|\banim\w*|AnimationState\w*)\s*(?:\[\s*)?"([^"]+)"\s*\]'
    r'|GetClip\s*\(\s*"([^"]+)"')


def requested_names():
    """name -> Counter of the API that used it, from every script in the tree."""
    out = collections.Counter()
    api = collections.defaultdict(collections.Counter)
    for root, _dirs, files in os.walk(SCRIPTS):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            with open(os.path.join(root, fn), encoding="utf-8", errors="replace") as fh:
                text = fh.read()
            for m in NAME_USE.finditer(text):
                name = next((g for g in m.groups() if g), None)
                if name:
                    out[name] += 1
                    head = text[max(0, m.start() - 60):m.start()]
                    who = re.findall(r'(\w+(?:Animation|animation|anim)?)\s*$|', head)
                    api[name][(who[0] if who and who[0] else "?") + ":" + (m.group(0).split("(")[0].strip())] += 1
    return out, api


def curve_stats(d):
    """-> (per-list lengths, total keyframes, last key time, curve target paths)."""
    lens = {}
    keys = 0
    last_t = 0.0
    paths = set()
    for attr in ("m_PositionCurves", "m_RotationCurves", "m_ScaleCurves", "m_EulerCurves",
                 "m_CompressedRotationCurves", "m_FloatCurves", "m_PPtrCurves"):
        lst = getattr(d, attr, None) or []
        lens[attr] = len(lst)
        for c in lst:
            kf = getattr(getattr(c, "curve", None), "m_Curve", None) or []
            keys += len(kf)
            if kf:
                last_t = max(last_t, float(kf[-1].time))
            p = getattr(c, "path", None)
            if p:
                paths.add(str(p))
    return lens, keys, last_t, paths


def main():
    print("RECON=%s (%s)  DUMP=%s" % (RECON, os.path.isdir(RECON), DUMP))

    type_hist = collections.Counter()
    clips = []            # dicts
    containers = []       # dicts
    anim_comps = []       # dicts (level files only)
    mono_by_level = {}
    load_errors = []

    def handle(path, is_level, target):
        try:
            env = UnityPy.load(path)
        except Exception as e:
            load_errors.append("%s: %s" % (target, e))
            return
        objs = {o.path_id: o for o in env.objects}
        for o in env.objects:
            tn = o.type.name
            type_hist[tn] += 1
            if tn == "AnimationClip":
                d = o.read()
                lens, keys, last_t, paths = curve_stats(d)
                name = str(getattr(d, "m_Name", ""))
                clips.append({
                    "file": target, "path_id": o.path_id, "name": name,
                    "sample_rate": float(getattr(d, "m_SampleRate", 0) or 0),
                    "wrap": getattr(d, "m_WrapMode", None),
                    "anim_type": getattr(d, "m_AnimationType", "<none>"),
                    "legacy_attr": getattr(d, "m_Legacy", "<none>"),
                    "lens": lens, "keys": keys, "last_t": last_t,
                    "paths": sorted(paths),
                })
            elif tn == "AnimationContainer":
                d = o.read()
                entries = []
                for e in (getattr(d, "m_Clips", None) or []):
                    first = getattr(e, "m_First", None)
                    second = getattr(e, "m_Second", None)
                    entries.append({
                        "name": str(getattr(first, "name", first) if first is not None else "?"),
                        "clip": int(getattr(second, "m_PathID", 0) or 0),
                        "clip_file": int(getattr(second, "m_FileID", 0) or 0),
                    })
                containers.append({"file": target, "path_id": o.path_id, "clips": entries,
                                   "is_level": is_level})
            elif tn == "Animation":
                d = o.read()
                go = int(getattr(getattr(d, "m_GameObject", None), "m_PathID", 0) or 0)
                clip = getattr(d, "m_Clip", None)
                anim_comps.append({
                    "file": target, "path_id": o.path_id, "go": go,
                    "clip": int(getattr(clip, "m_PathID", 0) or 0),
                    "clip_file": int(getattr(clip, "m_FileID", 0) or 0) if clip else 0,
                    "culling": getattr(d, "m_CullingType", None),
                    "animate_physics": getattr(d, "m_AnimatePhysics", None),
                })

    # NOT os.listdir(RECON): the APK ships some sharedassets files whole and some split, so
    # RECON holds only the 12 that needed reassembly while dump/assets holds the other 12
    # (sharedassets3/17/19 carry real clips) plus the hash-named bundles. sources() is the one
    # place that rule is encoded; iterating RECON produced 409 clips instead of 427 and made four
    # animation names look absent when they were simply in a file never opened.
    for base, path in u4.sources():
        handle(path, base.startswith("level"), base)

    print("\n=== type histogram (animation-relevant) ===")
    for tn in ("AnimationClip", "AnimationContainer", "Animation", "SkinnedMeshRenderer",
               "MonoBehaviour", "Avatar", "Animator"):
        print("  %-22s %d" % (tn, type_hist.get(tn, 0)))
    other = {k: v for k, v in type_hist.items() if v > 0 and k not in
             ("AnimationClip", "AnimationContainer", "Animation", "SkinnedMeshRenderer",
              "MonoBehaviour", "GameObject", "Transform") and k not in
             ("MeshRenderer", "MeshFilter", "Mesh", "Material", "Texture2D", "Shader",
              "AudioSource", "AudioClip", "Camera", "Light", "Collider", "BoxCollider",
              "SphereCollider", "CapsuleCollider", "Rigidbody", "TextAsset")}
    print("  (other types present: %s)" % ", ".join("%s x%d" % kv for kv in
                                                    sorted(other.items(), key=lambda x: -x[1])[:14]))
    if load_errors:
        print("  LOAD ERRORS: %s" % load_errors)

    print("\n=== AnimationClip parse plausibility (%d clips) ===" % len(clips))
    bad_name = [c for c in clips if not c["name"] or not re.match(r"^[\x20-\x7e]{1,64}$", c["name"])]
    bad_sr = [c for c in clips if c["sample_rate"] not in (7.5, 10, 15, 20, 24, 30, 60)]
    no_keys = [c for c in clips if c["keys"] == 0]
    short = [c for c in clips if c["last_t"] <= 0.05]
    print("  printable non-empty name : %d/%d  (bad %d)" % (len(clips) - len(bad_name), len(clips),
                                                             len(bad_name)))
    print("  sample rate in sane set  : %d/%d  (odd %d) values=%s"
          % (len(clips) - len(bad_sr), len(clips), len(bad_sr),
             sorted({c["sample_rate"] for c in clips})))
    print("  at least one keyframe    : %d/%d  (empty %d)" % (len(clips) - len(no_keys), len(clips),
                                                              len(no_keys)))
    print("  clip length > 0.05s      : %d/%d  (tiny %d)" % (len(clips) - len(short), len(clips),
                                                             len(short)))
    print("  m_AnimationType values   : %s" % collections.Counter(str(c["anim_type"]) for c in clips))
    print("  m_Legacy attribute       : %s" % collections.Counter(str(c["legacy_attr"]) for c in clips))
    print("  m_WrapMode values        : %s" % collections.Counter(str(c["wrap"]) for c in clips))
    kinds = collections.Counter()
    for c in clips:
        for k, n in c["lens"].items():
            if n:
                kinds[k] += 1
    print("  curve lists in use       : %s" % dict(kinds))
    print("  distinct clip names      : %d" % len({c["name"] for c in clips}))
    dupes = [n for n, k in collections.Counter(c["name"] for c in clips).items() if k > 1]
    print("  names appearing >1x      : %d (e.g. %s)" % (len(dupes), dupes[:8]))

    print("\n=== curve target paths (the only handle for binding Animation -> clips) ===")
    no_path = [c for c in clips if not c["paths"]]
    print("  clips with no Transform path at all: %d/%d" % (len(no_path), len(clips)))
    all_paths = collections.Counter(p for c in clips for p in c["paths"])
    print("  distinct target paths : %d, e.g. %s"
          % (len(all_paths), [p for p, _n in all_paths.most_common(12)]))
    print("  clips per file        : %s" % collections.Counter(c["file"] for c in clips).most_common(8))
    sizes = collections.Counter(len(c["paths"]) for c in clips)
    print("  path-set sizes        : %s" % sorted(sizes.items())[:10])
    # A clip whose paths all start with the same root prefix is bound to that skeleton root;
    # print how the path sets cluster so the binding rule can be chosen on data.
    sigs = collections.Counter(tuple(c["paths"][:1]) for c in clips)
    print("  most common first-path: %s" % sigs.most_common(10))

    print("\n=== AnimationContainer (%d found) ===" % len(containers))
    if containers:
        tot = sum(len(x["clips"]) for x in containers)
        named = sum(1 for x in containers for e in x["clips"] if e["name"] and e["name"] != "?")
        ptr = sum(1 for x in containers for e in x["clips"] if e["clip"])
        print("  entries total %d, with a usable name %d, with a non-zero clip ptr %d"
              % (tot, named, ptr))
        print("  sample: %s" % containers[0]["clips"][:6])
    else:
        print("  none -- legacy name resolution must be rebuilt another way")

    print("\n=== Animation components in levels (%d) ===" % len(anim_comps))
    by_file = collections.Counter(a["file"] for a in anim_comps)
    print("  per level: %s" % dict(by_file.most_common(8)))
    zero_clip = sum(1 for a in anim_comps if not a["clip"])
    print("  with m_Clip == null: %d/%d" % (zero_clip, len(anim_comps)))
    cross_file = sum(1 for a in anim_comps if a["clip_file"])
    print("  m_Clip pointing at ANOTHER file (FileID != 0): %d -- those need the externals "
          "table to resolve" % cross_file)

    req, api = requested_names()
    have = {c["name"] for c in clips}
    print("\n=== referring side: names the scripts pass to the animation API ===")
    print("  distinct names used: %d, call sites: %d" % (len(req), sum(req.values())))
    missing = sorted(n for n in req if n not in have)
    present = sorted(n for n in req if n in have)
    print("  present in the clip table : %d" % len(present))
    print("  MISSING from clip table   : %d" % len(missing))
    print("  present: %s" % present[:40])
    print("  missing: %s" % missing[:40])
    if missing:
        print("  sample missing usage: %s" % {m: dict(api[m]) for m in missing[:5]})
    print("\n  go/no-go for #14: %d/%d of the names the game asks for exist in the assets"
          % (len(present), len(req)))

    out = {
        "clips": [{k: v for k, v in c.items() if k != "lens"} for c in clips],
        "containers": containers,
        "animation_components": anim_comps,
        "requested_names": dict(req),
        "names_present": present,
        "names_missing": missing,
    }
    path = os.path.join(_REPO, "tools", "migration", "animations-survey.json")
    with open(path, "w", encoding="utf-8") as fh:
        json.dump(out, fh, indent=1)
    print("wrote %s (%d clips, %d containers, %d Animation components)"
          % (path, len(clips), len(containers), len(anim_comps)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
