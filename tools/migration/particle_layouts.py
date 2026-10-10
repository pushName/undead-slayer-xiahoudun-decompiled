"""Derive the Unity 4 byte layout of the legacy particle components -- survey pass.

Why this file exists: UnityPy 1.25 has no usable structure for these three classes. It exposes 1 field
on ParticleAnimator (just m_GameObject) and 6 on EllipsoidParticleEmitter, which means everything that
decides what a particle effect LOOKS like -- lifetime, emission rate, start size/colour, the size and
colour keyframe tracks, damping, angular velocity -- is sitting in bytes nothing has interpreted.
Mapping them onto a 2022 ParticleSystem therefore cannot start as "read the fields"; it has to start
by proving the layout, the same way MonoBehaviour fields and the 312-byte GUIStyle were settled here.

What makes the layout derivable at all: the record size is identical across every instance in the
build (measured 2026-10-10: EllipsoidParticleEmitter 116 bytes x120, ParticleAnimator 90 x121,
ParticleRenderer 104 x121, MeshParticleEmitter 120 x1). Fixed size plus many samples means a
candidate layout can be falsified cheaply: it must consume every byte of all N records.

This pass deliberately makes NO semantic claims. It prints three structural signals:
  * the anchor: where the owner GameObject's path_id actually appears in the record -- every
    component starts with a GameObject PPtr in Unity 4, so this fixes offset zero and turns the
    float view into something addressable;
  * per-offset statistics over all samples: constant offsets (defaults or padding) versus varying
    ones (real data), with the float interpretation of both;
  * a hexdump of the few records whose owner differs, so structure can be read by eye.

Run:  <unitypy venv python> tools/migration/particle_layouts.py [--dump N] RECON_DIR=<path>
"""
import argparse
import collections
import os
import struct
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

CLASSES = ("EllipsoidParticleEmitter", "MeshParticleEmitter", "ParticleAnimator",
           "ParticleRenderer")


def collect():
    """-> {class: [(container, path_id, owner_go_pid, bytes)]} over the whole build."""
    out = collections.defaultdict(list)
    for base, path in u4.sources():
        env = UnityPy.load(path)
        by = {o.path_id: o for o in env.objects}
        owners = collections.defaultdict(list)
        for pid, o in by.items():
            if o.type.name != "GameObject":
                continue
            try:
                d = o.read()
            except Exception:
                continue
            for tid, pp in u4.component_pairs(d):
                owners[int(pp.m_PathID)].append((pid, int(tid)))
        for pid, o in by.items():
            if o.type.name not in CLASSES:
                continue
            raw = bytes(o.get_raw_data())
            own = owners.get(pid) or []
            out[o.type.name].append((base, pid, own[0][0] if own else None, raw))
    return out


def anchor_offset(raw, owner):
    """Where the owner GameObject's path_id appears as a little-endian int32 in the record."""
    if owner is None:
        return []
    needle = struct.pack("<i", owner)
    hits, i = [], raw.find(needle)
    while i >= 0 and len(hits) < 6:
        hits.append(i)
        i = raw.find(needle, i + 1)
    return hits


def floats(raw):
    n = len(raw) // 4
    return list(struct.unpack_from("<%df" % n, raw, 0))


def report(rows, name, dump):
    sizes = collections.Counter(len(r[3]) for r in rows)
    print("\n===== %s: %d records, sizes %s =====" % (name, len(rows), dict(sizes)))
    if len(sizes) != 1:
        print("  !! size is NOT uniform -- the fixed-layout assumption does not hold for this class")
    (base, pid, owner, raw), = rows[:1]
    print("  anchor check (owner GameObject path_id inside the record), first 8 records:")
    offsets = collections.Counter()
    for b, p, o, r in rows[:8]:
        hits = anchor_offset(r, o)
        print("     %-28s owner=%-6s found at %s" % (b[:26], o, hits))
        for h in hits:
            offsets[h] += 1
    print("  anchor offsets seen: %s" % dict(offsets))

    # per-offset float view over ALL records: constants vs varying
    n = min(len(r[3]) for r in rows) // 4
    print("  per-slot float view (%d slots, %d records):" % (n, len(rows)))
    for i in range(n):
        vals = []
        for r in rows:
            vals.append(struct.unpack_from("<f", r[3], i * 4)[0])
        consts = len(set(round(v, 6) for v in vals)) == 1
        lo, hi = min(vals), max(vals)
        ints = [struct.unpack_from("<i", r[3], i * 4)[0] for r in rows]
        small_ints = all(-2 <= v <= 255 for v in ints)
        tag = ""
        if consts:
            tag = "CONSTANT %s" % round(vals[0], 6)
        elif small_ints and len(set(ints)) <= 40:
            tag = "int-like (values %s)" % sorted(set(ints))[:8]
        elif all(-1e4 <= v <= 1e4 for v in vals):
            tag = "float range %.4g..%.4g, %d distinct" % (lo, hi, len(set(vals)))
        else:
            tag = "wide/weird min %.3g max %.3g" % (lo, hi)
        print("    off %3d  %-46s  %s" % (i * 4, tag, [round(v, 4) for v in vals[:5]]))
    for b, p, o, r in rows[:dump]:
        print("  ---- %s!%d owner=%s len=%d" % (b, p, o, len(r)))
        print("       %s" % r.hex())


