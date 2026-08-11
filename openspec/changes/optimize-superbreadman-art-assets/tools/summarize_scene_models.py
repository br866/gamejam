import json
import os
import sys


assets_root = sys.argv[sys.argv.index("--") + 1]
backup_root = sys.argv[sys.argv.index("--") + 2]
manifest_path = sys.argv[sys.argv.index("--") + 3]

with open(manifest_path, encoding="utf-8") as stream:
    manifest = json.load(stream)

records = manifest["assets"]
optimized = [record for record in records if record["status"] == "optimized"]
backup_total = 0
runtime_total = 0
missing_runtime = []

for record in optimized:
    relative_path = record["path"]
    backup_path = os.path.join(backup_root, relative_path)
    runtime_path = os.path.join(assets_root, relative_path)
    backup_total += os.path.getsize(backup_path)
    if os.path.exists(runtime_path):
        runtime_total += os.path.getsize(runtime_path)
    else:
        missing_runtime.append(relative_path)

print(
    "SUMMARY="
    + json.dumps(
        {
            "optimized_count": len(optimized),
            "backup_bytes": backup_total,
            "runtime_bytes": runtime_total,
            "bytes_saved": backup_total - runtime_total,
            "triangles_before": sum(record["triangles_before"] for record in optimized),
            "triangles_after_recorded": sum(
                record["triangles_after"] for record in optimized
            ),
            "missing_runtime_paths": missing_runtime,
        },
        separators=(",", ":"),
    )
)
