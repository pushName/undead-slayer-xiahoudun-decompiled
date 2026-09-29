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


def sources():
    """[(basename, abspath)] for every loadable serialized file, fragments excluded.

    Levels first, then sharedassets (reassembled copies win over the non-split leftovers),
    then the hash-named AssetBundles under dump/assets -- which include
    'unity default resources', where Unity 4 keeps its built-in shaders.
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
    return out


def load(path):
    return UnityPy.load(path)
