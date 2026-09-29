"""Read-only: can Animation components be bound to clips at all, and is the binding checkable?

The problem this settles
Unity 4 legacy animation resolves Play("idle") through an AnimationContainer that lives on the
imported model. This APK has ZERO AnimationContainer objects and every Animation component has
m_Clip == null, so the binding information is not in a pointer. What IS in the data is each
clip's curve target paths ("Bip01/Bip01 Pelvis/...", relative to the Animation's Transform), and
the scenes contain the same bone names as real GameObjects. So the rule under test is:

    clip belongs to this Animation  <=>  every curve target path of the clip resolves to a
                                         descendant Transform of that Animation's GameObject

Verification is on the referring side, which is the only independent check available: the script
on that object calls Play("name"). If the binding is right, the names that script plays must be
inside the bound clip set. A binding that satisfies the containment rule but not the Play names
is wrong, so both are printed per object rather than aggregated into one number.

Run:  python tools/migration/scan_animation_binding.py   (system python, reads JSON only)
"""
import collections
import glob
import json
import os
import re
import sys

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCENES = os.path.join(_REPO, "tools", "migration", "scenes")
SURVEY = os.path.join(_REPO, "tools", "migration", "animations-survey.json")
SCRIPTS = os.path.join(_REPO, "unity", "Assets", "Scripts")

NAME_USE = re.compile(
    r'(?:Play|PlayQueued|CrossFade|CrossFadeQueued|SampleAnimation|IsPlaying)\s*\(\s*"([^"]+)"'
    r'|\[\s*"([^"]+)"\s*\]')


def scene_nodes(level_json):
    with open(level_json, encoding="utf-8") as fh:
        doc = json.load(fh)
    nodes = {n["id"]: n for n in doc.get("nodes", [])}
    by_go = {n.get("go_id"): n for n in doc.get("nodes", [])}
    return doc, nodes, by_go


def descendant_paths(nodes, root_id):
    """All '/'-joined relative paths of descendants, keyed the way Unity keys curve paths."""
    out = set()

    def walk(nid, prefix):
        for cid in nodes[nid].get("children") or []:
            child = nodes.get(cid)
            if child is None:
                continue
            p = prefix + child.get("name", "")
            out.add(p)
            walk(cid, p + "/")

    walk(root_id, "")
    return out


def animation_scripts_per_level(doc, nodes):
    """Animation component -> the MonoBehaviour scripts on the same GameObject.

    The level JSON stores native components as {"kind": "component", "type": "..."} -- kind
    alone is only component/renderer/unsupported, so matching on kind == "Animation" finds
    nothing (it did, and reported a clean zero).
    """
    hits = []
    for n in doc.get("nodes", []):
        comps = n.get("components", [])
        if not any(c.get("type") == "Animation" for c in comps):
            continue
        scripts = [c.get("script") for c in comps
                   if c.get("kind") == "MonoBehaviour" and c.get("script")]
        hits.append((n, scripts))
    return hits


def played_names_per_script():
    """script class -> set of animation state names its code plays."""
    out = collections.defaultdict(set)
    for root, _d, files in os.walk(SCRIPTS):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            with open(os.path.join(root, fn), encoding="utf-8", errors="replace") as fh:
                text = fh.read()
            if "GetComponent<Animation>" not in text and "animation" not in text:
                continue
            cls = fn[:-3]
            for m in NAME_USE.finditer(text):
                nm = next((g for g in m.groups() if g), None)
                if nm:
                    out[cls].add(nm)
    return out


def animation_objects(doc, nodes):
    """-> (node, scripts on it, descendant paths, descendant nodes).

    The SkinnedMeshRenderer is NOT on the Animation's own GameObject -- it sits on a descendant
    (the character root only carries Animation + scripts), so reading mesh references off the node
    itself yields an empty set and every same-file rule binds nothing.
    """
    for node, scripts in animation_scripts_per_level(doc, nodes):
        subs = descendant_paths(nodes, node["id"])
        desc = []

        def collect(nid):
            for cid in nodes[nid].get("children") or []:
                if cid in nodes:
                    desc.append(nodes[cid])
                    collect(cid)

        collect(node["id"])
        yield node, scripts, subs, desc


