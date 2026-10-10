"""Decode Unity 4 MonoBehaviour field values out of the raw serialized bytes.

Library + report mode. extract_scene.py imports decode_instance() and writes the result into the
level JSON; running this file directly prints a coverage report over all 23 levels and changes
nothing.

WHY THE STRICT BYTE-EXACTNESS RULE

Unity 4 player builds carry no TypeTree, so a field's offset can only be recovered by walking the
class's serialized layout from offset 24 (after the fixed MonoBehaviour header). That is only safe
when the layout is fully known. So this module refuses to guess:

  * a class is decoded only when every serializable field has a known fixed width
    (strings, arrays, and unknown custom types are skipped, not approximated);
  * the walk must consume the field region EXACTLY. A leftover refuses the whole class and is
    reported, because a half-correct layout would silently write wrong references into the scene --
    worse than no data at all. Running out BETWEEN two fields is the one exception, and it is a
    fact about the data rather than about the layout: Unity 4 reads positionally with no TypeTree,
    so a record that ends exactly at a field boundary means the trailing fields were never
    serialised into that scene and take their C# initializers at runtime. The prefix is decoded,
    and the absent tail is reported by name (Spawn, General_Stat, DB_Monster all show this).

The reference case that established the layout: Cam_intro in level0, raw_len 64, five public
Transform fields, 64 - 24 = 40 = 5 x 8 bytes, and four of the five resolve to a GameObject whose
name equals the field name.

Unity 4 PPtr is 8 bytes: { int32 FileID; int32 PathID }, FileID being a 1-based index into the
owning file's externals table (0 = same file).
"""
import json
import os
import re
import struct
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
SCRIPTS = os.path.join(REPO, "unity", "Assets", "Scripts")
BINDINGS = os.path.join(REPO, "tools", "migration", "monoscript-bindings.json")
HEADER_LEN = 24

# Serializable fixed-width types -> (bytes, alignment). bool is 1 byte but the next field is
# still 4-aligned, which is what the Cam_intro / level0 walk confirmed.
WIDTHS = {
    "int": (4, 4), "uint": (4, 4), "float": (4, 4), "long": (8, 8), "ulong": (8, 8),
    "double": (8, 8), "short": (2, 2), "ushort": (2, 2), "byte": (1, 1), "sbyte": (1, 1),
    "char": (2, 2), "bool": (1, 4),
    "Vector2": (8, 4), "Vector3": (12, 4), "Vector4": (16, 4), "Quaternion": (16, 4),
    "Color": (16, 4), "Color32": (4, 4), "Rect": (16, 4), "LayerMask": (4, 4),
}
# Anything deriving from UnityEngine.Object is serialised as a PPtr.
REF_TYPES = {
    "GameObject", "Transform", "Rigidbody", "Collider", "BoxCollider", "SphereCollider",
    "CapsuleCollider", "MeshCollider", "CharacterController", "Renderer", "MeshRenderer",
    "SkinnedMeshRenderer", "ParticleSystem", "ParticleSystemRenderer", "Camera", "Light",
    "AudioSource", "AudioListener", "Animation", "Mesh", "Material", "AudioClip",
    "AnimationClip", "Texture", "Texture2D", "RenderTexture", "Font", "TextAsset",
    "Sprite", "Component", "Behaviour", "MonoBehaviour", "ScriptableObject", "Object",
    "ConfigurableJoint", "HingeJoint", "SpringJoint", "ConstantForce", "Joint",
    # GUISkin derives from ScriptableObject, so it is a real serialised PPtr.
    "GUISkin",
}
# Types that are NOT UnityEngine.Object: Unity never serialises them, so they occupy zero bytes
# and must be dropped from the layout rather than counted as unknown. (Got this wrong first: two
# GUIStyle fields made their whole class refuse to decode.)
#
# GUIStyle is NOT in that list any more: it is serialised by Unity 4. The 312-byte size is not a
# guess -- sweeping 300..324 over all 394 scene MonoBehaviour records, 312 is the only value that
# makes any class consume its buffer exactly (266 -> 285 decoded, refusals 56 -> 37); every other
# candidate leaves the baseline untouched. Its internals are still unknown to us, so it is carried
# as an opaque block that is skipped and counted, never written into the scene.
NON_SERIALIZED = {"GUIContent", "RectOffset", "GradientColorKey", "RangeInt"}
OPAQUE_TYPES = {"GUIStyle": 312}
STRUCT_TYPES = {"Bounds": 24}

