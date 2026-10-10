"""Export the AudioClips the shipped build carries, and let ffmpeg prove they are audible.

Why this exists: the project had zero audio files, while the scripts ask for BGM and effects by
name (bgm_stage<N>, general_yell<N>a, bgm_boss...) and 45 scene fields reference 23 clips by
fileID. Task #14's "audio" column was the last unexported asset type.

What the data actually is: UnityPy exposes no m_Frequency / m_Channels / m_Length for a Unity 4
AudioClip (they parse to None), but every one of the 82 clips has its whole payload inline in
m_AudioData and the first byte is an MPEG frame sync (0xFF). ffmpeg identifies the stream as
mp3, 44100 Hz. So the export is a byte copy into a .mp3 -- no decoding, no transcoding, no
hand-written container. The clip name is unique across all 82 (checked, not assumed), which is what
lets the files sit at the root of Resources/, which is where Resources.Load("name") looks.

Post-conditions, because a file written is not a file that plays:
  * re-read every written file and require the bytes to equal the source payload;
  * run ffmpeg -decode-to-null on each and require a duration greater than zero;
  * require the exported count to equal the inventoried count, and refuse to overwrite a file
    whose bytes already differ from what this run would write.

Run under the UnityPy venv:
    <venv>\\python.exe tools/migration/export_audioclips.py     RECON_DIR=<path to recon>
"""
import collections
import json
import os
import shutil
import subprocess
import sys

import UnityPy

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unity4_files as u4

_REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT_DIR = os.path.join(_REPO, "unity", "Assets", "Resources")
INDEX = os.path.join(_REPO, "tools", "migration", "audio.json")


def collect():
    """[(container, path_id, name, payload bytes)] for every AudioClip in the build."""
    rows = []
    for base, path in u4.sources():
        env = UnityPy.load(path)
        for o in env.objects:
            if o.type.name != "AudioClip":
                continue
            d = o.read()
            rows.append((base, o.path_id, str(d.m_Name), bytes(d.m_AudioData)))
    return rows


def probe(ffmpeg, p):
    """-> (duration_seconds or None, first line describing the decoded stream)."""
    r = subprocess.run([ffmpeg, "-hide_banner", "-i", p, "-f", "null", "-"],
                       stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=120)
    out = r.stdout.decode("utf-8", "replace")
    dur = None
    for line in out.splitlines():
        if "Duration:" in line:
            tok = line.split("Duration:")[1].strip().split(",")[0]
            try:
                h, m, s = tok.split(":")
                dur = int(h) * 3600 + int(m) * 60 + float(s)
            except ValueError:
                dur = None
        if "Stream #" in line and "Audio:" in line:
            return dur, line.strip()[:120]
    return dur, ""


def main():
    ffmpeg = shutil.which("ffmpeg")
    if not ffmpeg:
        print("REFUSING: no ffmpeg on PATH. Without it there is no independent witness that the")
        print("exported bytes decode, and '82 files written' would be exactly the false green this")
        print("repository has been burned by several times.")
        return 2

    rows = collect()
    print("AudioClip objects inventoried: %d" % len(rows))
    by_name = collections.Counter(r[2] for r in rows)
    dupes = {k: v for k, v in by_name.items() if v > 1}
    if dupes:
        # Names must be unique or the flat Resources layout silently shadows a clip, and the
        # code loads by name. Refuse rather than pick one.
        print("REFUSING: %d clip names occur more than once (%s); a flat Resources/ folder cannot"
              " hold both." % (len(dupes), list(dupes.items())[:5]))
        return 2
    nosync = [r[2] for r in rows if not r[3] or r[3][0] != 0xFF]
    if nosync:
        print("REFUSING: %d clips do not start with an MPEG sync byte, so .mp3 would be a lie:"
              " %s" % (len(nosync), nosync[:8]))
        return 2

    os.makedirs(OUT_DIR, exist_ok=True)
    index, stats = {}, collections.Counter()
    problems = []
    total_dur = 0.0
    for base, pid, name, payload in sorted(rows):
        safe = name.replace("/", "_").replace("\\", "_").replace(":", "_")
        dest = os.path.join(OUT_DIR, safe + ".mp3")
        if os.path.exists(dest):
            with open(dest, "rb") as fh:
                if fh.read() != payload:
                    problems.append("%s: existing file differs from the source payload" % safe)
                    continue
            stats["unchanged"] += 1
        else:
            tmp = dest + ".tmp"
            with open(tmp, "wb") as fh:
                fh.write(payload)
            os.replace(tmp, dest)
            with open(dest, "rb") as fh:
                if fh.read() != payload:
                    problems.append("%s: re-read bytes differ from what was written" % safe)
                    continue
            stats["written"] += 1
        dur, line = probe(ffmpeg, dest)
        if dur is None or dur <= 0.0:
            problems.append("%s: ffmpeg reports no duration (%r)" % (safe, dur))
            continue
        total_dur += dur
        index["%s!%d" % (base, pid)] = {"name": name, "path": "Assets/Resources/%s.mp3" % safe,
                                        "bytes": len(payload), "seconds": round(dur, 3),
                                        "stream": line}
        stats["verified"] += 1

    print("exported and ffmpeg-verified: %d  (newly written %d, already byte-identical %d)"
          % (stats["verified"], stats["written"], stats["unchanged"]))
    print("total decoded audio: %.1f minutes" % (total_dur / 60.0))
    if problems:
        print("problems (%d):" % len(problems))
        for p in problems[:12]:
            print("  " + p)
    covered = len(index)
    if covered != len(rows):
        print("MISMATCH: %d clips inventoried but %d verified -- the index is incomplete, so the"
              " scene pass must not be told audio is available." % (len(rows), covered))
        return 2

    # The scene pass resolves cross-file references through this index; it is keyed exactly like
    # the material / mesh / texture indexes so asset_lookup() needs no special case.
    doc = {"count": covered, "index": index,
           "by_name": {v["name"]: k for k, v in index.items()}}
    tmp = INDEX + ".tmp"
    with open(tmp, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(doc, fh, indent=1, ensure_ascii=False)
    os.replace(tmp, INDEX)
    back = json.load(open(INDEX, encoding="utf-8"))
    if back["count"] != covered or len(back["index"]) != covered:
        print("MISMATCH: audio.json re-read as %d entries, expected %d" % (len(back["index"]),
                                                                          covered))
        return 2
    print("wrote %s (%d entries, re-read ok)" % (INDEX, covered))
    print("clips the scripts ask for by name, sample: %s"
          % sorted(n for n in (v["name"] for v in index.values()) if n.startswith("bgm"))[:8])
    return 0


if __name__ == "__main__":
    sys.exit(main())
