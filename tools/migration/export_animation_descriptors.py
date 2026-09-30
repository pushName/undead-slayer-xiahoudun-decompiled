"""Export every Unity 4 AnimationClip as a per-clip descriptor the Editor can rebuild from.

Why descriptors instead of .anim files
Unity owns the .anim serialization (AnimationCurve keyframe layout, m_EditorCurves vs runtime
curves, the serializedVersion block, the binding constant). Writing that YAML from memory yields
files that import while silently dropping data, so this emits a DESCRIPTION and
unity/Assets/Editor/AnimationClipBuilder.cs lets AnimationClip/SetCurve +
AssetDatabase.CreateAsset produce the real bytes -- the same split that made the scene rebuild
verifiable.

Coverage rule (this bit me once already)
Containers come from unity4_files.sources(). Enumerating %TEMP%\\recon instead finds only the 12
sharedassets files that needed reassembling and silently loses sharedassets3/17/19 and the
hash-named bundles -- that is how "4 animation state names do not exist" was produced as a false
conclusion.

What is deliberately NOT guessed
  * legacy flag: Unity 4 stored it as m_AnimationType; every clip here reads 1, and 2022 requires
    AnimationClip.legacy for the Animation component to play a clip at all. The descriptor records
    the source value AND the export decision separately so the two can never be confused.
  * animation events with a non-null objectReferenceParameter: recorded with the raw PPtr and
    listed under "unresolved" rather than mapped to a guess.
  * curve lists this exporter cannot represent (compressed rotation, PPtr curves): it refuses to
    drop them silently and fails the run instead.

Run:  <unitypy venv python> tools/migration/export_animation_descriptors.py
Out:  tools/migration/anim_clips/<container>_<pathid>.json  (generated, not committed)
      tools/migration/animation-index.json                  (committed: the binding table)

Key naming is constrained by the reader
The Editor side parses these with JsonUtility (the established route in this project -- see
SceneRebuilder), which binds by exact field name and cannot handle dictionaries or the C#
keywords "in"/"float". So curve families are "position"/"rotation"/"scale"/"float_curves" and a
keyframe is "t"/"value"/"in_slope"/"out_slope".
"""
import collections
import json
import os
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT_DIR = os.path.join(_REPO, "tools", "migration", "anim_clips")
INDEX = os.path.join(_REPO, "tools", "migration", "animation-index.json")

# Unity 4 WrapMode values seen in this build: 0 Default, 1 Clamp, 2 Loop, 8 ClampForever.
WRAP_NAMES = {0: "Default", 1: "Clamp", 2: "Loop", 4: "PingPong", 7: "Once", 8: "ClampForever"}


def _v(x):
    """Vector3f / Quaternion -> plain list of floats, full round-trip precision."""
    if x is None:
        return None
    if hasattr(x, "x"):
        out = [float(x.x), float(x.y), float(x.z)]
        if hasattr(x, "w"):
            out.append(float(x.w))
        return out
    return float(x)


def keys_of(curve):
    return [{
        "t": float(k.time),
        "value": _v(getattr(k, "value", None)),
        "in_slope": _v(getattr(k, "inSlope", None)),
        "out_slope": _v(getattr(k, "outSlope", None)),
    } for k in (getattr(curve, "m_Curve", None) or [])]


def describe(d):
    """AnimationClip object -> descriptor dict, or raises so nothing is half-written."""
    pos = []
    rot = []
    sca = []
    flt = []
    for c in (getattr(d, "m_PositionCurves", None) or []):
        pos.append({"path": str(c.path), "keys": keys_of(c.curve)})
    for c in (getattr(d, "m_RotationCurves", None) or []):
        rot.append({"path": str(c.path), "keys": keys_of(c.curve)})
    for c in (getattr(d, "m_ScaleCurves", None) or []):
        sca.append({"path": str(c.path), "keys": keys_of(c.curve)})
    for c in (getattr(d, "m_FloatCurves", None) or []):
        flt.append({"path": str(c.path), "attribute": str(c.attribute),
                    "class_id": int(c.classID), "keys": keys_of(c.curve)})

    # Refuse to silently lose a curve family this exporter cannot express.
    for attr in ("m_CompressedRotationCurves", "m_PPtrCurves", "m_EulerCurves"):
        lst = getattr(d, attr, None) or []
        if lst:
            raise RuntimeError("%s holds %d curve(s) this exporter does not represent"
                               % (attr, len(lst)))

    events = []
    unresolved = []
    for e in (getattr(d, "m_Events", None) or []):
        orp = getattr(e, "objectReferenceParameter", None)
        fid = int(getattr(orp, "m_FileID", 0) or 0) if orp else 0
        pid = int(getattr(orp, "m_PathID", 0) or 0) if orp else 0
        rec = {"time": float(e.time), "function": str(e.functionName),
               "data": str(e.data), "float": float(e.floatParameter),
               "int": int(e.intParameter), "options": int(e.messageOptions)}
        if fid or pid:
            rec["object_reference"] = {"file_id": fid, "path_id": pid}
            unresolved.append("event '%s' at %.4fs references object %d!%d -- not mapped"
                              % (rec["function"], rec["time"], fid, pid))
        events.append(rec)

    all_keys = [k for lst in (pos, rot, sca, flt) for c in lst for k in c["keys"]]
    return {
        "name": str(d.m_Name),
        "sample_rate": float(d.m_SampleRate),
        "wrap_mode": int(d.m_WrapMode),
        "wrap_name": WRAP_NAMES.get(int(d.m_WrapMode), "unknown %d" % int(d.m_WrapMode)),
        "source_animation_type": int(getattr(d, "m_AnimationType", 0) or 0),
        "source_legacy_attribute": getattr(d, "m_Legacy", None),
        # 2022's Animation component only plays clips flagged legacy; the source value is kept
        # above so this decision stays visible instead of masquerading as a recovered fact.
        "export_legacy": True,
        "length": max([k["t"] for k in all_keys] or [0.0]),
        "counts": {"position": len(pos), "rotation": len(rot), "scale": len(sca),
                   "float_curves": len(flt), "events": len(events), "keyframes": len(all_keys)},
        "curves": {"position": pos, "rotation": rot, "scale": sca, "float_curves": flt},
        "events": sorted(events, key=lambda r: r["time"]),
        "unresolved": unresolved,
    }


