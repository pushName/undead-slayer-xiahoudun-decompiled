"""Reassemble the split sharedassets files that the APK ships as byte-range fragments.

WHY THIS EXISTS

dump/assets/sharedassetsN.assets exists ONLY as sharedassetsN.assets.split0 .. splitK; there is no
whole file anywhere in the repository. Every tool that reads bundles has to work off reassembled
copies, and unity4_files.sources() deliberately refuses to load anything containing ".split" --
feeding a half file to UnityPy yields phantom objects (that is how a texture count once inflated
from 915 to 7466).

Before this script the reassembled copies existed only in %TEMP%\\recon, produced by hand, with no
way to reproduce them. Clear the temp directory and every bundle-dependent tool keeps running but
silently sees fewer objects -- the exact failure mode this repository keeps having to unlearn.
So this script writes them, and then PROVES it: byte totals, and a real UnityPy load with a
non-zero object count per file.

The ordering rule is verified, not assumed: concatenating split0, split1, ... by numeric index
reproduced all 12 existing copies byte-for-byte (size and md5) when this was first checked.

Run under the UnityPy venv:
    %TEMP%\\unitypy-venv\\Scripts\\python.exe tools/migration/reassemble_splits.py
Options:
    --verify-only   check what is already there, write nothing
    --force         rewrite even when the size already matches
    --no-load-test  skip the UnityPy load assertion (offline / no venv)
"""
import argparse
import collections
import hashlib
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files  # noqa: E402  (gives DUMP/RECON so the target path has one owner, not two)

SPLIT_RE = re.compile(r"^(.+\.assets)\.split(\d+)$")


def groups():
    """{basename: [(index, path), ...]} for every fragment group, ordered numerically."""
    out = collections.defaultdict(list)
    for name in os.listdir(unity4_files.DUMP):
        m = SPLIT_RE.match(name)
        if m:
            out[m.group(1)].append((int(m.group(2)), os.path.join(unity4_files.DUMP, name)))
    for parts in out.values():
        parts.sort()
    return out


def md5_of(path, chunk=1 << 20):
    h = hashlib.md5()
    with open(path, "rb") as fh:
        while True:
            b = fh.read(chunk)
            if not b:
                break
            h.update(b)
    return h.hexdigest()


def build(base, parts, force):
    """-> (status, detail). status in {ok, exists, rebuilt, GAP, mismatch, FAIL}"""
    dest = os.path.join(unity4_files.RECON, base)
    total = sum(os.path.getsize(p) for _, p in parts)
    idx = [i for i, _ in parts]
    if idx != list(range(len(idx))):
        return "GAP", "indices %s are not contiguous from 0 -- refusing to guess" % idx
    if os.path.exists(dest) and os.path.getsize(dest) == total and not force:
        return "exists", "%d bytes, %d parts" % (total, len(parts))

    tmp = dest + ".part"
    os.makedirs(unity4_files.RECON, exist_ok=True)
    h = hashlib.md5()
    with open(tmp, "wb") as out:
        for _, p in parts:
            with open(p, "rb") as fh:
                while True:
                    b = fh.read(1 << 20)
                    if not b:
                        break
                    out.write(b)
                    h.update(b)
    written = os.path.getsize(tmp)
    if written != total:
        os.remove(tmp)
        return "FAIL", "wrote %d bytes, expected %d" % (written, total)
    os.replace(tmp, dest)
    if os.path.exists(dest) and md5_of(dest) != h.hexdigest():
        return "mismatch", "re-read md5 differs from the streamed md5"
    return "rebuilt", "%d bytes from %d parts, md5 %s" % (total, len(parts), h.hexdigest()[:12])


def load_check():
    """Assert each reassembled file is a real serialized file with objects in it."""
    import UnityPy
    bad = []
    for name, path in unity4_files.sources():
        if not name.startswith("sharedassets"):
            continue
        try:
            n = sum(1 for _ in UnityPy.load(path).objects)
        except Exception as e:
            bad.append((name, "%s: %s" % (type(e).__name__, str(e)[:60])))
            continue
        if n == 0:
            bad.append((name, "loaded but 0 objects"))
        else:
            print("   %-26s %6d objects" % (name, n))
    return bad


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--verify-only", action="store_true")
    ap.add_argument("--force", action="store_true")
    ap.add_argument("--no-load-test", action="store_true")
    args = ap.parse_args()

    g = groups()
    if not g:
        print("no *.assets.splitN fragments under %s" % unity4_files.DUMP)
        return 1
    print("fragment groups: %d | target dir: %s" % (len(g), unity4_files.RECON))
    tally = collections.Counter()
    problems = []
    for base in sorted(g, key=lambda b: (len(b), b)):
        if args.verify_only:
            dest = os.path.join(unity4_files.RECON, base)
            total = sum(os.path.getsize(p) for _, p in g[base])
            good = os.path.exists(dest) and os.path.getsize(dest) == total
            tally["verified" if good else "missing"] += 1
            if not good:
                problems.append((base, "no reassembled copy of %d bytes" % total))
            continue
        status, detail = build(base, g[base], args.force)
        tally[status] += 1
        if status in ("GAP", "mismatch", "FAIL"):
            problems.append((base, detail))
        print("  %-8s %-26s %s" % (status, base, detail))

    print("\n%s" % dict(tally))
    for b, d in problems:
        print("  !! %s: %s" % (b, d))
    if problems:
        print("FAIL")
        return 1

    if not args.no_load_test:
        print("\nload test (this is what proves the copies are usable, not just present):")
        bad = load_check()
        for n, why in bad:
            print("  !! %s: %s" % (n, why))
        if bad:
            print("FAIL: %d reassembled file(s) do not load" % len(bad))
            return 1
    print("PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())
