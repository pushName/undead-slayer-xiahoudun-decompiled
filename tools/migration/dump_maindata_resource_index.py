"""Read-only: resolve mainData's shipped resource path table against the objects that actually
exist in the build.

Why this exists. The shipped game asks for assets BY NAME at runtime, and Unity 4 keeps the
authoritative name -> (serialized file, path_id) table in mainData's ResourceManager object.
`scan_prefabs.py` had to guess that mapping by matching GameObject.m_Name against the load keys, and
that guess produced 13 names with more than one candidate root (3 genuinely different:
`angel_splash5` has four same-named GameObjects in one container). The build itself does not guess,
so the question gets answered from its side.

What is already proven about the record layout, and what is not:

  * the path table runs from offset PATH_TABLE_OFFSET to the first record that fails its own
    invariants, 299 records here, each `(int32 A, int32 B, int32 len, name[len] NUL-padded up to a
    multiple of 4)`, names in sorted order;
  * A indexes a container, B is a path_id inside it -- on 60 sampled records that reading lands on
    a real object 46 times (34 with a matching m_Name) while swapping the two resolves 5.
  * NOT proven: the order of the container list in front of the table. A byte-order regex scan finds
    441 names, but each name is followed by a block whose size varies (18 bytes after
    `sharedassets1.assets`, 19 after `resources/unity_builtin_extra`), so that scan is not the vector
    order. Fixed strides die after 4 entries.

This tool therefore does not assume the container order. It pins it from the inside: a name that
occurs exactly once in the whole build, at path_id == B, forces A to be that container's index.
That worked -- 149 A values pinned, none conflicting with each other, none disagreeing with the
byte-order list -- so the container order is taken as established and (A = container index,
B = path_id) is the reading.

What the 299 records then say, measured rather than assumed:

  exact                     163  the object at files[A]!B is named this key
  points-at-other-object     27  an object exists at files[A]!B but carries a different m_Name --
                                 `angel_splash5` -> GameObject 'pt_ice', `bullet_angel5` ->
                                 'pt_bullet', `bullet_angel7` -> 'm2'. The registration name is the
                                 asset name, not the object name, so a candidate search by m_Name
                                 (what scan_prefabs.py did, and what named the textures) is looking
                                 at the wrong instrument for these.
  wrong-container            17  an object exists at files[A]!B, in a container the key does not
                                 live in at all: `jewel01a` -> 'name_boss1', `petkind_angel2` ->
                                 sharedassets10!7 'costume06' while the texture named petkind_angel2
                                 is sharedassets12!19.
  name-exists-elsewhere      92  nothing at files[A]!B, although the key exists elsewhere.

The last two groups are NOT yet explained and this tool does not claim they are. They share a
signature: within a run of records the B value equals the PREVIOUS record's true path_id
(`petkind_angel3` has B=19 and petkind_angel2's object really is at !19; `petkind_angel4` has B=20
and petkind_angel3's is at !20), which is what a second parallel vector read in the wrong lockstep
looks like. So either the record is wider than three ints plus a name, or m_PathIdRead is a separate
array and B means something else for these rows. Settling it needs the header before the container
block parsed too, not more guessing at the stride.

Run:  <unitypy venv python> tools/migration/dump_maindata_resource_index.py [--verbose]
      RECON_DIR=<path to reassembled sharedassets>
"""
import argparse
import collections
import json
import os
import re
import struct
import sys

import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MAINDATA = os.path.join(u4.DUMP, "mainData")
OUT = os.path.join(_REPO, "tools", "migration", "maindata-resource-index.json")

PATH_TABLE_OFFSET = 35796
CONTAINER_SCAN = (800, PATH_TABLE_OFFSET)          # where the container-name block lives
CONTAINER_RE = re.compile(
    rb"(?:sharedassets\d+\.assets|level\d+|[0-9a-f]{32}|resources/unity_builtin_extra"
    rb"|library/unity default resources)")


def read(path):
    with open(path, "rb") as fh:
        return fh.read()


