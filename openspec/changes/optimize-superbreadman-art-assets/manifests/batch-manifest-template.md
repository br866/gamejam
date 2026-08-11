# Asset Optimization Batch Manifest

## Batch Identity

| Field | Value |
|---|---|
| Batch ID | `YYYY-MM-DD-<classification>-<sequence>` |
| Reviewer | |
| Unity version | `2022.3.62f3c1` |
| Target platform | Standalone / PC |
| Status | Planned / Validated / Accepted / Rolled back |

## Candidate Assets

| Runtime path relative to `UnityProject/Assets` | Backup path relative to `Art/BackUp` | Type | Classification | Original SHA-256 | Original size | Original dimensions / triangles | Target budget | Optimized SHA-256 | Optimized size | Optimized dimensions / triangles |
|---|---|---|---|---|---:|---|---|---|---:|---|
| | | Color / Normal / Data / Emission / Static FBX | | | | | | | | |

## Required Preservation Checks

| Check | Result | Evidence |
|---|---|---|
| Runtime path unchanged | Pending | |
| Existing `.meta` retained unchanged | Pending | |
| Asset GUID unchanged | Pending | |
| FBX object names, material slots, UVs, and orientation retained | Not applicable / Pending | |
| No rig, animation, or blend-shape regression | Not applicable / Pending | |

## Unity Import Settings

| Runtime path | Texture type | sRGB | Mipmaps | Standalone maximum size | Compression / format | Validation result |
|---|---|---:|---:|---:|---|---|
| | | | | | | |

## Validation

| Check | Before | After | Result |
|---|---|---|---|
| Material texture assignments | | | |
| Prefab and scene references | | | |
| Intended scene visual comparison | | | |
| Unity PC texture memory | | | |
| Runtime asset disk size | | | |
| Git working-tree size | | | |
| PC build contribution, if measurable | | | |

## Rollback

| Step | Result | Evidence |
|---|---|---|
| Restore source content from `Art/BackUp` to its runtime path | Pending | |
| Retain original runtime `.meta` file | Pending | |
| Refresh Unity and verify references | Pending | |

## Acceptance

Accepted only after all applicable validation and rollback checks pass.
