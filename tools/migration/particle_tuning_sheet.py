"""Generate the comparison sheet for one pass through the original game.

Where this sits. particle_layouts.py proved the record shapes (363/363 records decode under an
exact-byte constraint) and narrowed roles by population-wide elimination. That elimination is honest
but weak: the engine's plausible ranges overlap almost completely (a size, a speed and a rate are all
"small positive floats"), so only the integer-valued slots reduced to two candidates. Absolute
measurement from memory ("how long does one particle live, roughly?") is what that leaves, and it is
the kind of question people answer badly.

So this sheet is built as differential comparison instead. For the slots whose values spread widest
across the emitter records, it takes the record at the bottom of the spread and the record at the top,
names both by the sprite they draw, and asks which visible dimension separates them. One answer
carries more than an absolute estimate: if the effect with offset 28 = 200 is the denser cloud,
"28 is a count" gains support, and if it is the sparser one that role is dead for every record at
once, because the field order is fixed across the whole build.

Answers go into the markdown itself -- one table per question -- so there is a single place saying
what was observed, and the converter can read it back without me re-transcribing numbers.

Run:  <unitypy venv python> tools/migration/particle_tuning_sheet.py [--limit N]
      RECON_DIR=<path to recon>     (needed to name the prefab each effect belongs to)
"""
import argparse
import collections
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
MIG = os.path.join(_REPO, "tools", "migration")
UNITY = os.path.join(_REPO, "unity")
OUT = os.path.join(MIG, "particle-tuning-sheet.md")

# the quantities a particle effect is actually judged by, in the words a player would use
DIMENSIONS = [("数量", "同屏粒子颗数（密度）"),
              ("大小", "单颗粒子的视觉尺寸"),
              ("速度", "粒子飞出去有多快"),
              ("持久", "粒子从出现到消失活多久"),
              # the direction rows are not decoration: the slots with both signs in their population
              # have already been narrowed to "an angle or a vector component", so an answer limited
              # to the four rows above could never land on that role.
              ("方向", "粒子有没有朝向/旋转的差别"),
              ("明暗", "颜色浓淡与透明度")]


def guid_to_texture():
    out = {}
    imported = os.path.join(UNITY, "Assets", "Imported")
    for root, _d, files in os.walk(imported):
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
    for key in ("_MainTex", "m_Texture", "_BumpMap"):
        m = re.search(re.escape(key) + r":[^\n]*guid: ([0-9a-f]{32})", txt)
        if m:
            return guids.get(m.group(1))
    return None


def spread(vals):
    """How wide a slot's values are, as a ratio -- what makes two records comparable at all.

    A ratio is only quoted when the whole population shares a sign: -4..5 is a change of direction,
    not "5000 times bigger", and printing it as a multiple would be a number with no meaning.
    """
    pos = sorted(abs(v) for v in vals if abs(v) > 1e-9)
    if len(pos) < 8:
        return 0.0
    return pos[-1] / pos[0]


def spread_honest(vals):
    """(ratio, phrasing) where the phrasing matches what the numbers actually say."""
    signed = [v for v in vals if abs(v) > 1e-9]
    lo, hi = min(vals), max(vals)
    if lo < 0 < hi:
        return spread(vals), "取值 %g..%g（有正有负，像方向性量而非大小）" % (lo, hi)
    return spread(vals), "取值 %g..%g，相差 %g 倍" % (lo, hi, spread(vals))


def build(records_doc, mats, guids, level_nodes, resolve, root_of):
    by_owner = collections.defaultdict(dict)
    for key, rec in records_doc["records"].items():
        by_owner[(key.split("!")[0], rec["owner"])][rec["class"]] = (key, rec)

    emitters = []
    for (cont, owner), trio in by_owner.items():
        em = trio.get("EllipsoidParticleEmitter") or trio.get("MeshParticleEmitter")
        if not em:
            continue
        f = em[1]["fields"]
        rk = trio.get("ParticleRenderer", (None, {}))[1].get("fields", {}).get("material0_32")
        mat = mats.get(resolve(cont, rk[0], rk[1])) if rk else None
        anim = trio.get("ParticleAnimator", (None, {}))[1].get("fields", {})
        emitters.append({
            "key": em[0], "cont": cont, "owner": owner, "f": f,
            "level": level_nodes.get((cont, owner)),
            "root": None if level_nodes.get((cont, owner)) else root_of(cont, owner),
            "mat": mat, "tex": texture_of(mat, guids),
            "alpha": [anim.get("color_%d" % o, [0, 0, 0, 0])[3] for o in (12, 16, 20, 24, 28)]
            if anim else None})

    slots = sorted({k for e in emitters for k in e["f"] if k.startswith("f_")},
                   key=lambda k: int(k.split("_")[1]))
    ranked = []
    for s in slots:
        vals = [e["f"][s] for e in emitters if isinstance(e["f"].get(s), (int, float))]
        if len(vals) < 8:
            continue
        ratio, phrase = spread_honest(vals)
        ranked.append((ratio, s, min(vals), max(vals), phrase, vals))
    ranked.sort(key=lambda r: -r[0])
    return emitters, ranked


