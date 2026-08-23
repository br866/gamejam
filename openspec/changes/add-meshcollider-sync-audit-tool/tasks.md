## 1. Tool Implementation

- [x] 1.1 Create `Assets/MoMing/Scripts/Editor/MeshColliderSyncAuditor.cs` with `Tools/Final/` menu group, selection-scoped audit (Ok/Proxy/Broken/Missing), and confirm-guarded repair writing `m_Mesh` via SerializedObject.
- [x] 1.2 Report per-issue console lines plus a summary; report-only path must not modify any component.

## 2. Tests

- [x] 2.1 Add `Assets/Editor/FormalMeshColliderSyncAuditorTests.cs` covering classification of synthetic Ok/Proxy/Broken/Missing fixtures.
- [x] 2.2 Assert repair reassigns collider mesh to renderer mesh and a follow-up audit reports no Broken/Missing issues.
- [x] 2.3 Add regression test auditing `L01_Content.prefab` contents expecting zero Broken/Missing violations.

## 3. Validation

- [x] 3.1 Project compiles with no errors or warnings from the new files.
- [x] 3.2 EditMode tests pass via the Unity Test Framework.
- [x] 3.3 End-to-end smoke: audit `FormalLevel01.unity` content hierarchy and confirm zero Broken/Missing findings.
