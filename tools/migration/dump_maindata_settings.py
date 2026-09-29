"""Read-only dump of the Unity-4 build settings objects in dump/assets/mainData.

Used to reconstruct unity/ProjectSettings/{TagManager,InputManager,EditorBuildSettings}.asset.
Run:  <unitypy venv python> tools/migration/dump_maindata_settings.py [--write-json]
"""
import json
import os
import sys

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MAIN = os.path.join(_REPO, "dump", "assets", "mainData")

WANTED = {"TagManager", "InputManager", "BuildSettings", "PlayerSettings", "RenderSettings", "LightmapSettings", "NavMeshSettings"}


def main():
    env = UnityPy.load(MAIN)
    found = {}
    print("=== objects in mainData ===")
    for obj in env.objects:
        print("%-24s path_id=%-6d bytes=%d" % (obj.type.name, obj.path_id, obj.byte_size))
        if obj.type.name in WANTED:
            found[obj.type.name] = obj.read()

    print()
    for name, data in found.items():
        print("=== %s ===" % name)
        try:
            tree = data.read_typetree()
        except Exception as exc:
            print("  (typetree unavailable: %r)" % (exc,))
            print("  raw dict:", data.__dict__)
            continue
        if "--write-json" in sys.argv:
            out = os.path.join(os.environ.get("TEMP", "/tmp"), "maindata_%s.json" % name)
            with open(out, "w", encoding="utf-8") as fh:
                json.dump(tree, fh, indent=1, default=str)
            print("  -> wrote", out)
        else:
            print(json.dumps(tree, indent=1, default=str)[:4000])
        print()


if __name__ == "__main__":
    main()
