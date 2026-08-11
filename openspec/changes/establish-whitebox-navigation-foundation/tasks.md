## 1. Whitebox Inventory and Classification

- [x] 1.1 Inspect `Assets/MoMing/Scenes/Test/superbreadman.unity` and record each route-relevant object as walkable ground, static blocker, dynamic blocker, actor, trigger, or navigation-ignored.
- [ ] 1.2 Measure the world-space collider footprints of the human, dog, and monster, including the dog's scaled capsule dimensions, before later navigation work.
- [x] 1.3 Identify existing movable physical objects that must participate in dynamic navigation updates and exclude visual-only objects and triggers.

## 2. Layers and Collision Geometry

- [x] 2.1 Add `NavGround`, `NavStatic`, `NavDynamic`, `NavIgnore`, `Player`, `Enemy`, and `Trigger` layers in `ProjectSettings/TagManager.asset`; preserve existing collision-matrix defaults until the scene owner assigns object layers.
- [ ] 2.2 Have the scene owner apply the defined layer roles to whitebox objects, player objects, monster objects, and interaction volumes without changing transforms or route layout.
- [ ] 2.3 Run the target-scene-only collider utility to replace every MeshCollider in `Assets/MoMing/Scenes/Test/superbreadman.unity` with a BoxCollider derived from the source mesh bounds, retaining its enabled, trigger, material, and contact-offset settings.
- [ ] 2.4 Validate the converted scene geometry after scene-owner Layer assignment.

## 3. Deferred Player Physics Stabilization

- [ ] 3.1 Update Rigidbody-based human and dog movement so physics owns all position, rotation, and follow movement.
- [ ] 3.2 Filter playable-character ground detection to `NavGround` and exclude trigger colliders.
- [ ] 3.3 Replace inactive linked-character direct transform following with collision-consistent Rigidbody movement while preserving current thresholds and switch behavior.
- [ ] 3.4 Validate character movement against obstacles, switching, linked movement, existing interactions, and checkpoint recovery in the whitebox.

## 4. Deferred Whitebox Navigation and Door Graphs

- [ ] 4.1 Add player navigation graphs that scan `NavGround`, exclude `NavStatic`, and use actor dimensions measured in task 1.2.
- [ ] 4.2 Add a door controller and player-only `Trigger` interaction at each level boundary; closed doors block player graph traversal and opened doors update the affected player graph region.
- [ ] 4.3 Configure each monster with a graph or graph tag limited to its assigned level, independent of door-open state.
- [ ] 4.4 Configure dynamic navigation updates for identified `NavDynamic` physical blockers using their collider bounds, and validate player and monster routes independently.

## 5. Deferred Monster Navigation Integration

- [x] 5.1 Add a focused monster navigation movement component that manages destination requests, path following, arrival, path failure, and repath cadence.
- [x] 5.2 Configure the Level2 monster in the target whitebox with required navigation movement, actor-clearance, and assigned-level settings.
- [x] 5.3 Update `MonsterPatrol` to retain patrol/chase state, bounds, detection, capture, reset, and audio ownership while delegating physical movement to the navigation component and rejecting targets outside its assigned level.
- [ ] 5.4 Validate patrol around static blockers, chase around dynamic blockers, player door traversal, monster no-cross-level behavior, unreachable chase targets, capture, reset, and resumed patrol behavior.

### Level2 Detection Adjustment

- [x] Use view-cone detection only for Level2 monster pursuit; remove hearing-radius pursuit and its debug visualization.
- [x] Fix navigation waypoint advancement to ignore vertical graph-node height during horizontal monster movement.
- [x] Use the configured patrol speed for Level2 waypoint patrol and the configured chase speed for pursuit.

## 6. Deferred Route Verification and Scope Review

- [ ] 6.1 Perform a Unity Editor Play Mode pass through the required whitebox route, including player collision, linked movement, monster encounter, dynamic obstacle behavior, and failure recovery.
- [ ] 6.2 Confirm no changes were made to `Assets/Scenes/Test/superbreadman 1.unity`, unrelated MoMing documents, UI, audio assets, models, materials, or lighting.
- [ ] 6.3 Review changed scene and prefab serialization for unintended object additions, deletions, transform edits, or unrelated asset churn.

## 7. Automated and Change Validation

- [ ] 7.1 Run available C# compilation or Unity test workflows and resolve errors introduced by the collider-conversion utility.
- [ ] 7.2 Run `openspec validate --all` and address planning validation errors before requesting implementation review.
