"""Read-only dump of mainData's PhysicsManager / TimeManager / QualitySettings / AudioManager.

Task #8's stated scope is Tag/Input/BuildSettings, but the layer collision matrix lives in
PhysicsManager and is index-based -- i.e. it is the one place where the layer indices used
by 2429 shipped GameObjects actually get their meaning. Worth looking at before deciding.

Run:  <unitypy venv python> tools/migration/dump_maindata_physics.py
"""
import os
import pprint

import UnityPy

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MAIN = os.path.join(_REPO, "dump", "assets", "mainData")

WANTED = {"PhysicsManager", "TimeManager", "QualitySettings", "AudioManager", "NavMeshProjectSettings"}

SKIP = {"object_reader"}


def main():
    env = UnityPy.load(MAIN)
    for obj in env.objects:
        if obj.type.name not in WANTED:
            continue
        data = obj.read()
        print("=== %s (path_id=%d) ===" % (obj.type.name, obj.path_id))
        fields = {k: v for k, v in vars(data).items() if k not in SKIP and v is not None}
        pprint.pprint(fields, width=140, sort_dicts=True)
        print()


if __name__ == "__main__":
    main()
