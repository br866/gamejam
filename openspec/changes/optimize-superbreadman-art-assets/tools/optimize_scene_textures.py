import bpy
import hashlib
import json
import os
import shutil
import sys


assets_root = sys.argv[sys.argv.index("--") + 1]
backup_root = sys.argv[sys.argv.index("--") + 2]
manifest_path = sys.argv[sys.argv.index("--") + 3]
image_extensions = {".png", ".jpg", ".jpeg", ".tga", ".psd"}
records = []


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


def classify(name):
    name = name.lower()
    if "normal" in name or "nor'ma'l" in name:
        return "normal", 1024
    if "metallic" in name or "roughness" in name or "mask" in name:
        return "data", 512
    if "emission" in name or "_emit" in name:
        return "emission", 512
    return "color", 1024


files = []
for directory, _, names in os.walk(assets_root):
    for name in names:
        if os.path.splitext(name)[1].lower() in image_extensions:
            files.append(os.path.join(directory, name))

for source_path in sorted(files):
    relative_path = os.path.relpath(source_path, assets_root).replace("\\", "/")
    backup_path = os.path.join(backup_root, relative_path)
    os.makedirs(os.path.dirname(backup_path), exist_ok=True)
    if not os.path.exists(backup_path):
        shutil.copy2(source_path, backup_path)

    image = bpy.data.images.load(source_path, check_existing=False)
    width, height = image.size
    image_type, maximum_size = classify(os.path.basename(source_path))
    source_bytes = os.path.getsize(source_path)
    target_scale = min(1.0, maximum_size / max(width, height))
    target_width = max(1, round(width * target_scale))
    target_height = max(1, round(height * target_scale))
    status = "unchanged-within-budget"

    if target_scale < 1.0:
        image.scale(target_width, target_height)
        image.filepath_raw = source_path
        image.file_format = "PNG"
        image.save()
        status = "resized"

    bpy.data.images.remove(image)
    records.append(
        {
            "path": relative_path,
            "status": status,
            "type": image_type,
            "backup_sha256": sha256(backup_path),
            "source_bytes": source_bytes,
            "optimized_bytes": os.path.getsize(source_path),
            "source_dimensions": [width, height],
            "optimized_dimensions": [target_width, target_height],
            "maximum_size": maximum_size,
        }
    )

summary = {
    "scope": "Assets/SuperBreadMan/Scene Model world-space textures",
    "policy": "Performance-priority; texture detail loss is permitted by project owner.",
    "asset_count": len(records),
    "resized_count": sum(record["status"] == "resized" for record in records),
    "unchanged_count": sum(
        record["status"] == "unchanged-within-budget" for record in records
    ),
    "source_bytes_before": sum(record["source_bytes"] for record in records),
    "optimized_bytes_after": sum(record["optimized_bytes"] for record in records),
}

with open(manifest_path, "w", encoding="utf-8") as stream:
    json.dump({"summary": summary, "assets": records}, stream, ensure_ascii=False, indent=2)

print("TEXTURE_SUMMARY=" + json.dumps(summary, separators=(",", ":")))
