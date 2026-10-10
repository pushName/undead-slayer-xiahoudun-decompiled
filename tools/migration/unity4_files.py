"""Shared access to the Unity 4 build data in dump/assets.

Kept in one place because getting the file list wrong is silently catastrophic here:
`sharedassetsN.assets.split0..N` are raw byte ranges, not serialized files, and UnityPy
happily "parses" them into thousands of phantom objects. The first version of
export_textures.py did exactly that and reported 7466 textures instead of 915.

Two rules, encoded once:
  * never load a path containing ".split" -- use the reassembled copies in RECON
  * a SerializedFile's externals table is what turns a PPtr's m_FileID into a real file
"""
import glob
import os

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP = os.path.join(_REPO, "dump", "assets")
RECON = os.environ.get("RECON_DIR") or os.path.join(os.environ.get("TEMP", "/tmp"), "recon")


def externals_of(env):
    """Ordered list of file basenames this SerializedFile depends on (m_FileID is 1-based)."""
    for attr in ("serial_file", "file"):
        f = getattr(env, attr, None)
        ext = getattr(f, "externals", None)
        if ext is not None:
            return [os.path.basename(str(d.path or "")).replace("\\", "/") for d in ext]
    return []


def resolve_target(this_name, externals, file_id):
    """PPtr m_FileID -> the basename of the file the object actually lives in."""
    fid = int(file_id)
    if fid == 0:
        return this_name
    if 0 < fid <= len(externals):
        return externals[fid - 1]
    return None


def split_families():
    """{whole-file basename: fragment count} for everything the APK ships in byte-range pieces."""
    out = {}
    for p in glob.glob(os.path.join(DUMP, "*.split*")):
        b = os.path.basename(p)
        whole = b.split(".split")[0]
        if ".split" not in b or not whole:
            continue
        out[whole] = out.get(whole, 0) + 1
    return out


def sources():
    """[(basename, abspath)] for every loadable serialized file, fragments excluded.

    Levels first, then sharedassets (reassembled copies win over the non-split leftovers),
    then the hash-named AssetBundles under dump/assets -- which include
    'unity default resources', where Unity 4 keeps its built-in shaders.

    Raises if a file that only exists as fragments has no reassembled copy to read. That is not
    pedantry: the reassembled copies live outside the repository and Windows' temp cleanup deletes
    them, and a caller that just iterates this list then reports a smaller world as if it were the
    world. On 2026-10-10 that happened twice in one day (first 449 loads all failed, then 11 of 12
    reassembled files were gone and an audio inventory came out at 32 clips instead of the real
    number), both times printing a confident count. Rebuild with tools/migration/reassemble_splits.py.
    Set ALLOW_INCOMPLETE_SOURCES=1 to get the old lenient behaviour back on purpose.
    """
    out = []
    seen = set()

    def add(p):
        b = os.path.basename(p)
        if b in seen or ".split" in b or not os.path.isfile(p):
            return
        seen.add(b)
        out.append((b, p))

    for p in sorted(glob.glob(os.path.join(DUMP, "level*"))):
        add(p)
    for p in sorted(glob.glob(os.path.join(RECON, "sharedassets*.assets"))):
        add(p)
    for p in sorted(glob.glob(os.path.join(DUMP, "sharedassets*.assets"))):
        add(p)
    for p in sorted(glob.glob(os.path.join(DUMP, "*"))):
        b = os.path.basename(p)
        if b.endswith(".dll") or b == "Managed":
            continue
        add(p)

    missing = sorted(w for w in split_families() if w not in seen)
    if missing and not os.environ.get("ALLOW_INCOMPLETE_SOURCES"):
        raise RuntimeError(
            "%d container(s) exist only as fragments and no reassembled copy is readable: %s. "
            "RECON_DIR=%s. Run: python tools/migration/reassemble_splits.py (under the UnityPy "
            "venv), or set ALLOW_INCOMPLETE_SOURCES=1 to accept a partial scan."
            % (len(missing), ", ".join(missing[:6]), RECON))
    return out


def load(path):
    return UnityPy.load(path)
