import bpy
import hashlib
import json
import os
import shutil
import sys


assets_root = sys.argv[sys.argv.index("--") + 1]
backup_root = sys.argv[sys.argv.index("--") + 2]
manifest_path = sys.argv[sys.argv.index("--") + 3]
skip_relative_paths = {
    "Level2/electro-medical_cabinet/electro-medical_cabinet.fbx",
}
records = []
files = []


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest().upper()


for directory, _, names in os.walk(assets_root):
    for name in names:
        if name.lower().endswith(".fbx"):
            files.append(os.path.join(directory, name))

for source_path in sorted(files):
    relative_path = os.path.relpath(source_path, assets_root).replace("\\", "/")
    backup_path = os.path.join(backup_root, relative_path)
    os.makedirs(os.path.dirname(backup_path), exist_ok=True)
    if not os.path.exists(backup_path):
        shutil.copy2(source_path, backup_path)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=source_path)
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    triangles_before = sum(
        sum(len(polygon.vertices) - 2 for polygon in obj.data.polygons)
        for obj in meshes
    )
    armatures = sum(1 for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    actions = len(bpy.data.actions)
    shape_keys = sum(
        len(obj.data.shape_keys.key_blocks) - 1
        for obj in meshes
        if obj.data.shape_keys
    )
    source_bytes = os.path.getsize(source_path)
    backup_hash = sha256(backup_path)

    if relative_path in skip_relative_paths:
        records.append(
            {
                "path": relative_path,
                "status": "skipped-already-optimized",
                "backup_sha256": backup_hash,
                "source_bytes": source_bytes,
                "triangles_before": triangles_before,
            }
        )
        continue

    if armatures or actions or shape_keys:
        records.append(
            {
                "path": relative_path,
                "status": "skipped-risky-structure",
                "backup_sha256": backup_hash,
                "source_bytes": source_bytes,
                "triangles_before": triangles_before,
                "armatures": armatures,
                "actions": actions,
                "shape_keys": shape_keys,
            }
        )
        continue

    if triangles_before < 1500:
        records.append(
            {
                "path": relative_path,
                "status": "skipped-low-triangle-count",
                "backup_sha256": backup_hash,
                "source_bytes": source_bytes,
                "triangles_before": triangles_before,
            }
        )
        continue

    ratio = 0.5 if triangles_before < 8000 else 0.35
    for obj in meshes:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new(
            name="AssetOptimizationDecimate", type="DECIMATE"
        )
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)

    triangles_after = sum(
        sum(len(polygon.vertices) - 2 for polygon in obj.data.polygons)
        for obj in meshes
    )
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    temporary_path = source_path + ".asset-optimization.tmp.fbx"
    bpy.ops.export_scene.fbx(
        filepath=temporary_path,
        use_selection=True,
        object_types={"MESH"},
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
        mesh_smooth_type="FACE",
    )
    os.replace(temporary_path, source_path)
    records.append(
        {
            "path": relative_path,
            "status": "optimized",
            "backup_sha256": backup_hash,
            "source_bytes": source_bytes,
            "optimized_bytes": os.path.getsize(source_path),
            "optimized_sha256": sha256(source_path),
            "triangles_before": triangles_before,
            "triangles_after": triangles_after,
            "ratio": ratio,
            "mesh_count": len(meshes),
        }
    )

with open(manifest_path, "w", encoding="utf-8") as stream:
    json.dump(records, stream, ensure_ascii=False, indent=2)

print("BATCH_RESULTS=" + json.dumps(records, ensure_ascii=False, separators=(",", ":")))
