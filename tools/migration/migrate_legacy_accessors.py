#!/usr/bin/env python3
"""Unity 4 -> 2022 legacy accessor codemod.

Rewrites the Unity 4 `Component`/`GameObject` shortcut accessors (which are
[Obsolete(error:true)] in 2022.3 and return a degraded `Component`) into
GetComponent<T>() calls, and maps the removed legacy particle API onto
ParticleSystem.

Safety preconditions (verified by tools/migration/audit_shims.py before this ran):
  * no script in the corpus declares its own member named like a shim,
  * no `.NAME` occurrence lives inside a string literal or comment,
  * the three shadowing locals (`Cha_Skill.cs:415`, `MakeUI.cs:44`,
    `TexturePacker.cs:29`) are only ever assigned FROM the shim, never
    accessed AS `.NAME`, so the leading-dot requirement leaves them alone.

Deliberately NOT rewritten here (needs a human decision):
  * `.guiText`        -- GUIText is removed outright in 2022, no GetComponent target
  * `.minEnergy/.maxEnergy` -- maps onto main.startLifetime, not a rename
  * `ParticleEmitter` used as a type name
  * `GameObject.AddComponent(string)` -- the string must be resolved to a type
"""
import argparse
import collections
import os
import re
import sys

# accessor -> replacement generic type
ACCESSORS = {
    "animation": "Animation",
    "renderer": "Renderer",
    "rigidbody": "Rigidbody",
    "collider": "Collider",
    "audio": "AudioSource",
    "camera": "Camera",
    "light": "Light",
    "particleSystem": "ParticleSystem",
    "particleEmitter": "ParticleSystem",  # legacy type removed; Shuriken replaces it
}
# handled separately, never blindly rewritten
SKIP_ACCESSORS = {"guiText", "guiTexture", "constantForce", "hingeJoint", "networkView"}

# `RECV.emit = VALUE;` occupying a whole statement line.
EMIT_WRITE = re.compile(r"^(\s*)(?P<recv>.+?)\.emit\s*=\s*(?P<val>.+?);\s*$")


def rewrite_line(line, stats, fname, lineno):
    """Apply the mechanical suffix rewrites to a single line."""
    # Never rewrite inside a comment: prose such as "ParticleEmitter.Emit()" would
    # otherwise be silently mutated. Audit confirmed no accessor name appears in a
    # string literal or trailing comment anywhere in the corpus.
    if line.lstrip().startswith("//"):
        return line

    out = line

    # 1) accessor suffix -> GetComponent<T>()
    for name, ty in ACCESSORS.items():
        pat = re.compile(r"\." + name + r"\b(?!\s*=>)")
        n = len(pat.findall(out))
        if n:
            out = pat.sub(".GetComponent<%s>()" % ty, out)
            stats["accessor." + name] += n

    # 2) legacy ParticleEmitter members that are pure renames
    for old, new, key in (
        (r"\.ClearParticles\(\)", ".Clear()", "particle.ClearParticles"),
        (r"\.Emit\(\)", ".Emit(1)", "particle.Emit()"),
    ):
        pat = re.compile(old)
        n = len(pat.findall(out))
        if n:
            out = pat.sub(new, out)
            stats[key] += n

    # 2b) plain renames that Unity itself tags (UnityUpgradable), i.e. a guaranteed 1:1
    #     substitution. Done here rather than by accepting the Editor's "Script Updating
    #     Consent" dialog: that dialog's default button also arms the updater for every file
    #     imported afterwards, which would rewrite this working copy behind our back.
    for old, new, key in (
        (r"\.GetChildCount\(\)", ".childCount", "rename.GetChildCount"),
    ):
        pat = re.compile(old)
        n = len(pat.findall(out))
        if n:
            out = pat.sub(new, out)
            stats[key] += n

    # 3) reads of `.emit` become `.emission.enabled`
    #    (writes were already lifted into a block by rewrite_statements)
    pat = re.compile(r"\.emit\b(?!\s*=[^=])")
    n = len(pat.findall(out))
    if n:
        out = pat.sub(".emission.enabled", out)
        stats["particle.emit.read"] += n

    if out != line:
        stats["lines_changed"] += 1
    return out


def rewrite_statements(lines, stats):
    """Lift `RECV.emit = X;` into a scoped block.

    `ParticleSystem.emission` returns a struct BY VALUE, so
    `ps.emission.enabled = true` is CS1612. It has to be bound to a variable
    first. A bare block keeps the name reusable and is valid anywhere the
    original statement was.
    """
    out = []
    for idx, line in enumerate(lines):
        m = EMIT_WRITE.match(line)
        if m and re.search(r"\.emit\s*=[^=]", line):
            indent = m.group(1)
            recv = m.group("recv")
            val = m.group("val")
            # `emit` is only ever a bool on the legacy emitter
            out.append(indent + "{")
            out.append(indent + "\tvar emission = " + recv + ".emission;")
            out.append(indent + "\temission.enabled = " + val + ";")
            out.append(indent + "}")
            stats["particle.emit.write"] += 1
            stats["lines_changed"] += 1
        else:
            out.append(line)
    return out


def process(path, stats, dry):
    raw = open(path, "rb").read()
    bom = raw[:3] == b"\xef\xbb\xbf"
    text = raw.decode("utf-8-sig")
    nl = "\r\n" if "\r\n" in text else "\n"
    lines = text.split(nl)

    before = dict(stats)
    lines = rewrite_statements(lines, stats)
    lines = [rewrite_line(l, stats, path, i + 1) for i, l in enumerate(lines)]

    changed = stats != before
    if changed and not dry:
        body = nl.join(lines)
        data = body.encode("utf-8")
        if bom:
            data = b"\xef\xbb\xbf" + data
        open(path, "wb").write(data)
    return changed


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("root", help="unity/Assets/Scripts")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    files = []
    for dp, dn, fn in os.walk(args.root):
        for f in fn:
            if f.endswith(".cs"):
                files.append(os.path.join(dp, f))
    files.sort()

    stats = collections.Counter()
    touched = []
    for p in files:
        # Count lines changed, not sum(stats.values()): stats holds the per-rule hits AND
        # lines_changed, so summing them double-counts every rewritten line.
        lines_before = stats["lines_changed"]
        if process(p, stats, args.dry_run):
            touched.append((os.path.relpath(p, args.root).replace("\\", "/"),
                            stats["lines_changed"] - lines_before))

    print(("DRY RUN -- nothing written\n" if args.dry_run else "") + f"files scanned: {len(files)}")
    print(f"files touched: {len(touched)}")
    print("\n=== rewrite counts ===")
    for k, v in sorted(stats.items(), key=lambda kv: -kv[1]):
        if k != "lines_changed":
            print(f"  {v:5d}  {k}")
    print(f"  {stats['lines_changed']:5d}  lines_changed (total)")

    print("\n=== per-file lines changed (top 30) ===")
    for rel, n in sorted(touched, key=lambda t: -t[1])[:30]:
        print(f"  {n:4d}  {rel}")


if __name__ == "__main__":
    sys.exit(main())
