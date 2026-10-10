"""Write the visual A/B sheet: decoded particle numbers that only the running original game can name.

Where this sits in the work. particle_layouts.py proved the *structure* of the Unity 4 particle
records -- 363 of 363 decode under an exact-byte constraint, and it lists four adjacent float pairs
per emitter that satisfy lo<=hi in every record and separate in at least one, which is what a
(minimum, maximum) pair looks like from the outside. What nothing in the APK can supply is the NAME
of those pairs. There is no TypeTree in these Unity 4 files, none in the venv, and the shipped
Assembly-CSharp.dll references only eight particle members (ParticleEmitter, emit, inside, radius,
force, rndVelocity, Particles, ClearParticles) -- never maxParticles, min|maxSize, min|maxEmitPower,
colorKey, sizeKey or damping. The look was authored in the editor and no shipped program speaks its
vocabulary.

So this sheet pairs each candidate with the two things that ARE observable:

  * which sprite the effect draws (resolved through the exported material), because "an emitter with
    15 at offset 28" is not recognisable but a particular explosion is; and
  * four questions about what the effect does on screen -- how long a particle lives, how fast it
    travels, how big it is, how many at once.

Those four answers are exactly the four pairs. Fill in what you can see; anything you mark as
unclear stays un-named in the converter rather than becoming a plausible-looking guess.

Run:  python tools/migration/particle_tuning_sheet.py [--limit N] [--lang en]
"""
import argparse
import collections
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4        # only to turn a material PPtr's FileID into the file it names

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MIG = os.path.join(_REPO, "tools", "migration")
UNITY = os.path.join(_REPO, "unity")
OUT = os.path.join(MIG, "particle-tuning-sheet.md")

PAIRS = [(12, 16), (24, 28), (80, 84), (88, 92)]

STRINGS = {
    "zh": {
        "title": "# 旧粒子特效 对表清单",
        "lead": [
            "这一份把 APK 里解出来的数字，换成只能在原版画面上确认的东西。",
            "",
            "已经证到的：四类组件的记录长度在全 build 内完全一致（116 / 90 / 104 / 120 字节），",
            "363 条记录在「每条字节必须用尽」的约束下全部解出；发射器里有 4 对相邻 float，",
            "每条记录都满足 前<=后 且至少一条严格小于——这就是 (最小, 最大) 参数的样子。",
            "",
            "还缺的：这四对分别管什么。离线渠道已经走尽——Unity 4 这批文件不带 TypeTree，",
            "venv 里也没有 typetree 签名库；出厂 Assembly-CSharp.dll 的字符串堆里只出现 8 个粒子成员名",
            "（ParticleEmitter / emit / inside / radius / force / rndVelocity / Particles / ClearParticles），",
            "maxParticles、min|maxSize、min|maxEmitPower、colorKey、damping 一个都没有。",
            "也就是说外观是当年在编辑器里设的，APK 里没有任何程序提到过它们的名字。",
            "",
            "所以每条给了两样可对表的东西：这个特效用的是哪张贴图（靠已导出的材质反查），",
            "以及四个只能靠眼睛回答的量。填得上就填，看不出就写「看不出来」，",
            "我按未定名处理，不会把猜的值写进转换器。",
        ],
        "src": "出处",
        "inlevel": "关卡里的对象名",
        "notlevel": "不在 23 个关卡里，多半是 prefab 内部对象",
        "mat": "材质",
        "tex": "纹理（认画面用这个）",
        "unresolved": "未解析",
        "flags": "打包标志",
        "pairs": "四对 min/max 候选",
        "alpha": "透明度轨（5 帧 alpha）",
        "noanim": "没有配对的动画器记录",
        "qhead": "| 观察项 | 你在原版里看到的 |",
        "qrow": ["单个粒子寿命（秒）", "初速（米/秒，估）",
                 "粒子大小（相对角色身高）", "同屏峰值粒子数"],
    },
}


def guid_to_texture():
    """guid -> project-relative path, for anything under unity/Assets/Imported."""
    out = {}
    for root, _d, files in os.walk(os.path.join(UNITY, "Assets")):
        if os.sep + "Imported" + os.sep not in root + os.sep:
            continue
        for fn in files:
            if not fn.endswith(".meta"):
                continue
            try:
                txt = open(os.path.join(root, fn), encoding="utf-8", errors="replace").read(200)
            except OSError:
                continue
            m = re.search(r"guid: ([0-9a-f]{32})", txt)
            if m:
                out[m.group(1)] = os.path.relpath(os.path.join(root, fn)[:-5], UNITY).replace(
                    os.sep, "/")
    return out


