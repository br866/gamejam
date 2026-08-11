# Asset Optimization Batch Manifest

## Batch Identity

| Field | Value |
|---|---|
| Batch ID | `2026-08-11-high-triangle-fbx-001` |
| Reviewer | Performance-priority acceptance requested |
| Unity version | `2022.3.62f3c1` |
| Target platform | Standalone / PC |
| Status | Accepted with known rear-detail degradation |

## Candidate Asset

| Runtime path relative to `UnityProject/Assets` | Backup path relative to `Art/BackUp` | Type | Classification | Original SHA-256 | Original size | Original triangles | Target budget | Optimized size | Optimized triangles |
|---|---|---|---|---|---:|---:|---:|---:|---:|
| `SuperBreadMan/Scene Model/Level2/electro-medical_cabinet/electro-medical_cabinet.fbx` | Same relative path | Static FBX | High-triangle static prop | `AAC23B7E8142EFFB8F17604C2AFBE5C641CD44F8ED32AA59DB752AE6AD33E0F5` | 40,510,012 B | 39,254 | 30-40% | 530,492 B | 13,700 (34.9%) |

## Blender Structural Validation

| Check | Original | Optimized candidate | Result |
|---|---|---|---|
| Mesh object name | `electro-medical_cabinet` | Preserved | Passed |
| Material slots | 1 | 1 | Passed |
| UV layers | 1 | 1 | Passed |
| Armatures | 0 | 0 | Passed |
| Actions | 0 | 0 | Passed |
| Blend shapes | 0 | 0 | Passed |

## Unity Preservation and Reference Checks

| Check | Result | Evidence |
|---|---|---|
| Runtime path unchanged | Passed | Optimized FBX replaced the original file in place |
| Existing `.meta` retained | Passed | The existing `.meta` file was not deleted or regenerated |
| Asset GUID unchanged | Passed | `a614e673090e73342a7c9f8e37f18e72` resolved after original restore and optimized restore |
| Reimported model resolves | Passed | Unity loaded the root GameObject after each import |
| Art-scene cabinet instances resolve | Passed | The art scene loaded two active cabinet renderers after optimized reimport |
| Art-scene material binding resolves | Passed | Both instances used valid `Material.001` with `Universal Render Pipeline/Lit` |
| Unity console errors | Passed | No errors after optimized reimport and art-scene load |

## Rollback

| Step | Result | Evidence |
|---|---|---|
| Restore source content from `Art/BackUp` | Passed | Original FBX restored at 40,510,012 B |
| Retain runtime `.meta` | Passed | GUID resolved after restore |
| Reapply optimized output | Passed | Optimized FBX restored at 530,492 B with the original GUID |

## Results

| Metric | Before | After | Result |
|---|---:|---:|---|
| FBX source file size | 40,510,012 B | 530,492 B | Approximately 98.7% reduction |
| Triangle count | 39,254 | 13,700 | Approximately 65.1% reduction |
| Git working-tree size | Not measured independently | Not measured independently | Pending batch-level baseline |
| Unity mesh runtime memory | Not measured | Not measured | Pending profiler measurement |
| PC build contribution | Not measured | Not measured | Pending PC build |
| Intended-scene visual comparison | No automated baseline | Additive Scene View targeting did not resolve the object | Pending manual review |

## Acceptance

Accepted for the current performance-priority pass. The unrestricted Decimate operation visibly degrades rear geometry, but the project owner has explicitly accepted that trade-off because runtime performance is more important than model completeness. The optimized 13,700-triangle, 530,492 B FBX is active at the original runtime path with its original GUID. Future static-FBX batches may use this profile only when their visual degradation is explicitly acceptable; the original remains recoverable from `Art/BackUp`.