# Per-type struct format. Getting this wrong is not subtle -- unpacking a 4-byte int where the
# field is 2 bytes overruns the buffer and the whole class then fails the exactness rule.
FMT = {"int": "<i", "uint": "<I", "short": "<h", "ushort": "<H", "byte": "<B", "sbyte": "<b",
       "char": "<H", "long": "<q", "ulong": "<Q", "float": "<f", "double": "<d", "LayerMask": "<i"}

CLASS_RE = re.compile(r"^\s*(?:public|internal)?\s*(?:abstract|sealed|partial|static)?\s*"
                      r"(?:class|struct)\s+(\w+)(?:\s*:\s*([\w<>,\. ]+))?\s*\{", re.M)
FIELD_RE = re.compile(r"^\t(?:public)\s+(?!static)(?!const)([\w\[\]<>,\s]+?)\s+(\w+)\s*(?:=[^;]*)?;\s*$", re.M)


def _align(off, a):
    return off + (-off % a) if a > 1 else off


def parse_classes(root=SCRIPTS):
    """{class_name: (base_name, [(type, name), ...])} from the regenerated sources."""
    out = {}
    for dirpath, _dirs, files in os.walk(root):
        for fn in files:
            if not fn.endswith(".cs"):
                continue
            with open(os.path.join(dirpath, fn), "r", encoding="utf-8", errors="replace") as fh:
                text = fh.read()
            for m in CLASS_RE.finditer(text):
                name, base = m.group(1), (m.group(2) or "").split(",")[0].strip() or None
                start = m.end()
                # class body = up to the next top-level 'public class' or file end
                nxt = text.find("\npublic class", start)
                body = text[start:nxt if nxt > 0 else len(text)]
                fields = []
                for fm in FIELD_RE.finditer(body):
                    typ = fm.group(1).strip()
                    if typ in ("get", "set", "return", "else"):
                        continue
                    fields.append((typ, fm.group(2)))
                out.setdefault(name, (base, fields))
    return out


def serializable_fields(cls, classes, depth=0):
    """Base-class fields first, matching Unity's serialisation order. None if unresolvable."""
    if cls not in classes:
        return None
    base, own = classes[cls]
    if base in ("MonoBehaviour", "ScriptableObject", "object", "Object", None, "Component", "Behaviour"):
        inherited = []
    elif depth > 8:
        return None
    else:
        inherited = serializable_fields(base, classes, depth + 1)
        if inherited is None:
            return None
    return inherited + own


# The engine bases these scripts declare. A class reaching one of them is an Object reference for
# serialisation purposes; a class with no base (or `: object`) is a plain [Serializable] holder and
# stays inline.
OBJECT_BASES = {"MonoBehaviour", "ScriptableObject", "Component", "Behaviour", "Object",
                "UnityEngine.Object", "GameObject"}


def derives_from_object(typ, classes, depth=0):
    """True when a field of type `typ` serialises as an 8-byte PPtr rather than inline bytes."""
    if typ in REF_TYPES:
        return True
    if typ not in classes or depth > 8:
        return False
    base = classes[typ][0]
    if not base:
        return False
    return base in OBJECT_BASES or derives_from_object(base, classes, depth + 1)