def classify(rows):
    """Per 4-byte slot, which interpretation survives EVERY sample.

    float_ok means: finite, and either exactly 0 or with a magnitude an engine could plausibly
    store (1e-9..1e9). A slot that fails that is not a float: the alpha ramp bytes of a colour
    (ff ff ff 0a = 2.6e-18 as a float) fail exactly this way, which is what separates the colour
    tracks from the numbers. bool_ok additionally requires the remaining three bytes to be zero,
    so "bool here" and "bool followed by alignment padding here" stay distinguishable.
    """
    n = min(len(r[3]) for r in rows) // 4
    out = []
    for i in range(n):
        raws = [r[3][i * 4:i * 4 + 4] for r in rows]
        fl = [struct.unpack("<f", b)[0] for b in raws]
        it = [struct.unpack("<i", b)[0] for b in raws]
        finite = all(f == f and abs(f) != float("inf") for f in fl)
        float_ok = finite and all(v == 0.0 or 1e-9 <= abs(v) <= 1e9 for v in fl)
        bool_ok = all(b[0] in (0, 1) and b[1:] == b"\0\0\0" for b in raws)
        quad_ok = all(v == 0 or 1 <= v <= 255 for b in raws for v in b) and not float_ok
        out.append(dict(off=i * 4, float_ok=float_ok, bool_ok=bool_ok, quad_ok=quad_ok,
                        floats=[round(v, 5) for v in fl[:4]], ints=sorted(set(it))[:6],
                        distinct=len(set(fl))))
    return out


# --------------------------------------------------------------------------- layouts
# A layout is an ordered list of (name, kind) and must consume the record exactly. Kinds:
#   pptr   8 bytes (file id + path id)        int/uint  4 bytes
#   fl     4 bytes, float                     b4        4 bytes: one byte + three zero bytes
#   rgba   4 bytes, four 0..255 channels      pad       4 bytes that must be zero
#   tail   the odd trailing bytes, exact size given
# Names marked "?" are structural: the bytes are accounted for and their type is proven, but the
# engine property they carry is not identified -- Unity 4 ships no TypeTree for these classes, the
# build has none, and UnityPy's own field names for them failed an anchor test (see the module
# docstring), so naming them further would be guessing.
# Built from the measured per-slot table (--classify), not from a remembered field order:
#   * the emitter's four flags at [8,12) are PACKED bytes (01 01 00 00), not bool+padding, which is
#     why the first version of this layout failed all 120 records;
#   * ParticleRenderer [8,12) is four packed bytes too, then two floats that are 1.0 in every record,
#     two that are 0.0 in every record, an int at 28 that is 1 (the material count), and the material
#     PPtr at 32 -- that last placement is corroborated independently: searching all 121 records for
#     the byte pattern of the PPtr UnityPy reads as m_Materials[0] finds it at 32 in 118 of them, and
#     in the other three those records are simply at a different offset, which is printed as a
#     mismatch rather than averaged away.
LAYOUTS = {
    "EllipsoidParticleEmitter": [
        ("gameObject", "pptr"),
        ("flag_8", "bool1"), ("flag_9", "bool1"), ("flag_10", "bool1"), ("flag_11", "bool1"),
    ] + [("f_%d" % o, "fl") for o in range(12, 96, 4)] + [
        # off 96 is not a float: its int readings are 0,1,256,257,65536,65537 -- four bytes each
        # holding 0 or 1, i.e. another block of packed flags. Reading it as a float is what failed
        # 113 of the 120 records.
        ("flag_96", "bool1"), ("flag_97", "bool1"), ("flag_98", "bool1"), ("flag_99", "bool1"),
    ] + [("f_%d" % o, "fl") for o in range(100, 116, 4)],
    "MeshParticleEmitter": [
        ("gameObject", "pptr"),
        ("flag_8", "bool1"), ("flag_9", "bool1"), ("flag_10", "bool1"), ("flag_11", "bool1"),
    ] + [("f_%d" % o, "fl") for o in range(12, 96, 4)] + [
        ("flag_96", "bool1"), ("flag_97", "bool1"), ("flag_98", "bool1"), ("flag_99", "bool1"),
    ] + [("f_%d" % o, "fl") for o in range(100, 116, 4)] + [
        # the last 4 bytes of the single MeshParticleEmitter record are not float-sane (0x21 padded),
        # and with one sample there is no way to tell a flag block from an int field -- left as raw
        # bytes and reported as unknown rather than named.
        ("unknown_116", "b4s"),
    ],
    "ParticleAnimator": [
        ("gameObject", "pptr"),
        ("flag_8", "b4"),
        ("color_12", "rgba"), ("color_16", "rgba"), ("color_20", "rgba"),
        ("color_24", "rgba"), ("color_28", "rgba"),
    ] + [("f_%d" % o, "fl") for o in range(32, 88, 4)] + [("tail_88", "tail2")],
    "ParticleRenderer": [
        ("gameObject", "pptr"),
        ("flag_8", "byte1"), ("flag_9", "bool1"), ("flag_10", "bool1"), ("flag_11", "byte1"),
        ("one_12", "fl"), ("one_16", "fl"), ("zero_20", "fl"), ("zero_24", "fl"),
        ("materialCount_28", "int"), ("material0_32", "pptr"),
    ] + [("zero_%d" % o, "fl") for o in range(40, 76, 4)] + [
        ("bytes_76", "b4s"), ("f_80", "fl"), ("f_84", "fl"), ("f_88", "fl"),
        ("enum_92", "b4s"), ("enum_96", "b4s"), ("f_100", "fl"),
    ]
}


