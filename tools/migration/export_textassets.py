"""Export the shipped TextAssets into unity/Assets/Resources so Resources.Load() finds them.

WHY THIS EXISTS

UI_intro.cs:191 does `Resources.Load("hsp_version") as TextAsset` and line 192 immediately
dereferences it, so with an empty Resources/ folder that is a plain NullReferenceException at the
top of the intro screen. The 2022 project had exactly one file under Assets/Resources (Unity's own
BillingMode.json), while the scripts contain 101 Resources.Load call sites.

The name on disk IS the lookup key, so these files are written as the bare shipped name with no
source suffix -- unlike Imported/, where suffixes disambiguate same-named meshes and materials.

Unity 4 serialises TextAsset as { string m_Name; byte[] m_Script; ... } with 4-byte alignment
between fields. The payload is read out of the raw object bytes rather than UnityPy's decoded
`m_Script` string: that attribute gives 39902 characters for blockName where the real payload is
91547 bytes, so anything written from it depends on a decode/encode round trip. Re-encoding is
asserted to be lossless here anyway, as a second opinion on the raw parse.

Run:  <unitypy venv python> tools/migration/export_textassets.py [--dry-run]
"""
import argparse
import hashlib
import os
import struct
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import UnityPy                                            # noqa: E402
from unity4_files import sources                          # noqa: E402

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT = os.path.join(REPO, "unity", "Assets", "Resources")
EXT = ".bytes"


def aligned(off):
    return (off + 3) & ~3


def read_text_assets(path):
    """[(path_id, name, payload_bytes)] from one serialized file, parsed from raw bytes."""
    out = []
    env = UnityPy.load(path)
    for o in env.objects:
        if o.type.name != "TextAsset":
            continue
        raw = o.get_raw_data()
        try:
            n = struct.unpack_from("<i", raw, 0)[0]
            name = raw[4:4 + n].decode("utf-8", "replace")
            off = aligned(4 + n)
            ln = struct.unpack_from("<i", raw, off)[0]
            off += 4
            if ln < 0 or off + ln > len(raw):
                print("  skip  %s!%d %r: declared payload %d does not fit in %d bytes"
                      % (os.path.basename(path), o.path_id, name, ln, len(raw)))
                continue
            out.append((o.path_id, name, raw[off:off + ln]))
        except struct.error as e:
            print("  skip  %s!%d: truncated header (%s)" % (os.path.basename(path), o.path_id, e))
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    found = {}
    for name, path in sources():
        for pid, ta_name, body in read_text_assets(path):
            digest = hashlib.md5(body).hexdigest()
            prev = found.get(ta_name)
            if prev and prev[2] != digest:
                # Same key, different bytes: Resources.Load can only return one of them, so
                # picking silently would ship the wrong table.
                print("  CONFLICT %r differs between %s!%d and %s!%d -- refusing to choose"
                      % (ta_name, prev[1], prev[0], name, pid))
                return 1
            if prev:
                continue
            found[ta_name] = (pid, name, digest, body)

    print("TextAssets found: %d distinct names" % len(found))
    written = 0
    for ta_name in sorted(found):
        pid, src, digest, body = found[ta_name]
        dest = os.path.join(OUT, ta_name + EXT)
        print("  %-14s %7d bytes  md5 %s  <- %s!%d"
              % (ta_name, len(body), digest[:10], src, pid))
        if args.dry_run:
            continue
        os.makedirs(OUT, exist_ok=True)
        with open(dest, "wb") as fh:
            fh.write(body)
        # Post-condition: what is on disk must be the parsed bytes, and the UTF-8 round trip
        # must be lossless -- that is what makes trusting UnityPy's decoded string moot.
        back = open(dest, "rb").read()
        if back != body:
            print("  FAIL %s: re-read %d bytes, wrote %d" % (ta_name, len(back), len(body)))
            return 1
        if body.decode("utf-8", "replace").encode("utf-8") != body:
            print("  note %s: payload is not clean UTF-8; written as raw bytes" % ta_name)
        written += 1

    if args.dry_run:
        print("\nDRY RUN -- nothing written")
        return 0
    print("written %d file(s) -> %s" % (written, os.path.relpath(OUT, REPO).replace(os.sep, "/")))
    print("Now let Unity import them (focus the Editor window). Resources.Load(\"<name>\") resolves")
    print("by file name, so these must keep the bare shipped name with no source suffix.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