def texture_of(mat_rel, guids):
    if not mat_rel:
        return None
    full = os.path.join(UNITY, mat_rel)
    if not os.path.exists(full):
        return None
    txt = open(full, encoding="utf-8", errors="replace").read()
    for key in ("_MainTex", "m_Texture", "_MainTex_ST", "_BumpMap"):
        m = re.search(re.escape(key) + r":[^\n]*guid: ([0-9a-f]{32})", txt)
        if m:
            return guids.get(m.group(1), "guid " + m.group(1)[:12] + "（贴图不在 Imported 里）")
    return None


def triplets(records_doc, mats, guids, level_nodes, resolve, prefab_of, root_of):
    by_owner = collections.defaultdict(dict)
    for key, rec in records_doc["records"].items():
        by_owner[(key.split("!")[0], rec["owner"])][
            rec["class"]] = (key, rec)
    out = []
    for (cont, owner), trio in by_owner.items():
        em = trio.get("EllipsoidParticleEmitter") or trio.get("MeshParticleEmitter")
        if not em:
            continue
        f = em[1]["fields"]
        rk = trio.get("ParticleRenderer", (None, {}))[1].get("fields", {}).get("material0_32")
        # The material is usually in another file: m_Materials' first int is an index into this
        # container's externals, and skipping that step silently misses most of the textures --
        # which would leave the sheet unusable for recognition, the one thing it is for.
        mat = mats.get(resolve(cont, rk[0], rk[1])) if rk else None
        anim = trio.get("ParticleAnimator", (None, {}))[1].get("fields", {})
        alpha = [anim.get("color_%d" % o, [0, 0, 0, 0])[3] for o in (12, 16, 20, 24, 28)] \
            if anim else None
        pairs = [(a, b, f.get("f_%d" % a), f.get("f_%d" % b)) for a, b in PAIRS
                 if f.get("f_%d" % a) is not None]
        out.append({"key": em[0], "container": cont, "owner": owner, "pairs": pairs,
                    "flags_8": [f.get("flag_%d" % i) for i in range(8, 12)],
                    "flags_96": [f.get("flag_%d" % i) for i in range(96, 100)],
                    "mat": mat, "tex": texture_of(mat, guids), "alpha": alpha,
                    "level": level_nodes.get((cont, owner)),
                    "prefab": None if level_nodes.get((cont, owner)) else prefab_of(cont, owner),
                    # when a container holds many roots, the only way to say which prefab this
                    # emitter belongs to is to climb its own Transform chain
                    "root": None if level_nodes.get((cont, owner)) else root_of(cont, owner)})
    return out


