## 1. Source Classification

- [x] 1.1 Capture the active source selection with GlobalObjectId, hierarchy path, mesh identity, and effective world Transform.
- [x] 1.2 Accept only explicit `Static Scene/Level3` visual candidates after excluding formal Level 1/2 world-position duplicates.
- [x] 1.3 Record Level 4, player/runtime, interaction, and unresolved shared candidates with their exclusion or review reason.

## 2. Formal Level 3 Art Assembly

- [x] 2.1 Create `FormalLevel03` and a Level 3-owned content Prefab without changing the source scene.
- [x] 2.2 Copy accepted Level 3 visual objects using effective source world position, rotation, and scale.
- [x] 2.3 Create `Level03SourceManifest.md` containing accepted source identities, exclusions, unresolved candidates, and selection summary.

## 3. Verification And Handoff

- [x] 3.1 Verify every assembled Level 3 visual against its source world Transform within the recorded tolerance.
- [x] 3.2 Inspect Formal Level 3 and verify Formal Level 1, Formal Level 2, and the source scene remain unchanged.
- [x] 3.3 Check the Unity Console and record Level 3 traversal, collision, player anchors, navigation, and mechanics as deferred follow-up work.