def decode_one(raw, spec, label, problems):
    """-> {name: value} or None, appending to `problems` instead of raising.

    The byte cursor must land exactly on len(raw); anything else means the layout is wrong for this
    record, which is reported with its offset rather than being averaged away.
    """
    vals, off = {}, 0
    for name, kind in spec:
        if kind == "pptr":
            if off + 8 > len(raw):
                problems.append("%s: %s wants 8 bytes at %d, record is %d"
                                % (label, name, off, len(raw)))
                return None
            f, p = struct.unpack_from("<ii", raw, off)
            vals[name] = (f, p)
            off += 8
        elif kind == "tail2":
            rest = len(raw) - off
            if rest != 2:
                problems.append("%s: tail expected 2 bytes at %d, got %d" % (label, off, rest))
                return None
            vals[name] = list(raw[off:off + 2])
            off += rest
        elif kind in ("bool1", "byte1"):
            if off + 1 > len(raw):
                problems.append("%s: %s wants a byte at %d, record is %d"
                                % (label, name, off, len(raw)))
                return None
            b = raw[off]
            if kind == "bool1" and b not in (0, 1):
                problems.append("%s: %s at %d is %d, not 0/1 so not a bool" % (label, name, off, b))
                return None
            vals[name] = b
            off += 1
        else:
            need = 4
            if off + need > len(raw):
                problems.append("%s: %s wants 4 bytes at %d, record is %d"
                                % (label, name, off, len(raw)))
                return None
            b = raw[off:off + 4]
            if kind == "fl":
                v = struct.unpack("<f", b)[0]
                if not (v == v and abs(v) != float("inf")) or (v != 0.0 and not
                                                               (1e-9 <= abs(v) <= 1e9)):
                    problems.append("%s: %s at %d is not float-sane (%r)" % (label, name, off, v))
                    return None
                vals[name] = round(v, 6)
            elif kind == "b4":
                if b[0] not in (0, 1) or b[1:] != b"\0\0\0":
                    problems.append("%s: %s at %d is not a bool+padding (%s)"
                                    % (label, name, off, b.hex()))
                    return None
                vals[name] = bool(b[0])
            elif kind == "uint":
                vals[name] = struct.unpack("<I", b)[0]
            elif kind == "int":
                vals[name] = struct.unpack("<i", b)[0]
            elif kind == "pad":
                if b != b"\0\0\0\0":
                    problems.append("%s: %s at %d should be padding but is %s"
                                    % (label, name, off, b.hex()))
                    return None
            elif kind == "b4s":
                vals[name] = list(b)
            elif kind == "rgba":
                vals[name] = list(b)
            else:
                raise ValueError(kind)
            off += need
    if off != len(raw):
        problems.append("%s: layout consumed %d of %d bytes" % (label, off, len(raw)))
        return None
    return vals


