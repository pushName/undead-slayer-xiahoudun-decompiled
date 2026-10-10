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
import re

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


def component_pairs(go_data):
    """GameObject.m_Component -> [(classID, PPtr)], the one place that knows the order.

    UnityPy 1.25 returns each entry as a 2-tuple with the *classID first* and the PPtr second.
    Getting this backwards is not loud: a classID is a small integer, and small integers are valid
    path_ids in the same file, so `read(file, classID)` happily returns whatever object happens to
    live at that number -- a Material, usually. A scan of the prefab interiors reported "506 Material
    components" that way, and the ledger resolver that shared the mistake mis-attributed which prefab
    each reference belonged to. Pass a whole tuple in and the order is detected, so a future UnityPy
    that flips it cannot silently produce classids-as-pointers again.
    """
    out = []
    for c in getattr(go_data, "m_Component", []) or []:
        if isinstance(c, (tuple, list)) and len(c) == 2:
            a, b = c
            a_pptr = hasattr(a, "m_PathID")
            b_pptr = hasattr(b, "m_PathID")
            if a_pptr and not b_pptr:
                c = (b, a)          # (PPtr, classID) order: normalise to (classID, PPtr)
            elif not a_pptr and not b_pptr:
                continue            # neither half is a pointer: nothing to resolve
        elif hasattr(c, "m_PathID"):
            c = (None, c)
        else:
            continue
        out.append((c[0], c[1]))
    return out


def component_path_ids(go_data):
    return [int(getattr(pp, "m_PathID", pp) or 0) for _tid, pp in component_pairs(go_data)]


def safe_token(s):
    """A file-name-safe form of a container or clip name."""
    return re.sub(r"[^0-9A-Za-z_-]", "_", str(s))


def clip_asset_path(name, container, path_id):
    """The ONE rule for where a clip's .anim lives: Animations/<state>__<container>_<id>/<state>.anim

    It lives here because two different producers have to agree on it: export_animation_descriptors
    writes it into animation-index.json, and extract_scene writes it into every scene's bindings and
    into the AnimationClip-typed MonoBehaviour fields. When the descriptor exporter kept its own flat
    naming (<state>_<id>_<container>.anim) the index pointed at 427 files of which 0 existed, and the
    scene pass pointed at folders -- so 51 field references could never land and nothing complained.
    The file name is the state name because a legacy Animation state IS the clip's object name and
    CreateAsset pins that to the file name; the folder carries the clip identity.
    """
    key = safe_token(container).replace("_assets", "") + "_" + str(path_id)
    return "Assets/Imported/Animations/%s/%s.anim" % (safe_token(name) + "__" + key,
                                                      safe_token(name))


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