def spec_for(typ, classes, depth=0):
    """Describe how one serialised value of type `typ` is laid out.

    -> (spec, None) or (None, reason). A spec is a dict:
         {'kind':'value','type':T,'size':n,'align':a}   fixed scalar / Vector-like
         {'kind':'ref','type':T}                        8-byte PPtr
         {'kind':'string'}                              int32 length + bytes + pad to 4
         {'kind':'array','elem':spec}                   int32 count + elements
         {'kind':'struct','type':T,'size':n,'align':4,'fields':[(name,spec)]}
    Only fixed-size structs are accepted, and bool[] is refused outright: whether Unity packs a
    bool array 1 byte per element or 4 is not something to guess, because the wrong stride can
    still consume the buffer exactly and then write plausible-looking nonsense.
    """
    if depth > 6:
        return None, "type nesting too deep at %r" % typ
    m = re.match(r"^(.+)\[\]$", typ) or re.match(r"^List<(.+)>$", typ) or \
        re.match(r"^IList<(.+)>$", typ)
    if m:
        inner, why = spec_for(m.group(1).strip(), classes, depth + 1)
        if inner is None:
            return None, why
        if m.group(1).strip() in ("bool", "sbyte"):
            return None, "bool[] element stride is not settled (1 vs 4 bytes)"
        return {"kind": "array", "elem": inner}, None
    if typ == "string":
        return {"kind": "string"}, None
    if typ in WIDTHS:
        w, a = WIDTHS[typ]
        return {"kind": "value", "type": typ, "size": w, "align": a}, None
    if typ in STRUCT_TYPES:
        return {"kind": "value", "type": typ, "size": STRUCT_TYPES[typ], "align": 4}, None
    if typ in OPAQUE_TYPES:
        return {"kind": "opaque", "type": typ, "size": OPAQUE_TYPES[typ], "align": 4}, None
    if typ in REF_TYPES:
        return {"kind": "ref", "type": typ, "size": 8, "align": 4}, None
    if typ in classes and derives_from_object(typ, classes, depth):
        # A field whose declared type is itself a UnityEngine.Object derivative is serialised as a
        # PPtr, NOT expanded inline. This was wrong first: Cha_Control has a General_Stat field, the
        # walk inlined General_Stat's own 20 bytes, the layout then over-ran the record and the whole
        # class refused -- which is how 234 null references came to look like unrecoverable data.
        return {"kind": "ref", "type": typ, "size": 8, "align": 4}, None
    if typ in classes:
        fields = serializable_fields(typ, classes)
        if fields is None:
            return None, "unknown base of %r" % typ
        if not fields:
            return None, "struct %r has no parsed fields" % typ
        subs = []
        size = 0
        for t, name in fields:
            if t in NON_SERIALIZED:
                continue
            s, why = spec_for(t, classes, depth + 1)
            if s is None:
                return None, "%s (inside %s.%s)" % (why, typ, name)
            if s["kind"] in ("string", "array"):
                return None, "variable-length %r inside %s.%s" % (t, typ, name)
            size = _align(size, s["align"]) + s["size"]
            subs.append((name, s))
        return {"kind": "struct", "type": typ, "size": _align(size, 4), "align": 4,
                "fields": subs}, None
    if typ.startswith("UnityEngine."):
        return None, "unmapped engine type %r" % typ
    return None, "unknown type %r" % typ