def mesh_files_of(subtree_nodes):
    """The sharedassetsN files behind every Mesh reference in the subtree.

    export_meshes.py sanitises the origin into the file NAME with underscores
    (cha1_1_sharedassets2_assets_110.asset); matching "sharedassets2.assets" returns nothing and
    the rule silently binds 0 clips -- which is what the first run of this comparison did.
    """
    out = set()
    for n in subtree_nodes:
        for c in n.get("components", []):
            v = c.get("mesh")
            if not v:
                continue
            m = re.search(r"_(sharedassets\d+)_assets_", str(v))
            if m:
                out.add(m.group(1) + ".assets")
    return out


def rule_compare(clips, played, nodes_by_level):
    """Score three candidate binding rules on two axes: names satisfied, and clips over-bound."""
    rules = {
        "containment": lambda c, subs, files: bool(c["paths"]) and all(p in subs for p in c["paths"]),
        "containment+samefile": lambda c, subs, files: bool(c["paths"]) and c["file"] in files
                                                     and all(p in subs for p in c["paths"]),
        "samefile+exactpathset": lambda c, subs, files: bool(c["paths"]) and c["file"] in files
                                                        and set(c["paths"]) <= subs
                                                        and len(c["paths"]) >= max(1, len(subs) // 2),
    }
    print("\n=== rule comparison (which binding can the export use?) ===")
    for label, pred in rules.items():
        sat = unsat = 0
        total_bound = 0
        objects = 0
        misses = []
        for level, doc, nodes in nodes_by_level:
            for node, scripts, subs, desc in animation_objects(doc, nodes):
                files = mesh_files_of(desc)
                want = set()
                for s in scripts:
                    want |= played.get(s, set())
                if not want:
                    continue
                objects += 1
                bound = [c for c in clips if pred(c, subs, files)]
                total_bound += len(bound)
                have = {c["name"] for c in bound}
                missing = sorted(want - have)
                if missing:
                    unsat += 1
                    misses.append((level, node.get("name"), missing[:5], len(bound),
                                   sorted(files)))
                else:
                    sat += 1
        print("  %-22s objects checked %2d  all-names-satisfied %2d  unsatisfied %2d  "
              "avg clips/object %.1f"
              % (label, objects, sat, unsat, total_bound / max(1, objects)))
        for m in misses[:6]:
            print("        %s %s missing %s (bound %d, mesh files %s)" % m)

    # Rule 4: bind by the referring side. Over-binding is harmless for legacy Animation (an
    # unused state costs nothing), an unresolved Play("name") is not -- so the names the object's
    # own scripts ask for are the requirement, and the inference rules above only decide which
    # same-named clip fills the slot.
    print("  %-22s (referring-side rule)" % "name-driven")
    objects = sat = unsat = 0
    total_bound = 0
    dup_unresolved = []
    no_clip = collections.Counter()
    for level, doc, nodes in nodes_by_level:
        for node, scripts, subs, desc in animation_objects(doc, nodes):
            files = mesh_files_of(desc)
            want = set()
            for s in scripts:
                want |= played.get(s, set())
            if not want:
                continue
            objects += 1
            missing = []
            for name in sorted(want):
                same = [c for c in clips if c["name"] == name]
                if not same:
                    no_clip[name] += 1
                    missing.append(name)
                    continue
                total_bound += 1
                if len(same) > 1:
                    pick = [c for c in same if c["file"] in files] or \
                           [c for c in same if all(p in subs for p in c["paths"])]
                    if len(pick) != 1:
                        dup_unresolved.append((level, node.get("name"), name, len(same),
                                               [c["file"] for c in same]))
            if missing:
                unsat += 1
            else:
                sat += 1
    print("        objects %d  all names bindable %d  with at least one unusable name %d  "
          "states registered %d" % (objects, sat, unsat, total_bound))
    print("        names the scripts play that exist in NO clip: %s" % no_clip.most_common(20))
    print("        same-name slots the tiebreakers could not resolve: %d" % len(dup_unresolved))
    for d in dup_unresolved[:10]:
        print("           %s %s '%s' has %d clips in %s" % d)


def main():
    if not os.path.exists(SURVEY):
        sys.exit("run scan_animations.py first -- %s is missing" % SURVEY)
    survey = json.load(open(SURVEY, encoding="utf-8"))
    clips = survey["clips"]
    by_name = collections.defaultdict(list)
    for c in clips:
        by_name[c["name"]].append(c)

    clip_paths = collections.Counter()
    for c in clips:
        for p in c["paths"]:
            clip_paths[p] += 1

    played = played_names_per_script()
    print("clips %d, distinct names %d, scripts that touch Animation %d"
          % (len(clips), len(by_name), len(played)))

    nodes_by_level = []
    for jf in sorted(glob.glob(os.path.join(SCENES, "level*.json")),
                     key=lambda p: int(re.findall(r"\d+", os.path.basename(p))[0])):
        level = os.path.basename(jf)[:-5]
        doc, nodes, _by_go = scene_nodes(jf)
        if any(c.get("type") == "Animation" for n in doc.get("nodes", [])
               for c in n.get("components", [])):
            nodes_by_level.append((level, doc, nodes))

    rule_compare(clips, played, nodes_by_level)

    total_anim = bound_objects = 0
    sat = unsat = 0
    unsat_detail = []
    ambiguous = []
    orphan_clips = collections.Counter()
    per_level_report = []
    used_global = set()

    for jf in sorted(glob.glob(os.path.join(SCENES, "level*.json")),
                     key=lambda p: int(re.findall(r"\d+", os.path.basename(p))[0])):
        level = os.path.basename(jf)[:-5]
        doc, nodes, _by_go = scene_nodes(jf)
        anims = animation_scripts_per_level(doc, nodes)
        if not anims:
            continue
        used_here = set()
        rows = []
        for node, scripts in anims:
            total_anim += 1
            subs = descendant_paths(nodes, node["id"])
            # A clip binds here if every one of its curve paths is a descendant path of this
            # Animation's GameObject (or the clip has no paths at all, which cannot bind).
            matches = [c for c in clips if c["paths"] and all(p in subs for p in c["paths"])]
            want = set()
            for s in scripts:
                want |= played.get(s, set())
            have = {c["name"] for c in matches}
            missing = sorted(want - have)
            for c in matches:
                used_here.add(id(c))
                used_global.add(id(c))
            if want:
                if not missing:
                    sat += 1
                else:
                    unsat += 1
                    unsat_detail.append((level, node.get("name"), sorted(scripts), len(matches),
                                         missing[:6]))
            if len({c["name"] for c in matches}) != len(matches):
                ambiguous.append((level, node.get("name"),
                                  [n for n, k in collections.Counter(
                                      c["name"] for c in matches).items() if k > 1][:6]))
            rows.append((node.get("name"), sorted(scripts), len(subs), len(matches),
                         sorted(have)[:8], missing[:6]))
        per_level_report.append((level, rows))

    # Orphans are clips no scene Animation object can host; computed once, globally, because a
    # clip unused in ONE level is not otherwise bound somewhere else.
    for c in clips:
        if c["paths"] and id(c) not in used_global:
            orphan_clips[c["name"]] += 1

    print("\n=== per-scene binding, Animation components in the 23 rebuilt scenes ===")
    for level, rows in per_level_report:
        print("\n%s" % level)
        for name, scripts, sub_n, match_n, have, missing in rows:
            print("  %-18s paths=%-3d matched=%-3d scripts=%s" % (name, sub_n, match_n, scripts))
            print("       has: %s" % (have,))
            if missing:
                print("       PLAY NAMES NOT SATISFIED: %s" % (missing,))

    print("\n=== summary ===")
    print("  Animation components in scenes        : %d" % total_anim)
    print("  objects whose code plays something and ALL names bound   : %d" % sat)
    print("  objects whose code plays something and SOME name missing : %d" % unsat)
    if unsat_detail:
        print("  first missing cases:")
        for r in unsat_detail[:12]:
            print("    %s %s scripts=%s matched=%d missing=%s" % r)
    if ambiguous:
        print("  same clip name bound twice to one object (duplicate bone names): %d case(s)"
              % len(ambiguous))
        for a in ambiguous[:8]:
            print("    %s %s dup=%s" % a)
    print("  clips never bound to any scene Animation object: %d of %d (names x count) -- these "
          "belong to prefabs/runtime spawns" % (len(orphan_clips), len(clips)))
    print("  e.g. %s" % orphan_clips.most_common(14))
    return 0


if __name__ == "__main__":
    sys.exit(main())
