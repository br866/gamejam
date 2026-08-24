## 1. Nearby Candidate Audit

- [x] 1.1 Capture current selection bounds, source identities, and the eight-unit expanded scan volume.
- [x] 1.2 Inventory nearby visual candidates and classify accepted, Level 2 excluded, Level 4 excluded, same-position duplicate, and non-visual objects.
- [x] 1.3 Exclude only Level 2, Level 4, and existing Formal Level 3 same-position duplicates before copying.

## 2. Level 3 Visual Completion

- [x] 2.1 Rebuild `L03_Content` from the existing Level 3 visual union plus accepted nearby visual candidates.
- [x] 2.2 Preserve source world position, rotation, and scale while stripping source runtime behavior from copied visuals.
- [x] 2.3 Reinstance the completed visual Prefab in Formal Level 3 without modifying scene-owned anchors or collision roots.

## 3. Verification And Record

- [x] 3.1 Validate completed visual object count and representative source-to-destination world transforms.
- [x] 3.2 Revalidate Level 3 spawn support and baseline route clearance after nearby art completion.
- [x] 3.3 Record scan bounds, inclusion counts, exclusions, and deferred ownership questions in `Level03SourceManifest.md`.
- [x] 3.4 Inspect Console and verify the source scene and Formal Level 2 remain unchanged.