def _read(spec, raw, off, end):
    """-> (value, new_offset, None) or (None, None, reason)."""
    k = spec["kind"]
    off = _align(off, spec.get("align", 4))
    if k == "value":
        if off + spec["size"] > end:
            return None, None, "value overrun"
        typ = spec["type"]
        if typ == "bool":
            return {"bool": bool(raw[off])}, off + 1, None
        if typ in FMT:
            v = struct.unpack_from(FMT[typ], raw, off)[0]
            return ({"float": float(v)} if typ in ("float", "double") else {"int": v}), \
                off + spec["size"], None
        vals = list(struct.unpack_from("<%df" % (spec["size"] // 4), raw, off))
        return {"floats": vals, "vec": typ}, off + spec["size"], None
    if k == "opaque":
        # Bytes are consumed so the rest of the class stays aligned; the content is deliberately
        # not interpreted, and the caller is told it was skipped.
        if off + spec["size"] > end:
            return None, None, "opaque overrun"
        return {"opaque": spec["type"], "size": spec["size"]}, off + spec["size"], None
    if k == "ref":
        if off + 8 > end:
            return None, None, "ref overrun"
        fid, pid = struct.unpack_from("<ii", raw, off)
        return {"ref": pid, "file": fid, "type": spec["type"]}, off + 8, None
    if k == "string":
        if off + 4 > end:
            return None, None, "string length overrun"
        (n,) = struct.unpack_from("<i", raw, off)
        off += 4
        if n < 0 or off + n > end:
            return None, None, "string length %d out of range" % n
        blob = raw[off:off + n]
        try:
            s = blob.decode("utf-8")
        except UnicodeDecodeError:
            return None, None, "string bytes are not utf-8"
        if any(ord(c) < 9 or (13 < ord(c) < 32) or ord(c) == 127 for c in s):
            return None, None, "string contains control characters -- wrong offset?"
        return {"str": s}, _align(off + n, 4), None
    if k == "array":
        if off + 4 > end:
            return None, None, "array count overrun"
        (n,) = struct.unpack_from("<i", raw, off)
        off += 4
        if n < 0 or n > 8192:
            return None, None, "array count %d implausible" % n
        items = []
        for _i in range(n):
            v, off, why = _read(spec["elem"], raw, off, end)
            if why:
                return None, None, "array element %d: %s" % (_i, why)
            items.append(v)
        return {"arr": items, "elem": spec["elem"].get("type") or spec["elem"]["kind"]}, off, None
    if k == "struct":
        if off + spec["size"] > end:
            return None, None, "struct overrun"
        vals = {}
        sub = off
        for name, s in spec["fields"]:
            v, sub, why = _read(s, raw, sub, end)
            if why:
                return None, None, "%s: %s" % (name, why)
            vals[name] = v
        return {"obj": vals, "struct": spec["type"]}, _align(off + spec["size"], 4), None
    return None, None, "unhandled spec %r" % k


def decode_instance(raw, fields, classes=None):
    """-> ({name: value}, None, [truncated field names]) or (None, reason, []).

    The walk must consume the field region EXACTLY: a leftover refuses the whole class rather than
    emitting a partial guess. A record that runs out BETWEEN fields is different and legitimate --
    Unity 4 has no TypeTree, so it reads positionally and simply stops at the end of the buffer, and
    the fields past that point take their C# initializers at runtime. Measured on this build:
    Spawn's record ends at offset 508, exactly where its last declared int (cur_general) would
    start, with the preceding 24 fields fitting byte-for-byte; General_Stat carries 4 bytes, exactly
    its first two shorts, and refuses to carry the other seven at all. Those are trailing fields the
    shipped scene never serialised, not a layout we got wrong, so the prefix is decoded and the
    missing tail is reported by name instead of throwing away the data that IS there.

    A field that only PARTIALLY fits still refuses the class: that is drift, not truncation.
    """
    off = HEADER_LEN
    end = len(raw)
    out = {}
    truncated = []
    for i, (typ, name) in enumerate(fields):
        if typ in NON_SERIALIZED:
            continue
        spec, why = spec_for(typ, classes or {})
        if spec is None:
            return None, "%s on %s" % (why, name), []
        if _align(off, spec.get("align", 4)) >= end:
            # Nothing left in the buffer: everything from here on was never serialised.
            truncated = [n for t, n in fields[i:]
                         if t not in NON_SERIALIZED and n not in out]
            break
        v, off, why = _read(spec, raw, off, end)
        if why:
            return None, "%s: %s" % (name, why), []
        out[name] = v
    off = _align(off, 4)
    if off != end:
        return None, "layout consumed %d of %d bytes (leftover %d) -- refusing a partial guess" % (
            off, end, end - off), []
    return out, None, truncated


def load_bindings():
    with open(BINDINGS, "r", encoding="utf-8") as fh:
        return json.load(fh)


_CLASSES = None


def decode_for_class(cls, raw):
    """-> ([(field_name, value)], None, [truncated]) or (None, reason, []). Caches the class table."""
    global _CLASSES
    if _CLASSES is None:
        _CLASSES = parse_classes()
    fields = serializable_fields(cls, _CLASSES)
    if fields is None:
        return None, "class %s not found in %s" % (cls, SCRIPTS), []
    vals, why, truncated = decode_instance(raw, fields, _CLASSES)
    if vals is None:
        return None, why, []
    # list, not dict: the caller relies on field order matching the serialised layout.
    return list(vals.items()), None, truncated


def classify_refusal(why):
    """Bucket a refusal so the tally says what kind of problem it is.

    "37 refused" read as 37 defects and sent me hunting a bug that was not there. The dominant
    bucket is a record SHORTER than the declared layout, which for these classes is faithful to
    the original: DB_Monster.enemy[] (16 elements, field region 0 bytes) is assigned element by
    element inside Awake(), and General_Stat.g_maxhp is computed at runtime from rndkey -- the
    values come from the shipped C# we already have, so there is nothing to recover from bytes.
    """
    w = why or ""
    if "overrun" in w or "out of range" in w:
        return "record shorter than layout (data assigned in code, or lives in the prefab)"
    if "leftover" in w:
        return "record longer than layout (a field this module does not know)"
    if "[,]" in w:
        return "two-dimensional array (unsupported by design)"
    if "bool[]" in w:
        return "bool[] stride unresolved (refused on purpose)"
    if "variable-length" in w:
        return "variable-length member inside a struct (refused on purpose)"
    return "other"


def report():
    import UnityPy
    from unity4_files import sources
    b = load_bindings()
    externals, monoscripts = b["level_externals"], b["monoscripts"]
    classes = parse_classes()
    decoded = skipped = noref = 0
    trunc = {}
    trunc_total = 0
    reasons = {}
    per_class = {}
    for name, path in sorted(sources()):
        if not name.startswith("level"):
            continue
        ext = externals.get(name, [])
        env = UnityPy.load(path)
        for o in env.objects:
            if o.type.name != "MonoBehaviour":
                continue
            raw = o.get_raw_data()
            scf, scp = struct.unpack_from("<ii", raw, 12)
            src = name if scf == 0 else (ext[scf - 1] if 0 < scf <= len(ext) else None)
            cls = monoscripts.get("%s!%d" % (src, scp)) if src else None
            if not cls:
                skipped += 1
                reasons["script unresolved"] = reasons.get("script unresolved", 0) + 1
                continue
            fields = serializable_fields(cls, classes)
            if fields is None:
                skipped += 1
                reasons["class not found in sources"] = reasons.get("class not found in sources", 0) + 1
                continue
            vals, why, truncated = decode_instance(raw, fields, classes)
            if vals is None:
                skipped += 1
                reasons[why.split("--")[0].strip()[:48]] = reasons.get(why.split("--")[0].strip()[:48], 0) + 1
                continue
            decoded += 1
            noref += sum(1 for v in vals.values() if "ref" in v and v["ref"] != 0)
            per_class[cls] = per_class.get(cls, 0) + 1
            if truncated:
                trunc[cls] = max(trunc.get(cls, 0), len(truncated))
                trunc_total += len(truncated)
    print("MonoBehaviour instances decoded exactly: %d   refused: %d" % (decoded, skipped))
    print("non-null object references recovered:  %d" % noref)
    print("distinct classes fully decoded: %d" % len(per_class))
    print("records ending at a field boundary: %d field value(s) absent across %d class(es), "
          "worst: %s" % (trunc_total, len(trunc),
                        sorted(trunc.items(), key=lambda x: -x[1])[:5]))
    print("top classes:", sorted(per_class.items(), key=lambda x: -x[1])[:10])
    print("\nrefusal reasons:")
    for k, v in sorted(reasons.items(), key=lambda x: -x[1]):
        print("   %-52s %d" % (k, v))
    buckets = {}
    for k, v in reasons.items():
        buckets[classify_refusal(k)] = buckets.get(classify_refusal(k), 0) + v
    print("\nwhat kind of problem each refusal actually is:")
    for k, v in sorted(buckets.items(), key=lambda x: -x[1]):
        print("   %-72s %d" % (k, v))


if __name__ == "__main__":
    report()
