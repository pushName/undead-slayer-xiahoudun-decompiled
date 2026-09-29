"""Restore Unity 2022 ProjectSettings from the Unity 4.5.4 build data in dump/assets/mainData.

Writes:
  unity/ProjectSettings/TagManager.asset          (10 tags + 32 named layers)
  unity/ProjectSettings/EditorBuildSettings.asset (24 scenes, original order)
  tools/migration/scene-guids.tsv                 (path -> md5 guid, a ledger; see its header)
  tools/migration/maindata-physics.tsv            (authoritative physics record; NOT yet applied)

Deliberately does NOT write:
  InputManager.asset  -- see SKIP_INPUTMANAGER below.
  DynamicsManager.asset -- the Unity 4 -> 2022 hex encoding of m_LayerCollisionMatrix is
                           unconfirmed; a wrong guess silently corrupts combat physics, so the
                           values are recorded in maindata-physics.tsv instead of applied.

Run:  <unitypy venv python> tools/migration/restore_projectsettings.py [--dry-run]
"""
import hashlib
import os
import re
import shutil
import sys
import time

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MAIN = os.path.join(_REPO, "dump", "assets", "mainData")
PS_DIR = os.path.join(_REPO, "unity", "ProjectSettings")
MIG_DIR = os.path.join(_REPO, "tools", "migration")

DRY = "--dry-run" in sys.argv

SKIP_INPUTMANAGER = """\
InputManager.asset is intentionally left alone.

Evidence: mainData's 17 axes compared field-by-field with unity/ProjectSettings/InputManager.asset's
18, after normalising float32-vs-float (0.0010000000474974513 vs 0.001) and empty-string-vs-None.
14 of the 17 are identical. The whole difference is three items:

  only in the Unity 4 build data : Window Shake X, Window Shake Y (type 3 -- removed from Unity
                                   years ago and not referenced anywhere), and Fire3 bound to
                                   "left cmd" instead of 2022's "left shift".
  only in the 2022 file          : Submit (x2) and Cancel.

So the project never customised a single axis: every name in the shipped set is a Unity stock
name, and the only real delta is keyboard bindings, which are irrelevant to a touch-driven
Android build (Input.GetTouch / mousePosition / GetMouseButtonDown).

  * The scripts read no axes at all: Input.GetAxis / GetAxisRaw / GetButton / GetButtonDown /
    GetButtonUp appear 0 times across unity/Assets/Scripts. Actual input use is mousePosition(56),
    GetTouch(16), GetMouseButtonDown(13), GetMouseButtonUp(9), anyKeyDown(7), multiTouchEnabled(6),
    anyKey(5), GetMouseButton(3), touchCount(2), GetKeyDown(2).
  * Overwriting would DELETE Submit/Cancel, which the uGUI EventSystem needs once task #9 builds
    the UI.

Nothing to restore, and a real cost to restoring it.
"""


def scene_guid(path):
    """Guid for a scene path: whatever Unity actually owns, else a deterministic stand-in.

    A player build carries no source GUIDs, so md5(path) is the only reproducible value -- but it
    is a LEDGER, not a spec. Unity allocates a .unity file's guid itself when it imports it, and
    that is the value EditorBuildSettings.asset has to reference (LoadLevel by name resolves
    through it). Reading the .meta when it exists keeps this script from silently reverting a
    synced build list back to md5 on the next run.
    """
    meta = os.path.join(_REPO, "unity", path + ".meta")
    if os.path.exists(meta):
        with open(meta, "r", encoding="utf-8", errors="replace") as fh:
            m = re.search(r"^guid: ([0-9a-f]{32})\s*$", fh.read(), re.M)
        if m:
            return m.group(1)
    return hashlib.md5(path.encode("utf-8")).hexdigest()


def build_layers(tag):
    layers = []
    for i in range(32):
        if i < 8:
            layers.append(tag.get("Builtin_Layer_%d" % i, "") or "")
        else:
            layers.append(tag.get("User_Layer_%d" % i, "") or "")
    return layers