def slug(file):
    return file.replace(".assets", "").replace(".", "_")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    srcs = u4.sources()
    print("containers from sources(): %d" % len(srcs))

    written = 0
    keyframes = 0
    events = 0
    per_container = collections.Counter()
    name_count = collections.Counter()
    index = []
    failures = []

    for base, path in srcs:
        try:
            env = UnityPy.load(path)
        except Exception as e:
            failures.append("%s: load %s" % (base, e))
            continue
        for o in env.objects:
            if o.type.name != "AnimationClip":
                continue
            try:
                d = o.read()
                desc = describe(d)
            except Exception as e:
                failures.append("%s!%d: %s: %s" % (base, o.path_id, type(e).__name__, e))
                continue
            cid = "%s@%d" % (base, o.path_id)
            fname = "%s_%d.json" % (slug(base), o.path_id)
            fpath = os.path.join(OUT_DIR, fname)
            payload = dict(desc)
            payload["id"] = cid
            payload["container"] = base
            payload["path_id"] = o.path_id
            payload["asset_path"] = "Assets/Imported/Animations/%s_%d_%s.anim" % (
                "".join(ch if (ch.isalnum() or ch in "_-") else "_" for ch in desc["name"]),
                o.path_id, slug(base))
            blob = json.dumps(payload, sort_keys=True, indent=1)
            if os.path.exists(fpath):
                with open(fpath, encoding="utf-8") as fh:
                    if fh.read() != blob:
                        failures.append("%s: existing descriptor differs, refusing to overwrite"
                                        % fname)
                        continue
            else:
                with open(fpath, "w", encoding="utf-8") as fh:
                    fh.write(blob)
            # Post-condition: the file on disk must re-read with the same counts the clip had in
            # memory. Nothing else here proves the description survived.
            with open(fpath, encoding="utf-8") as fh:
                back = json.load(fh)
            if back["counts"] != desc["counts"]:
                failures.append("%s: round-trip counts differ %s vs %s"
                                % (fname, back["counts"], desc["counts"]))
                continue
            written += 1
            keyframes += desc["counts"]["keyframes"]
            events += desc["counts"]["events"]
            per_container[base] += 1
            name_count[desc["name"]] += 1
            index.append({
                "id": cid,
                "container": base,
                "path_id": o.path_id,
                "name": desc["name"],
                "asset_path": payload["asset_path"],
                "descriptor": "tools/migration/anim_clips/" + fname,
                "wrap_mode": desc["wrap_mode"],
                "wrap_name": desc["wrap_name"],
                "length": desc["length"],
                "keyframes": desc["counts"]["keyframes"],
                "events": desc["counts"]["events"],
                "paths": sorted({c["path"] for c in desc["curves"]["position"]
                                 + desc["curves"]["rotation"] + desc["curves"]["scale"]}),
            })

    doc = {
        "_comment": ("Generated by tools/migration/export_animation_descriptors.py. "
                     "Clip identity -> curve descriptor -> 2022 .anim asset path. The descriptor "
                     "files under tools/migration/anim_clips/ are regenerable and not committed; "
                     "this index is the binding table the Editor builder and the scene rebuilder "
                     "both read, so it is committed. Both lists are arrays, not objects keyed by "
                     "id, because JsonUtility cannot bind dictionaries."),
        "clips": sorted(index, key=lambda e: (e["name"], e["id"])),
        "duplicate_names": [{"name": n, "count": c}
                            for n, c in sorted(name_count.items()) if c > 1],
    }
    with open(INDEX, "w", encoding="utf-8") as fh:
        json.dump(doc, fh, sort_keys=True, indent=1)

    print("\n=== export result ===")
    print("  descriptors written : %d" % written)
    print("  keyframes           : %d" % keyframes)
    print("  animation events    : %d" % events)
    print("  distinct names      : %d (%d names occur more than once)"
          % (len(name_count), len([n for n, c in name_count.items() if c > 1])))
    print("  per container       : %s" % sorted(per_container.items(), key=lambda x: -x[1]))
    print("  index               : %s (%d entries)" % (INDEX, len(index)))
    if failures:
        print("\n  FAILURES (%d):" % len(failures))
        for f in failures[:15]:
            print("    " + f)
        return 1
    if written != 427:
        print("\n  UNEXPECTED: scan_animations.py counted 427 clips, this wrote %d -- the two "
              "counts must agree or the scope is wrong again." % written)
        return 1
    print("\n  matches the 427-clip inventory and the 118081-keyframe tally: %s"
          % ("yes" if keyframes == 118081 else "NO, keyframes=%d" % keyframes))
    return 0


if __name__ == "__main__":
    sys.exit(main())
