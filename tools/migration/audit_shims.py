import os, re, collections, sys

# 默认扫本仓库的 unity/Assets/Scripts；也可以传路径覆盖：python audit_shims.py <dir>
_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(_REPO, "unity", "Assets", "Scripts")

# Unity 4 legacy Component/GameObject shortcut accessors.
# transform / gameObject are NOT obsolete in 2022 -> deliberately excluded.
MAP = {
    "animation": "Animation",
    "renderer": "Renderer",
    "rigidbody": "Rigidbody",
    "collider": "Collider",
    "audio": "AudioSource",
    "camera": "Camera",
    "light": "Light",
    "particleSystem": "ParticleSystem",
    "particleEmitter": "ParticleSystem",
    "constantForce": "ConstantForce",
    "hingeJoint": "HingeJoint",
    "guiText": "GUIText",
    "guiTexture": "GUITexture",
    "networkView": "NetworkView",
}

files = []
for dp, dn, fn in os.walk(ROOT):
    for f in fn:
        if f.endswith(".cs"):
            files.append(os.path.join(dp, f))
files.sort()
print(f"scanned files: {len(files)}")

usage = collections.Counter()
usage_files = collections.defaultdict(set)
in_string_or_comment = collections.Counter()
own_member_decls = collections.defaultdict(list)
own_local_decls = collections.defaultdict(list)

# declaration patterns that would make `.NAME` refer to a user member, not the Unity shim
decl_re = {
    n: re.compile(r"\b(?:public|private|protected|internal)\s+(?:static\s+)?(?:readonly\s+)?[\w.<>\[\]]+\s+" + n + r"\s*[;={=]")
    for n in MAP
}
local_re = {
    n: re.compile(r"(?:^|[;{}\s])([\w.<>\[\]]+)\s+" + n + r"\s*=")
    for n in MAP
}

for path in files:
    rel = os.path.relpath(path, ROOT).replace("\\", "/")
    src = open(path, encoding="utf-8", errors="replace").read()
    lines = src.split("\n")

    for n in MAP:
        m = decl_re[n].search(src)
        if m:
            own_member_decls[n].append((rel, m.group(0).strip()[:80]))
        for i, ln in enumerate(lines, 1):
            mm = local_re[n].search(ln)
            if mm and mm.group(1) not in ("return", "new", "else", "case"):
                own_local_decls[n].append((rel, i, ln.strip()[:90]))

    for i, ln in enumerate(lines, 1):
        code = ln
        # strip line comments and string literals so we can flag matches that live inside them
        stripped = re.sub(r'"(?:\\.|[^"\\])*"', '""', code)
        stripped = re.sub(r"//.*$", "", stripped)
        for n in MAP:
            for m in re.finditer(r"\." + n + r"\b", code):
                usage[n] += 1
                usage_files[n].add(rel)
                if not re.search(r"\." + n + r"\b", stripped):
                    in_string_or_comment[n] += 1

print("\n=== usage of `.NAME` across corpus ===")
for n, c in usage.most_common():
    if c:
        flag = f"   <-- {in_string_or_comment[n]} inside string/comment!" if in_string_or_comment[n] else ""
        print(f"  {c:5d}  .{n}  ({len(usage_files[n])} files){flag}")

print("\n=== scripts declaring their OWN member with a colliding name ===")
any_decl = False
for n, hits in own_member_decls.items():
    if hits:
        any_decl = True
        print(f"  .{n}:")
        for rel, txt in hits[:12]:
            print(f"      {rel}: {txt}")
if not any_decl:
    print("  (none)")

print("\n=== local variables named like a shim (would break `.NAME` on those) ===")
seen = set()
for n, hits in own_local_decls.items():
    for rel, i, txt in hits:
        key = (n, txt)
        if key in seen:
            continue
        seen.add(key)
        print(f"  .{n}  {rel}:{i}: {txt}")
if not seen:
    print("  (none)")