def render_tagmanager(tags, layers, sorting):
    out = ["%YAML 1.1", "%TAG !u! tag:unity3d.com,2011:", "--- !u!78 &1",
           "TagManager:", "  serializedVersion: 2"]
    out.append("  tags:" if tags else "  tags: []")
    out += ["  - %s" % t for t in tags]
    out.append("  layers:")
    # Unity writes an empty layer as a bare "- " with a trailing space.
    out += ["  - %s" % n if n else "  - " for n in layers]
    out.append("  m_SortingLayers:")
    for s in sorting:
        out.append("  - name: %s" % s["name"])
        out.append("    uniqueID: %d" % s["uniqueID"])
        out.append("    locked: %d" % s["locked"])
    return "\n".join(out) + "\n"


def render_buildsettings(levels):
    out = ["%YAML 1.1", "%TAG !u! tag:unity3d.com,2011:", "--- !u!1045 &1",
           "EditorBuildSettings:", "  m_ObjectHideFlags: 0", "  serializedVersion: 2"]
    out.append("  m_Scenes:" if levels else "  m_Scenes: []")
    for path in levels:
        out.append("  - enabled: 1")
        out.append("    path: %s" % path)
        out.append("    guid: %s" % scene_guid(path))
    out.append("  m_configObjects: {}")
    return "\n".join(out) + "\n"


# The dump's serialized level files are numbered one *below* the build index they belong to:
# dump/assets/levelN holds BuildSettings.levels[N+1]. Measured, not assumed -- all 23 extracted
# scenes were checked against this list, and the scripts each scene binds corroborate it (dump
# level7 contains UI_general.cs + General_Stat.cs, which is build index 8 = General; index 7 = Map
# is dump level6). The consequence that is easy to get wrong: the entry with NO source data is
# index 0 (Ready), while index 23 (Ending) does have a file -- an almost empty one (level22 is 1 node).
def scene_guids_tsv(levels, refs, dump_indices):
    """Render scene-guids.tsv. dump_indices = {N} for which dump/assets/levelN actually exists."""
    head = ["# Original scene build order from mainData BuildSettings.levels (Unity 4.5.4f1).",
            "# guid is md5(path) and is a LEDGER ONLY -- do not write it into the .unity.meta files.",
            "# Unity allocates those guids itself on import, so the build settings have to be re-synced",
            "# to the real guids from inside the Editor (menu: Undead Slayer > Sync Build Settings To",
            "# The Actual Scene Files). Loading a scene by name cannot resolve until that is done.",
            "# level_file_in_dump is the APK's serialized scene for that build entry. The dump numbers",
            "# those files one BELOW the build index (dump levelN == levels[N+1]), verified for all 23.",
            "# Index 0 (Ready) is the entry with no source data -- it needs a hand-made stand-in.",
            "# Index 23 (Ending) has dump level22, but that holds a single node.",
            "# calls_in_scripts counts Application.LoadLevel*\"<name>\") sites in unity/Assets/Scripts.",
            "# index\tname\tpath\tguid\tlevel_file_in_dump\tcalls_in_scripts"]
    rows = []
    for i, path in enumerate(levels):
        name = os.path.basename(path)[:-len(".unity")]
        source = "level%d" % (i - 1) if (i - 1) in dump_indices else "(no dump file)"
        rows.append("%d\t%s\t%s\t%s\t%s\t%d"
                    % (i, name, path, scene_guid(path), source, refs.get(name, 0)))
    return "\n".join(head + rows) + "\n"


def write(path, text):
    rel = os.path.relpath(path, _REPO).replace(os.sep, "/")
    old = None
    if os.path.exists(path):
        with open(path, "rb") as fh:
            old = fh.read()
    if old is not None and old.decode("utf-8") == text:
        print("  unchanged  %s" % rel)
        return False
    if DRY:
        print("  would write %s (%d bytes, was %s)"
              % (rel, len(text), "%d bytes" % len(old) if old is not None else "absent"))
        return True
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as fh:
        fh.write(text.encode("utf-8"))
    print("  wrote       %s (%d bytes, was %s)"
          % (rel, len(text), "%d bytes" % len(old) if old is not None else "absent"))
    return True


def backup_psdettings(names):
    if DRY:
        return None
    stamp = time.strftime("%Y%m%d-%H%M%S")
    dest = os.path.join(os.environ.get("TEMP", "/tmp"), "projectsettings-backup-" + stamp)
    os.makedirs(dest, exist_ok=True)
    for n in names:
        src = os.path.join(PS_DIR, n)
        if os.path.exists(src):
            shutil.copy2(src, os.path.join(dest, n))
    return dest