def parse_path_table(buf):
    """[(A, B, name)] from the proven offset, stopping at the first record that breaks layout.

    The stop condition is deliberately strict: a wrong step mid-table would silently shift every
    later name, and a shifted name looks exactly like a real record.
    """
    out = []
    p = PATH_TABLE_OFFSET
    while p + 16 < len(buf):
        a, b, ln = struct.unpack_from("<iii", buf, p)
        if not (1 <= ln <= 240) or a < 0:
            break
        raw = buf[p + 12:p + 12 + ln]
        if not re.fullmatch(rb"[\w./\- ]+", raw):
            break
        out.append((a, b, raw.decode("utf-8", "replace")))
        p += 12 + ((ln + 3) // 4) * 4
    return out, p


def containers_by_byte_order(buf):
    lo, hi = CONTAINER_SCAN
    return [m.group().decode() for m in CONTAINER_RE.finditer(buf[lo:hi])]


def object_table(sources):
    """(name -> {"container!path_id"} folded to lower case, "container!path_id" -> (type, m_Name)).

    The name lookup is case-insensitive on purpose: the table itself stores `cashshop`, `gameno` and
    `blockname` while the shipped scripts ask for "CashShop", "gameNo" and "blockName", and the
    original build worked, so Unity 4's resource lookup folds case. That is the opposite of what
    check_resources_load_keys.py assumes in its docstring, and it is the reason the first version of
    this tool reported 16 "no-such-name" records that are plain case differences.
    """
    by_name = collections.defaultdict(set)
    by_key = {}
    unreadable = []
    for base, path in sources:
        try:
            env = u4.load(path)
        except Exception as exc:
            unreadable.append("%s: %s" % (base, str(exc)[:60]))
            continue
        for o in env.objects:
            try:
                nm = str(getattr(o.read(), "m_Name", "") or "")
            except Exception:
                nm = ""
            by_key["%s!%d" % (base, o.path_id)] = (o.type.name, nm)
            if nm:
                by_name[nm.lower()].add("%s!%d" % (base, o.path_id))
    return by_name, by_key, unreadable


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--verbose", action="store_true")
    args = ap.parse_args()

    buf = read(MAINDATA)
    records, end = parse_path_table(buf)
    files = containers_by_byte_order(buf)
    max_a = max(r[0] for r in records)
    print("path table: %d records, offsets %d..%d, A range 0..%d"
          % (len(records), PATH_TABLE_OFFSET, end, max_a))
    print("container names found by byte order: %d (so A up to %d needs %d of them)"
          % (len(files), max_a, max_a + 1))
    dupes = [n for n, c in collections.Counter(nm for _a, _b, nm in records).items() if c > 1]
    print("duplicate names inside the table: %d %s" % (len(dupes), dupes[:4]))

    sources = u4.sources()
    known = {base for base, _p in sources}
    outside = [f for f in files[:max_a + 1] if f not in known]
    print("container names in A's range that u4.sources() cannot open: %d %s"
          % (len(outside), outside[:6]))

    inv2, by_key, unreadable = object_table(sources)
    print("objects indexed: %d (containers unreadable: %d %s)"
          % (len(by_key), len(unreadable), unreadable[:3]))
    if unreadable:
        print("REFUSING to conclude: %d container(s) could not be opened, so 'nothing there' would "
              "be a statement about this scan." % len(unreadable))
        return 2

    def pointed_at(key):
        return by_key.get(key)

    stats = collections.Counter()
    rows = []
    drift = []
    pins = {}
    conflicts = []
    for a, b, name in records:
        key = "%s!%d" % (files[a], b) if a < len(files) else None
        holders = sorted(inv2.get(name.lower(), []))
        pointed = pointed_at(key) if key else None
        if key and holders and key in holders:
            cls = "exact"
        elif pointed and pointed[1].lower() == name.lower():
            cls = "exact"
        elif pointed:
            cls = "points-at-other-object"
            if len(holders) == 1:
                hcont = holders[0].split("!")[0]
                if hcont != files[a]:
                    cls = "wrong-container"
        elif holders:
            cls = "name-exists-elsewhere"
        else:
            cls = "no-such-name"
        stats[cls] += 1
        rows.append({"path": name, "a": a, "b": b, "verdict": cls,
                     "listed_container": files[a] if a < len(files) else None,
                     "pointed_object": None if pointed is None else
                     {"key": key, "type": pointed[0], "m_Name": pointed[1]},
                     "name_holders": holders})
        # The container list is not trusted, so pin it from the records themselves: a name that
        # exists once in the build, at path_id B, forces A to be that container's index.
        if len(holders) == 1 and holders[0].endswith("!%d" % b):
            cont = holders[0].split("!")[0]
            if a in pins and pins[a] != cont:
                conflicts.append((a, pins[a], cont, name))
            else:
                pins.setdefault(a, cont)

    wrong_pos = [(a, pins[a]) for a in sorted(pins)
                 if a < len(files) and files[a] != pins[a]]
    print("\nverdicts: %s" % dict(stats))
    print("container list fitted from the records themselves: %d A values pinned, %d mutually "
          "conflicting, %d where the byte-order list disagrees (%s)"
          % (len(pins), len(conflicts), len(wrong_pos),
             ["A=%d list=%s wants=%s" % (a, files[a][:12], c[:12]) for a, c in wrong_pos[:3]]))
    for cls in ("points-at-other-object", "wrong-container", "name-exists-elsewhere",
                "no-such-name"):
        ex = [r for r in rows if r["verdict"] == cls][:6]
        if ex:
            print("--- %s (%d shown of %d) ---" % (cls, len(ex), stats[cls]))
            for r in ex:
                po = r["pointed_object"]
                print("   %-20s A=%-4d B=%-5d -> %s | name in build at %s"
                      % (r["path"], r["a"], r["b"],
                         ("%s %r" % (po["key"][:34], po["m_Name"])) if po else "nothing",
                         ",".join(h[:30] for h in r["name_holders"][:2]) or "nowhere"))

    doc = {"records": rows, "containers": files, "stats": dict(stats),
           "container_pins": [{"a": a, "container": pins[a]} for a in sorted(pins)],
           "container_pin_conflicts": conflicts,
           "container_pin_disagree_with_byte_order":
               [{"a": a, "byte_order": files[a], "pinned": c} for a, c in wrong_pos],
           "table_offsets": [PATH_TABLE_OFFSET, end]}
    tmp = OUT + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(doc, fh, indent=1, ensure_ascii=False, sort_keys=True)
    os.replace(tmp, OUT)
    back = json.load(open(OUT, encoding="utf-8"))
    if len(back["records"]) != len(rows):
        print("MISMATCH: wrote %d records, re-read %d" % (len(rows), len(back["records"])))
        return 2
    print("wrote %s (%d records, re-read ok)"
          % (os.path.relpath(OUT, _REPO).replace(os.sep, "/"), len(back["records"])))
    if args.verbose:
        for r in rows:
            print("   %s" % r)
    return 0


if __name__ == "__main__":
    sys.exit(main())
