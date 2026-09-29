"""Build the MonoScript binding table that scene reconstruction needs.

WHY THIS EXISTS
Unity 4 player builds carry NO TypeTree, so UnityPy's schema-driven reader can only
parse a MonoBehaviour when its total size happens to match the (wrong) layout it
assumes -- in this data that is 98 of 394 objects; the other 296 raise
"Expected to read N bytes, but only read 24 bytes".

Unity 4 serializes PPtr as { int32 FileID; int32 PathID } = 8 bytes (64-bit PathID only
arrived with SerializedFile v13 / Unity 5.5). So a MonoBehaviour header is a fixed 24 bytes:

    0   int32  m_GameObject.FileID
    4   int32  m_GameObject.PathID
    8   int32  m_Enabled
    12  int32  m_Script.FileID      <- 0 = this file, n>0 = n-th entry in the externals table
    16  int32  m_Script.PathID      <- MonoScript path_id inside that file
    20  int32  m_Name.length
    24  ...    m_Name bytes, then the script's own serialized field data

This module reads those fixed offsets straight from the raw object bytes, so it resolves
100% of the pointers instead of 25%. It validates each one (the GameObject back-reference
must exist in the same file, the name length must fit) and reports anything that does not.

A MonoBehaviour's own field values therefore also start at offset 24 -- that is what the
scene converter will need, and it can only be decoded per-class from Assembly-CSharp.

Emits:
  tools/migration/monoscript-bindings.tsv    human audit
  tools/migration/monoscript-bindings.json   consumed by the scene converter

Read-only. Run:  <unitypy venv python> tools/migration/build_monoscript_table.py
"""
import collections
import glob
import json
import os
import struct
import sys

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP = os.path.join(_REPO, "dump", "assets")
RECON = os.environ.get("RECON_DIR") or os.path.join(os.environ.get("TEMP", "/tmp"), "recon")
SCRIPTS = os.path.join(_REPO, "unity", "Assets", "Scripts")
OUT_TSV = os.path.join(_REPO, "tools", "migration", "monoscript-bindings.tsv")
OUT_JSON = os.path.join(_REPO, "tools", "migration", "monoscript-bindings.json")

HDR = struct.Struct("<iiiii")  # goFileID goPathID enabled scFileID scPathID
NAME_LEN_OFF = 20
HEADER_LEN = 24

import re
GUID_RE = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.M)


def shared_files():
    out = {}
    for p in sorted(glob.glob(os.path.join(DUMP, "sharedassets*.assets"))):
        out[os.path.basename(p)] = p
    for p in sorted(glob.glob(os.path.join(RECON, "sharedassets*.assets"))):
        out.setdefault(os.path.basename(p), p)
    return out


def meta_guids():
    guids = {}
    for root, _d, files in os.walk(SCRIPTS):
        for fn in files:
            if fn.endswith(".cs.meta"):
                with open(os.path.join(root, fn), "r", encoding="utf-8", errors="replace") as fh:
                    m = GUID_RE.search(fh.read())
                if m:
                    guids[fn[:-len(".cs.meta")]] = m.group(1)
    return guids


def cs_files():
    names = {}
    for root, _d, files in os.walk(SCRIPTS):
        for fn in files:
            if fn.endswith(".cs"):
                names.setdefault(fn[:-3],
                                 os.path.relpath(os.path.join(root, fn), _REPO).replace("\\", "/"))
    return names


