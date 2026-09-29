"""Decode Unity 4 MonoBehaviour field values out of the raw serialized bytes.

Library + report mode. extract_scene.py imports decode_instance() and writes the result into the
level JSON; running this file directly prints a coverage report over all 23 levels and changes
nothing.

WHY THE STRICT BYTE-EXACTNESS RULE

Unity 4 player builds carry no TypeTree, so a field's offset can only be recovered by walking the
class's serialized layout from offset 24 (after the fixed MonoBehaviour header). That is only safe
when the layout is fully known. So this module refuses to guess:

  * a class is decoded only when every serializable field has a known fixed width
    (string, arrays, and unknown custom types are skipped, not approximated);
  * the walk must consume the field region EXACTLY. Any leftover or overrun skips the whole class
    and is reported, because a half-correct layout would silently write wrong references into the
    scene -- worse than no data at all.

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
NON_SERIALIZED = {"GUIStyle", "GUIContent", "RectOffset", "GradientColorKey", "RangeInt"}
STRUCT_TYPES = {"Bounds": 24}

# Per-type struct format. Getting this wrong is not subtle -- unpacking a 4-byte int where the
# field is 2 bytes overruns the buffer and the whole class then fails the exactness rule.
FMT = {"int": "<i", "uint": "<I", "short": "<h", "ushort": "<H", "byte": "<B", "sbyte": "<b",
       "char": "<H", "long": "<q", "ulong": "<Q", "float": "<f", "double": "<d", "LayerMask": "<i"}

CLASS_RE = re.compile(r"^\s*(?:public|internal)?\s*(?:abstract|sealed|partial|static)?\s*"
                      r"class\s+(\w+)(?:\s*:\s*([\w<>,\. ]+))?\s*\{", re.M)
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


def field_layout(fields):
    """-> (list of (kind, name, size), reason_to_skip). kind in {value, ref}. Exactly one is set."""
    layout = []
    for typ, name in fields:
        if typ in NON_SERIALIZED:
            continue
        if typ.endswith("[]") or typ.startswith("List<") or typ in ("string",):
            return None, "variable-length %r on %s" % (typ, name)
        if typ in WIDTHS:
            w, a = WIDTHS[typ]
            layout.append(("value", name, typ, w, a))
        elif typ in STRUCT_TYPES:
            layout.append(("value", name, typ, STRUCT_TYPES[typ], 4))
        elif typ in REF_TYPES:
            layout.append(("ref", name, typ, 8, 4))
        elif typ.startswith("UnityEngine."):
            return None, "unmapped engine type %r on %s" % (typ, name)
        else:
            return None, "unknown type %r on %s" % (typ, name)
    return layout, None


def decode_instance(raw, fields):
    """-> ({name: value}, None) or (None, reason). raw is the whole MonoBehaviour record."""
    layout, why = field_layout(fields)
    if layout is None:
        return None, why
    off = HEADER_LEN
    end = len(raw)
    out = {}
    for kind, name, typ, size, align in layout:
        off = _align(off, align)
        if off + size > end:
            return None, "ran out of bytes at %s (need %d at %d, len %d)" % (name, size, off, end)
        if kind == "ref":
            fid, pid = struct.unpack_from("<ii", raw, off)
            out[name] = {"ref": pid, "file": fid, "type": typ}
        elif typ == "bool":
            out[name] = {"bool": bool(raw[off])}
        elif typ in FMT:
            v = struct.unpack_from(FMT[typ], raw, off)[0]
            out[name] = {"float": v} if typ in ("float", "double") else {"int": v}
        else:
            out[name] = {"floats": list(struct.unpack_from("<%df" % (size // 4), raw, off)),
                         "vec": typ}
        off += size
    off = _align(off, 4)
    if off != end:
        return None, "layout consumed %d of %d bytes (leftover %d) -- refusing a partial guess" % (
            off, end, end - off)
    return out, None


def load_bindings():
    with open(BINDINGS, "r", encoding="utf-8") as fh:
        return json.load(fh)


_CLASSES = None


def decode_for_class(cls, raw):
    """-> ([(field_name, value)], None) or (None, reason). Caches the parsed source classes."""
    global _CLASSES
    if _CLASSES is None:
        _CLASSES = parse_classes()
    fields = serializable_fields(cls, _CLASSES)
    if fields is None:
        return None, "class %s not found in %s" % (cls, SCRIPTS)
    vals, why = decode_instance(raw, fields)
    if vals is None:
        return None, why
    # list, not dict: the caller relies on field order matching the serialised layout.
    return list(vals.items()), None


def report():
    import UnityPy
    from unity4_files import sources
    b = load_bindings()
    externals, monoscripts = b["level_externals"], b["monoscripts"]
    classes = parse_classes()
    decoded = skipped = noref = 0
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
            vals, why = decode_instance(raw, fields)
            if vals is None:
                skipped += 1
                reasons[why.split("--")[0].strip()[:48]] = reasons.get(why.split("--")[0].strip()[:48], 0) + 1
                continue
            decoded += 1
            noref += sum(1 for v in vals.values() if "ref" in v and v["ref"] != 0)
            per_class[cls] = per_class.get(cls, 0) + 1
    print("MonoBehaviour instances decoded exactly: %d   refused: %d" % (decoded, skipped))
    print("non-null object references recovered:  %d" % noref)
    print("distinct classes fully decoded: %d" % len(per_class))
    print("top classes:", sorted(per_class.items(), key=lambda x: -x[1])[:10])
    print("\nrefusal reasons:")
    for k, v in sorted(reasons.items(), key=lambda x: -x[1]):
        print("   %-52s %d" % (k, v))


if __name__ == "__main__":
    report()