def physics_tsv(phys):
    matrix = list(phys.m_LayerCollisionMatrix)
    rows = ["# Unity 4.5.4f1 PhysicsManager (mainData path_id=10) -> Unity 2022.3 DynamicsManager.asset",
            "# Recorded, NOT applied. 27 of the 32 collision-matrix entries are non-default, so this",
            "# is load-bearing for combat: it decides which attack layers connect with which targets.",
            "#",
            "# Why it is not applied: m_LayerCollisionMatrix is spelled in the YAML as 256 hex chars",
            "# (32 chunks of 8). Unclear whether each chunk is the uint32's little-endian bytes or its",
            "# big-endian digits. Unity accepts either silently -- a wrong guess does not error, it",
            "# permutes every mask's bits and quietly changes which attacks land. One blog post",
            "# (CSDN 104958961) asserts little-endian; I could not confirm it offline, so I will not",
            "# write the field on that alone.",
            "#",
            "# Recommended fix, which sidesteps the encoding entirely: let Unity serialise the file.",
            "# A menu item calling Physics.IgnoreLayerCollision(i, j, ignore) for every pair (the",
            "# only public setter -- verified against UnityEngine.PhysicsModule.xml; there is no",
            "# Physics.SetLayerCollision), then AssetDatabase.SaveAssets(), writes",
            "# DynamicsManager.asset in the correct spelling. Verify by reading the 256 hex chars",
            "# back, decoding to 32 uint32, and asserting they equal the mask_hex column below.",
            "# Physics.GetIgnoreLayerCollision(a, b) observes Unity's own decoded result, and",
            "# symmetry (0 of 528 pairs violate it, versus 270 violations under the wrong spelling)",
            "# is then a cheap invariant to assert.",
            "#",
            "# Other Unity 4 physics fields, mapped:",
            "# kind\tunity4_field\tvalue\tunity2022_field\taction",
            ]
    scalar = [
        ("scalar", "m_Gravity", "(0, -9.81, 0)", "m_Gravity", "already equal - no change"),
        ("scalar", "m_BounceThreshold", "2", "m_BounceThreshold", "already equal - no change"),
        ("scalar", "m_RaycastsHitTriggers", "True", "m_QueriesHitTriggers", "already equal - no change"),
        ("scalar", "m_SolverIterationCount", "4", "m_DefaultSolverIterations", "PROPOSED: 6 -> 4 (safe, plain integer)"),
        ("scalar", "m_MaxAngularVelocity", "7", "m_DefaultMaxAngularSpeed", "PROPOSED: 50 -> 7; Unity 4's default differs from 2022's, so this is a real behaviour change either way"),
        ("scalar", "m_SleepVelocity", "0.15", "-", "NO MAPPING: Unity 5 replaced the sleep model with m_SleepThreshold; keep 2022 default 0.005"),
        ("scalar", "m_SleepAngularVelocity", "0.14", "-", "NO MAPPING: same as above"),
        ("scalar", "m_MinPenetrationForPenalty", "0.01", "-", "NO MAPPING: penalty solver removed in Unity 5"),
        ("scalar", "m_DefaultMaterial", "None (fileID 0)", "m_DefaultMaterial", "already equal - no change"),
    ]
    for row in scalar:
        rows.append("\t".join(row))
    rows.append("#")
    rows.append("# m_LayerCollisionMatrix: 32 uint32 masks, bit j of entry i = layer i collides with layer j.")
    rows.append("# Verified symmetric (0 violations over all 528 pairs), so the decoded values are correct;")
    rows.append("# only their YAML hex spelling is in question.")
    rows.append("# kind\tindex\tlayer_name\tmask_dec\tmask_hex")
    layers = LAYER_NAMES
    for i, v in enumerate(matrix):
        rows.append("matrix\t%d\t%s\t%d\t0x%08X" % (i, layers[i] or "(unnamed)", v, v))
    rows.append("#")
    rows.append("# concatenated, big-endian digits per uint32 (UNCONFIRMED):")
    rows.append("# " + "".join("%08x" % v for v in matrix))
    rows.append("# concatenated, little-endian bytes per uint32 (UNCONFIRMED):")
    rows.append("# " + "".join(
        "%02x%02x%02x%02x" % (v & 0xFF, (v >> 8) & 0xFF, (v >> 16) & 0xFF, (v >> 24) & 0xFF)
        for v in matrix))
    return "\n".join(rows) + "\n"


