"""Read-only: which tags / layers does the shipped asset data actually use?

Answers two questions before we write unity/ProjectSettings/TagManager.asset:
  1. Are the 10 tags in mainData's TagManager load-bearing (i.e. referenced by
     GameObject.m_TagString in the asset files)? If not, restoring them is cosmetic.
  2. Does the data reference any tag or layer index that TagManager does NOT define?
     That would mean the shipped TagManager is incomplete.

Run:  <unitypy venv python> tools/migration/scan_tags_layers.py
"""
import collections
import glob
import os

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
DUMP_ASSETS = os.path.join(_REPO, "dump", "assets")
RECON = os.environ.get("RECON_DIR") or os.path.join(os.environ.get("TEMP", "/tmp"), "recon")

TAGMANAGER_TAGS = ['efs_mon', 'ui', 'p_plan', 'skill', 'icon_skill', 'sound', 'dummy',
                   'general', 'ranking', 'cashshop']
LAYERS = {
    0: 'Default', 1: 'TransparentFX', 2: 'Ignore Raycast', 3: '', 4: 'Water', 5: 'UI',
    6: '', 7: '', 8: 'unit', 9: 'GUI', 10: 'obj', 11: 'loading1', 12: 'general',
    13: 'enemyattack', 14: '', 15: 'ally_behit_collider', 16: 'stunattack',
    17: 'riseupattack2', 18: 'shockattack', 19: 'darkattack', 20: 'normalattack',
    21: 'powerattack', 22: 'skillattack', 23: 'poisonattack', 24: 'deathattack',
    25: 'generalattack', 26: 'pierceattack', 27: 'crowdattack', 28: 'pushattack',
    29: 'riseupattack', 30: 'burnattack', 31: 'coldattack',
}


def targets():
    files = sorted(glob.glob(os.path.join(DUMP_ASSETS, "level*")))
    files += sorted(glob.glob(os.path.join(DUMP_ASSETS, "sharedassets*.assets")))
    files += sorted(glob.glob(os.path.join(RECON, "sharedassets*.assets")))
    return files


def main():
    tags = collections.Counter()
    layers = collections.Counter()
    per_file_tags = {}
    scanned = failed = 0

    for path in targets():
        base = os.path.basename(path)
        try:
            env = UnityPy.load(path)
        except Exception as exc:
            failed += 1
            print("LOAD FAIL %s: %r" % (base, exc))
            continue
        n_go = 0
        for obj in env.objects:
            if obj.type.name != "GameObject":
                continue
            try:
                data = obj.read()
            except Exception:
                continue
            n_go += 1
            tags[getattr(data, "m_TagString", "") or ""] += 1
            layers[getattr(data, "m_Layer", 0)] += 1
        if n_go:
            scanned += 1
            per_file_tags[base] = n_go

    print("scanned files with GameObjects: %d   load failures: %d" % (scanned, failed))
    print("total GameObjects: %d" % sum(tags.values()))
    print()
    print("=== m_TagString distribution ===")
    BUILTIN = ("Untagged", "Player", "MainCamera", "EditorOnly", "GameController",
               "Respawn", "Finish")
    for tag, n in tags.most_common():
        # Unity serializes "Untagged" as the empty string.
        label = tag if tag else "Untagged (empty)"
        if not tag or tag in BUILTIN:
            defined = "builtin"
        elif tag in TAGMANAGER_TAGS:
            defined = "DEFINED in mainData TagManager"
        else:
            defined = "!! NOT IN TAGMANAGER !!"
        print("%7d  %-20s %s" % (n, label, defined))
    print()
    print("=== m_Layer distribution (index: name from mainData TagManager) ===")
    for layer, n in sorted(layers.items()):
        name = LAYERS.get(layer, "!! OUT OF RANGE !!")
        print("%7d  layer %-3d %s" % (n, layer, name if name else "(unnamed)"))


if __name__ == "__main__":
    main()