def pick(out, limit):
    """Spread the sheet over the integer pair's range, then add objects that live in a level.

    Sorting on one measurable quantity and taking quartile points is deliberate: the point of the
    sheet is to tell two pairs apart, and entries whose numbers are all equal teach nothing.
    """
    def key_of(c):
        for _a, _b, lo, hi in c["pairs"]:
            if isinstance(hi, (int, float)) and hi == int(hi) and hi > 1:
                return hi
        return 0
    out = sorted(out, key=key_of)
    idxs = [0, len(out) // 4, len(out) // 2, 3 * len(out) // 4, len(out) - 1]
    picks = [out[i] for i in idxs if i < len(out)]
    seen = {p["key"] for p in picks}
    for c in out:
        if len(picks) >= limit:
            break
        if c["key"] not in seen and (c["level"] or c["tex"]):
            picks.append(c)
            seen.add(c["key"])
    return picks[:limit], out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--limit", type=int, default=12)
    ap.add_argument("--out", default=OUT)
    args = ap.parse_args()
    T = STRINGS["zh"]
    rp = os.path.join(MIG, "particle-records.json")
    if not os.path.exists(rp):
        sys.exit("particle-records.json missing -- run particle_layouts.py --out first")
    doc = json.load(open(rp, encoding="utf-8"))
    mats = json.load(open(os.path.join(MIG, "materials.json"), encoding="utf-8"))["index"]
    guids = guid_to_texture()
    print("indexed %d guid(s) under unity/Assets (Imported only)" % len(guids))

    level_nodes = {}
    for jp in sorted(glob.glob(os.path.join(MIG, "scenes", "level*.json"))):
        d = json.load(open(jp, encoding="utf-8"))
        for n in d["nodes"]:
            level_nodes[(d["source_level"], n.get("go_id"))] = n["name"]

    ext = {}

    def resolve(cont, file_id, path_id):
        if not file_id:
            return "%s!%d" % (cont, path_id)
        if cont not in ext:
            src = {b: p for b, p in u4.sources() if b == cont}
            if not src:
                ext[cont] = []
            else:
                import UnityPy
                ext[cont] = u4.externals_of(UnityPy.load(src[cont]))
        tgt = u4.resolve_target(cont, ext[cont], file_id)
        return "%s!%d" % (tgt, path_id) if tgt else "%s!%d" % (cont, path_id)

    # For an object that is not in a level, name the prefab it lives in. Hash-named bundles hold a
    # single root, so the answer is exact there; in sharedassetsN it is only a count, and saying so
    # is better than implying we know which one.
    pc = json.load(open(os.path.join(MIG, "prefab-contents.json"), encoding="utf-8"))["per_root"]
    roots_by_file = collections.defaultdict(list)
    for key, v in pc.items():
        roots_by_file[key.split("!")[0]].append(v["name"])

    def prefab_of(cont, owner):
        names = roots_by_file.get(cont) or []
        if len(names) == 1:
            return names[0]
        if names:
            return "%d 个根对象之一（同文件 %s）" % (len(names), cont)
        return None

    hier = {}

    def root_of(cont, go_pid):
        """Climb m_Father to the root GameObject's name, or None if the file cannot be opened."""
        if cont not in hier:
            src = {b: p for b, p in u4.sources() if b == cont}
            if not src:
                hier[cont] = None
            else:
                import UnityPy
                objs = {o.path_id: o for o in UnityPy.load(src[cont]).objects}
                names, dad = {}, {}
                for pid, o in objs.items():
                    try:
                        d = o.read()
                    except Exception:
                        continue
                    if o.type.name == "GameObject":
                        names[pid] = str(d.m_Name)
                    elif o.type.name == "Transform":
                        gop = int(getattr(d.m_GameObject, "m_PathID", 0) or 0)
                        fa = d.m_Father
                        if gop and fa is not None and int(getattr(fa, "m_FileID", 0) or 0) == 0:
                            ftp = int(getattr(fa, "m_PathID", 0) or 0)
                            if ftp:
                                ft = objs.get(ftp)
                                if ft is not None:
                                    fd = ft.read()
                                    dad[gop] = int(getattr(fd.m_GameObject, "m_PathID", 0) or 0)
                hier[cont] = (names, dad)
        if not hier[cont]:
            return None
        names, dad = hier[cont]
        cur, guard = go_pid, 0
        while dad.get(cur) and guard < 64:
            cur = dad[cur]
            guard += 1
        return names.get(cur)

    all_t = triplets(doc, mats, guids, level_nodes, resolve, prefab_of, root_of)
    if not all_t:
        print("NO TRIPLETS -- nothing to put on the sheet; the records file is not what this expects")
        return 2
    picks, ordered = pick(all_t, args.limit)
    have_tex = sum(1 for c in picks if c["tex"])
    print("emitter triplets: %d, sheet entries: %d, of which resolvable to a texture: %d"
          % (len(all_t), len(picks), have_tex))
    if have_tex == 0:
        print("WARNING: not one entry has a texture, so the sheet cannot be used to recognise an"
              " effect by eye. That is a materials/textures resolution gap, not a particle one.")

    lines = [T["title"], ""] + T["lead"] + [""]
    for i, c in enumerate(picks, 1):
        # A heading of `hash!37` is not something anyone can look up in a running game, so the
        # name wins wherever it exists -- the level object name, else the single prefab that bundle
        # holds -- and the raw key stays next to it for the converter.
        label = c["level"] or c["root"] or (
            c["prefab"] if isinstance(c["prefab"], str) and not c["prefab"][0].isdigit() else None)
        lines += ["## %d. %s（`%s`）" % (i, label or "未命名对象", c["key"]), "",
                  "- %s：`%s`（%s：%s%s）" % (
                      T["src"], c["key"], T["inlevel"],
                      c["level"] or T["notlevel"],
                      "" if c["level"] or not (c["root"] or c["prefab"])
                      else "，属于 %s" % (c["root"] or c["prefab"])),
                  "- %s：`%s`" % (T["mat"], c["mat"] or T["unresolved"]),
                  "- %s：`%s`" % (T["tex"], c["tex"] or T["unresolved"]),
                  "- %s：[8..11]=%s，[96..99]=%s" % (T["flags"], c["flags_8"], c["flags_96"]),
                  "- %s：%s" % (T["pairs"], "、".join("`%d/%d=%s..%s`" % (a, b, lo, hi)
                                                     for a, b, lo, hi in c["pairs"])),
                  "- %s：%s" % (T["alpha"], c["alpha"] or T["noanim"]), "",
                  T["qhead"], "|---|---|"]
        lines += ["| %s |  |" % q for q in T["qrow"]] + [""]
    with open(args.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write("\n".join(lines))
    print("wrote %s" % args.out)
    return 0


if __name__ == "__main__":
    sys.exit(main())