LAYER_NAMES = []


def scene_references():
    """Which scene names do the scripts actually LoadLevel()? Returns Counter-like dict.

    Application.LoadLevel(string) resolves against the EditorBuildSettings scene list, so
    this is the check that the 24 restored entries cover every scene the code can ask for.
    """
    import re
    scripts = os.path.join(_REPO, "unity", "Assets", "Scripts")
    pat = re.compile(r'LoadLevel\w*\s*\(\s*"([^"]+)"')
    counts = {}
    for root, _dirs, files in os.walk(scripts):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            with open(os.path.join(root, fn), "r", encoding="utf-8", errors="replace") as fh:
                for m in pat.finditer(fh.read()):
                    counts[m.group(1)] = counts.get(m.group(1), 0) + 1
    return counts


def main():
    env = UnityPy.load(MAIN)
    objs = {}
    for obj in env.objects:
        if obj.type.name in ("TagManager", "InputManager", "BuildSettings", "PhysicsManager"):
            objs[obj.type.name] = obj.read()

    tag = objs["TagManager"]
    bld = objs["BuildSettings"]
    phys = objs["PhysicsManager"]

    raw_tags = list(tag.tags)
    # Unity 4 pads the tag array with a trailing empty string.
    tags = [t for t in raw_tags if t]
    dropped = len(raw_tags) - len(tags)

    layers = build_layers(vars(tag))
    LAYER_NAMES.extend(layers)
    assert len(layers) == 32, layers

    sorting = [{"name": s.name, "uniqueID": int(s.uniqueID), "locked": 0}
               for s in (tag.m_SortingLayers or [])] or [{"name": "Default", "uniqueID": 0, "locked": 0}]

    levels = list(bld.levels)

    matrix = list(phys.m_LayerCollisionMatrix)
    violations = [(i, j) for i in range(32) for j in range(i + 1, 32)
                  if ((matrix[i] >> j) & 1) != ((matrix[j] >> i) & 1)]
    assert not violations, "collision matrix is not symmetric: %s" % violations[:5]

    print("mainData (Unity %s) read OK" % bld.m_Version)
    print("  tags   : %d kept, %d empty padding dropped -> %s" % (len(tags), dropped, tags))
    print("  layers : %d/32 named" % sum(1 for n in layers if n))
    print("  scenes : %d" % len(levels))
    print("  physics: collision matrix symmetric, %d of 32 entries are non-default"
          % sum(1 for v in matrix if v != 0xFFFFFFFF))
    print()

    dest = backup_psdettings(["TagManager.asset", "EditorBuildSettings.asset"])
    if dest:
        print("backup -> %s" % dest)

    print("writing:")
    write(os.path.join(PS_DIR, "TagManager.asset"), render_tagmanager(tags, layers, sorting))
    write(os.path.join(PS_DIR, "EditorBuildSettings.asset"), render_buildsettings(levels))

    dump_assets = os.path.join(_REPO, "dump", "assets")
    present = {int(m.group(1)) for m in
               (re.fullmatch(r"level(\d+)", n) for n in os.listdir(dump_assets)) if m}
    expected = set(range(len(levels) - 1))
    if present != expected:
        print("  !! dump level files do not match the build-index-minus-one convention:")
        print("     extra=%s  missing=%s" % (sorted(present - expected), sorted(expected - present)))

    refs = scene_references()
    listed = {os.path.basename(p)[:-len(".unity")] for p in levels}
    write(os.path.join(MIG_DIR, "scene-guids.tsv"), scene_guids_tsv(levels, refs, present))

    orphan_calls = sorted(set(refs) - listed)
    never_loaded = sorted(listed - set(refs))
    print("  LoadLevel cross-check: %d distinct names called, %d of 24 build entries targeted"
          % (len(refs), len(set(refs) & listed)))
    if orphan_calls:
        print("  !! called by LoadLevel but NOT in the build list (would fail at runtime): %s"
              % orphan_calls)
    else:
        print("  ok: every LoadLevel target exists in the build list")
    print("  entries never loaded by name (reached another way): %s" % never_loaded)
    print()

    write(os.path.join(MIG_DIR, "maindata-physics.tsv"), physics_tsv(phys))

    print()
    print(SKIP_INPUTMANAGER)


if __name__ == "__main__":
    main()