def main():
    shared = shared_files()
    guids = meta_guids()
    cs = cs_files()

    # (sharedassets basename, path_id) -> MonoScript m_Name
    monoscripts = {}
    for fname, path in shared.items():
        try:
            env = UnityPy.load(path)
        except Exception as exc:
            print("LOAD FAIL", fname, repr(exc), file=sys.stderr)
            continue
        for o in env.objects:
            if o.type.name == "MonoScript":
                try:
                    monoscripts[(fname, o.path_id)] = str(o.read().m_Name)
                except Exception:
                    monoscripts[(fname, o.path_id)] = "<unreadable>"

    refs = collections.Counter()
    per_level = collections.defaultdict(collections.Counter)
    level_externals = {}
    problems = []
    layout_bad = []
    total = 0

    levels = sorted(glob.glob(os.path.join(DUMP, "level*")),
                    key=lambda p: (len(os.path.basename(p)), os.path.basename(p)))

    for lp in levels:
        lname = os.path.basename(lp)
        env = UnityPy.load(lp)
        sf = env.serial_file if hasattr(env, "serial_file") else env.file
        externals = [os.path.basename(str(d.path)).replace("\\", "/") for d in (sf.externals or [])]
        level_externals[lname] = externals

        # Validation of the 8-byte-PPtr layout: offset 4 must be the path_id of a GameObject
        # that actually exists in this same file. If the layout were wrong this would miss.
        gameobjects = {o.path_id for o in env.objects if o.type.name == "GameObject"}

        for o in env.objects:
            if o.type.name != "MonoBehaviour":
                continue
            total += 1
            raw = bytes(o.get_raw_data())
            if len(raw) < HEADER_LEN:
                problems.append((lname, o.path_id, "short header %dB" % len(raw)))
                continue
            _gof, go_pid, _en, scf, scp = HDR.unpack_from(raw, 0)
            if _gof != 0 or go_pid not in gameobjects:
                layout_bad.append((lname, o.path_id, _gof, go_pid))
            nlen = struct.unpack_from("<i", raw, NAME_LEN_OFF)[0]
            if nlen < 0 or HEADER_LEN + nlen > len(raw):
                problems.append((lname, o.path_id, "implausible m_Name length %d in %dB" % (nlen, len(raw))))
                continue
            target = lname if scf == 0 else (externals[scf - 1] if scf - 1 < len(externals) else "<bad FileID %d>" % scf)
            name = monoscripts.get((target, scp))
            if name is None:
                problems.append((lname, o.path_id, "MonoScript %s!%d not found" % (target, scp)))
                continue
            refs[name] += 1
            per_level[lname][name] += 1

    referenced = sorted(refs)
    lower = {k.lower(): k for k in cs}

    rows = ["# MonoScript bindings for scene reconstruction (Unity 4 build data -> Unity 2022).",
            "# name      = MonoScript.m_Name in the shipped data; Unity 4 has no separate class-name",
            "#             field, so m_Name IS the class name.",
            "# instances = MonoBehaviour objects in level0..level22 bound to this script.",
            "# guid      = unity/Assets/Scripts/<name>.cs.meta guid, i.e. what a 2022 MonoBehaviour's",
            "#             m_Script must reference ({fileID: 11500000, guid: ..., type: 3}).",
            "# name\tinstances\tcs_file\tguid\tstatus",
            ]
    stats = collections.Counter()
    for name in referenced:
        rel = cs.get(name)
        if rel is not None:
            guid = guids.get(name, "")
            status = "ok" if guid else "cs_present_editor_has_not_imported_yet"
        else:
            alt = lower.get(name.lower())
            if alt is None:
                rel, guid, status = "", "", "NO_MATCHING_CS"
            else:
                rel, guid = cs[alt], guids.get(alt, "")
                status = "CASE_ONLY_MATCH -> %s" % alt
        stats[status] += 1
        rows.append("%s\t%d\t%s\t%s\t%s" % (name, refs[name], rel, guid, status))

    unreferenced = sorted(set(monoscripts.values()) - set(referenced))

    with open(OUT_TSV, "w", encoding="utf-8", newline="\n") as fh:
        fh.write("\n".join(rows) + "\n")

    payload = {
        "monoscripts": {"%s!%d" % k: v for k, v in monoscripts.items()},
        "level_externals": level_externals,
        "script_to_cs": {n: cs.get(n, lower.get(n.lower(), "")) for n in referenced},
        "script_to_guid": {n: guids.get(n) or guids.get(lower.get(n.lower(), ""), "") for n in referenced},
        "instances_per_script": dict(refs),
        "instances_per_level": {k: dict(v) for k, v in per_level.items()},
    }
    with open(OUT_JSON, "w", encoding="utf-8") as fh:
        json.dump(payload, fh, indent=1, ensure_ascii=False)

    print("MonoBehaviour objects in levels : %d" % total)
    print("  script pointer resolved       : %d" % sum(refs.values()))
    print("  problems                      : %d" % len(problems))
    print("  layout validation failures    : %d  (m_GameObject back-ref at offset 4 must be a real GameObject)" % len(layout_bad))
    for p in problems[:10]:
        print("     ", p)
    print()
    print("distinct MonoScript assets      : %d (%d names)" % (len(monoscripts), len(set(monoscripts.values()))))
    print("referenced by scene components  : %d" % len(referenced))
    print("present in data but unreferenced: %d" % len(unreferenced))
    print()
    print("binding status:")
    for k, v in stats.most_common():
        print("  %4d  %s" % (v, k))
    print()
    print("wrote %s" % os.path.relpath(OUT_TSV, _REPO).replace(os.sep, "/"))
    print("wrote %s" % os.path.relpath(OUT_JSON, _REPO).replace(os.sep, "/"))
    print()
    print("top 12 scripts by scene instances:")
    for n, c in refs.most_common(12):
        print("  %5d  %-26s %s" % (c, n, "guid ok" if (guids.get(n) or guids.get(lower.get(n.lower(), ""))) else "NO GUID"))


if __name__ == "__main__":
    main()