def verify(rows, name, sink=None):
    spec = LAYOUTS[name]
    problems, ok = [], 0
    decoded = []
    for base, pid, owner, raw in rows:
        v = decode_one(raw, spec, "%s!%d" % (base, pid), problems)
        if v is None:
            continue
        ok += 1
        decoded.append((base, pid, owner, raw, v))
    if sink is not None:
        for base, pid, owner, raw, v in decoded:
            sink["%s!%d" % (base, pid)] = {"class": name, "owner": owner, "fields": v,
                                           "bytes": len(raw)}
    print("\n===== %s: layout over %d records =====" % (name, len(rows)))
    print("  bytes accounted for: %d/%d records decode cleanly" % (ok, len(rows)))
    if problems:
        print("  failures (%d), first 8 -- the layout is wrong, not the data:" % len(problems))
        for p in problems[:8]:
            print("     " + p)
    if not ok:
        return []
    # semantic cross-checks that do not need names
    fls = sorted(k for k, kind in spec if kind == "fl")
    print("  float slots: %d   rgba slots: %s   bool slots: %s"
          % (len(fls), [k for k, kind in spec if kind == "rgba"],
             [k for k, kind in spec if kind == "b4"]))
    # min/max pair evidence: adjacent float slots where lo <= hi in EVERY record. Names come from
    # the spec, not from a naming convention, so the check still works on slots that were named.
    pairs = []
    fls, off = [], 0
    for n, kind in spec:                      # walk the spec the way the decoder does
        width = {"pptr": 8, "tail2": 2}.get(kind, 1 if kind in ("bool1", "byte1") else 4)
        if kind == "fl":
            fls.append((off, n))
        off += width
    fls.sort()
    for (oa, na), (ob, nb) in zip(fls, fls[1:]):
        if ob - oa != 4:
            continue
        va = [d[4][na] for d in decoded]
        vb = [d[4][nb] for d in decoded]
        # two constants also satisfy lo<=hi, which proves nothing: require the pair to actually
        # separate in at least one record before calling it a candidate min/max field.
        if all(x <= y for x, y in zip(va, vb)) and any(x < y for x, y in zip(va, vb)):
            pairs.append((oa, na, ob, nb, round(min(va), 4), round(max(vb), 4)))
    print("  adjacent slot pairs with lo<=hi in every record (candidate min/max fields): %d"
          % len(pairs))
    for oa, na, ob, nb, lo, hi in pairs:
        print("     off %3d/%3d  %-14s %-14s range %s .. %s" % (oa, ob, na, nb, lo, hi))
    return decoded


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dump", type=int, default=0, help="hexdumps per class")
    ap.add_argument("--classify", action="store_true", help="print the per-slot classification")
    ap.add_argument("--out", default=None,
                    help="write the layouts plus every decoded record (the converter's input)")
    args = ap.parse_args()
    rows = collect()
    print("legacy particle component records in the build:")
    for k in CLASSES:
        print("  %-26s %d" % (k, len(rows.get(k, []))))
    decoded_all = {}
    for k in CLASSES:
        if not rows.get(k):
            continue
        if args.classify:
            print("\n---- %s: per-slot admissible interpretations ----" % k)
            for c in classify(rows[k]):
                print("    off %3d float_ok=%-5s bool_ok=%-5s quad_ok=%-5s floats=%s ints=%s"
                      % (c["off"], c["float_ok"], c["bool_ok"], c["quad_ok"], c["floats"],
                         c["ints"]))
        if k in LAYOUTS:
            verify(rows[k], k, decoded_all)
        if args.dump:
            report(rows[k], k, args.dump)
    clean = sum(1 for _ in decoded_all)
    total = sum(len(v) for v in rows.values())
    print("")
    print("=== decode coverage across all four classes: %d/%d records ===" % (clean, total))
    failed = total - clean
    if failed:
        print("  %d record(s) did not decode -- named above per class; they stay gaps rather than"
              " being folded into the layout of a class they may not belong to" % failed)
    if args.out:
        import json
        doc = {"layouts": {k: [(n, t) for n, t in v] for k, v in LAYOUTS.items()},
               "records": decoded_all}
        NL = chr(10)
        with open(args.out, "w", encoding="utf-8", newline=NL) as fh:
            json.dump(doc, fh, indent=1, ensure_ascii=False)
        print("wrote %s (%d decoded records)" % (args.out, len(decoded_all)))
    return 0



if __name__ == "__main__":
    sys.exit(main())
