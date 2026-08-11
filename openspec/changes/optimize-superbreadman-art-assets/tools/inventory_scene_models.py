import bpy
import json
import os
import sys


root = sys.argv[sys.argv.index("--") + 1]
records = []
files = []

for directory, _, names in os.walk(root):
    for name in names:
        if name.lower().endswith(".fbx"):
            files.append(os.path.join(directory, name))

for path in sorted(files):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=path)
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    records.append(
        {
            "path": os.path.relpath(path, root).replace("\\", "/"),
            "triangles": sum(
                sum(len(polygon.vertices) - 2 for polygon in obj.data.polygons)
                for obj in meshes
            ),
            "meshes": len(meshes),
            "armatures": sum(
                1 for obj in bpy.context.scene.objects if obj.type == "ARMATURE"
            ),
            "actions": len(bpy.data.actions),
            "shape_keys": sum(
                len(obj.data.shape_keys.key_blocks) - 1
                for obj in meshes
                if obj.data.shape_keys
            ),
            "bytes": os.path.getsize(path),
        }
    )

print("ASSET_RECORDS=" + json.dumps(records, separators=(",", ":")))