def name_of(e):
    return e["level"] or e["root"] or "未命名对象"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--limit", type=int, default=6, help="comparison questions")
    ap.add_argument("--out", default=OUT)
    args = ap.parse_args()

    rp = os.path.join(MIG, "particle-records.json")
    if not os.path.exists(rp):
        sys.exit("particle-records.json missing -- run particle_layouts.py --out first")
    doc = json.load(open(rp, encoding="utf-8"))
    mats = json.load(open(os.path.join(MIG, "materials.json"), encoding="utf-8"))["index"]
    guids = guid_to_texture()

    ext = {}

    def resolve(cont, file_id, path_id):
        if not file_id:
            return "%s!%d" % (cont, path_id)
        if cont not in ext:
            src = {b: p for b, p in u4.sources() if b == cont}
            ext[cont] = u4.externals_of(u4.load(src[cont])) if src else []
        tgt = u4.resolve_target(cont, ext[cont], file_id)
        return "%s!%d" % (tgt, path_id) if tgt else "%s!%d" % (cont, path_id)

    hier = {}

    def root_of(cont, go_pid):
        """The prefab an internal object belongs to: climb m_Father to the root GameObject."""
        if cont not in hier:
            src = {b: p for b, p in u4.sources() if b == cont}
            if not src:
                hier[cont] = None
            else:
                objs = {o.path_id: o for o in u4.load(src[cont]).objects}
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
                        ftp = int(getattr(fa, "m_PathID", 0) or 0) if fa is not None else 0
                        if gop and ftp and int(getattr(fa, "m_FileID", 0) or 0) == 0 and ftp in objs:
                            fd = objs[ftp].read()
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

    level_nodes = {}
    for jp in sorted(glob.glob(os.path.join(MIG, "scenes", "level*.json"))):
        d = json.load(open(jp, encoding="utf-8"))
        for n in d["nodes"]:
            level_nodes[(d["source_level"], n.get("go_id"))] = n["name"]

    emitters, ranked = build(doc, mats, guids, level_nodes, resolve, root_of)
    named = sum(1 for e in emitters if e["tex"])
    print("emitter records: %d, of which a sprite could be resolved for: %d"
          % (len(emitters), named))
    print("slots by spread of values, widest first: %s"
          % [(r[1], round(r[0], 1)) for r in ranked[:10]])
    if named == 0:
        print("STOP: no emitter resolves to a texture, so nothing on this sheet can be recognised by"
              " eye. That is a material/texture gap, not a particle one.")
        return 2

    lines = ["# 旧粒子对照清单（跑一次原版就够）", "",
             "用法：每题给两个特效，各自画哪张贴图也写了。在原版里认出它们，然后只回答一句：",
             "A 相对 B，在哪个维度上不同、方向朝哪。看不出差别就写「看不出」，我按未定名处理。",
             "",
             "为什么这样问，而不是「这颗粒子大概活几秒」：字段顺序对整个 build 固定，所以一个反例就能",
             "全局否掉一个候选角色；而绝对估值既难答又难核。另外离线排除法能收窄的确实有限——只有整数",
             "那几个偏移收到「数量或速率」两选一，其余小正浮点的合理区间互相包含，数值上分不出尺寸、",
             "速度和速率。", ""]

    import particle_layouts as PL
    asked, used_slots, used_objs = 0, set(), set()
    for sp, slot, lo, hi, phrase, vals in ranked:
        if asked >= args.limit:
            break
        if sp < 2 or slot in used_slots or lo == 0:
            continue
        low = [e for e in emitters if e["f"].get(slot) == lo and e["tex"]
               and name_of(e) not in used_objs]
        high = [e for e in emitters if e["f"].get(slot) == hi and e["tex"]
                and name_of(e) not in used_objs]
        if not low or not high:
            continue
        a, b = low[0], high[0]
        used_slots.add(slot)
        used_objs.update({name_of(a), name_of(b)})
        asked += 1
        # state, in the question itself, which candidates the population has already narrowed this
        # slot to -- otherwise the answer has nothing to discriminate between.
        cands = PL.domains_of(vals)
        lines += ["## Q%d  偏移 %s（%s）" % (asked, slot.split("_")[1], phrase), "",
                  "- 离线排除后只剩这些候选：%s" % ("、".join(cands) if cands else "无（不是 float）"),
                  "",
                  "- A：`%s`（`%s`） 贴图 `%s`，该偏移 = %s"
                  % (name_of(a), a["key"], os.path.basename(a["tex"]), a["f"][slot]),
                  "- B：`%s`（`%s`） 贴图 `%s`，该偏移 = %s"
                  % (name_of(b), b["key"], os.path.basename(b["tex"]), b["f"][slot]),
                  "", "A 相对 B：", "", "| 维度 | A>B | A<B | 看不出 |", "|---|---|---|---|"]
        lines += ["| %s（%s） |  |  |  |" % (d, w) for d, w in DIMENSIONS]
        lines += ["", "旁证（可空）：A 的 alpha 轨 %s；B 的 alpha 轨 %s"
                  % (a["alpha"], b["alpha"]), ""]

    easy = [e for e in emitters if e["level"] and e["tex"]][:3]
    lines += ["## 附：顺手可测的绝对值（只在上面答不定时才需要）", ""]
    for e in easy:
        lines += ["- `%s`（`%s`）贴图 `%s`，标志 %s，整数对 %s / %s"
                  % (name_of(e), e["key"], os.path.basename(e["tex"]),
                     [e["f"].get("flag_%d" % i) for i in range(8, 12)],
                     e["f"].get("f_28"), e["f"].get("f_32")),
                  "  - 寿命(秒)：＿＿　初速(单位/秒)：＿＿　大小(相对角色)：＿＿　峰值粒子数：＿＿"]
    lines += ["", "## 读回答案时遵守的规则", "",
              "某个维度上只要有一次方向与候选角色的预测相反，该候选对这个偏移就整体作废；",
              "只有全部答案都不矛盾的候选才允许进转换器；仍然多解的一律留 unknown，不填默认值。", ""]

    with open(args.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write("\n".join(lines))
    print("wrote %s: %d comparison questions, %d absolute rows" % (args.out, asked, len(easy)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
