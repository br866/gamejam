## Context

See `proposal.md` for motivation and `specs/whitebox-navigation-foundation/spec.md` for the required behavior. The target whitebox is `Assets/MoMing/Scenes/Test/superbreadman.unity`; its art-scene alignment remains deferred. The project already contains a verified isolated A* navigation smoke test, while production player movement uses Rigidbody and the current monster patrol script directly changes transforms.

## Goals / Non-Goals

**Goals:**
- Convert target-scene mesh collision to approximate BoxCollider collision without modifying source Prefabs or other scenes.
- Define a Layer vocabulary that the scene owner can assign to whitebox objects.
- Define how door controllers control player level transitions while monsters remain within their assigned level.
- Provide a practical later setup sequence for interactions and A* navigation.

**Non-Goals:**
- Rebuild the whitebox layout, alter room routing, or add scene content to solve navigation gaps.
- Apply layers automatically or configure collision matrix rules before the scene owner has classified objects.
- Implement player movement, monster navigation, interaction objects, or A* graph objects in this pass.
- Apply any scene or Prefab changes to the paired art scene during this change.

## Decisions

### Use explicit layer roles shared by collision and navigation

The project will add dedicated layers for `NavGround`, `NavStatic`, `NavDynamic`, `NavIgnore`, `Player`, `Enemy`, and `Trigger`. The implementation will inventory existing whitebox objects and assign one gameplay role to each relevant object before configuring collision matrices and graph masks.

This makes ground probing, physical collision, static graph scanning, and dynamic graph updates use the same scene classification. It replaces unfiltered physics queries and avoids inferring semantics from object names.

Alternative considered: use existing default layers and individual component masks. This was rejected because masks would drift between player grounding, colliders, and navigation configuration.

### Convert only target-scene MeshColliders to BoxColliders

The conversion utility opens only `Assets/MoMing/Scenes/Test/superbreadman.unity`, enumerates MeshCollider components that belong to that loaded scene, and replaces each valid MeshCollider with a BoxCollider on the same GameObject. The new collider uses the source mesh's local bounds and preserves enabled, trigger, physics material, and contact-offset values.

This deliberately favors speed over precise collision. Mesh bounds can close a doorway or alter a slope, stair, L-shaped, or concave boundary, so the result requires editor validation. Scene ownership filtering prevents changes to shared source Prefabs and other maps.

Alternative considered: manually replace each collider or retain meshes for irregular geometry. This was rejected at the scene owner's request in favor of a coarse all-BoxCollider whitebox.

### Leave object classification to the scene owner

`NavGround`, `NavStatic`, `NavDynamic`, `NavIgnore`, `Player`, `Enemy`, and `Trigger` are created in project Layer slots 8 through 14. The scene owner assigns them to existing objects after reviewing the converted collision volumes; the implementation does not infer category from hierarchy names or mutate any object Layer.

This avoids a bulk classification that could give a wall the ground role, make a trigger block navigation, or assign a player-only rule to an enemy.

Alternative considered: automatically classify objects based on ProBuilder mesh names or hierarchy placement. This was rejected because current whitebox geometry does not encode dependable gameplay roles.

### Use separate player and monster graph constraints

Later setup will use the installed A* package to scan `NavGround`, with `NavStatic` and `NavDynamic` as collision obstacles. Doors remain physical `NavStatic` blockers when closed; their controller updates their collider and graph area when player passage changes. Monster path requests must be constrained to the monster's assigned level graph or level-specific graph tag, even when a door is open.

Separating monster reachability from door openness prevents a player unlock from accidentally allowing a monster to pursue across levels. The isolated smoke-test scene remains a regression reference rather than becoming a runtime dependency of the level.

Alternative considered: use one unrestricted global graph for every actor. This was rejected because it violates the monster's no-cross-level rule.

### Future interaction and navigation setup

For each level boundary, create a `DoorController` owner object and a physical door object. The door's BoxCollider is `NavStatic` while closed; when the controller opens it for players, the collider and corresponding graph region are updated. Add a trigger volume on the `Trigger` layer only for player interaction; it does not contribute collision or navigation obstruction.

For each monster level, use either one bounded GridGraph or a graph tag that covers only that level. Patrol points and valid chase positions share that same level identifier. If the player changes level, clear or retarget the monster's chase request instead of allowing a route through the connection door.

Dynamic blockers later use `NavDynamic` and local graph updates; visual-only objects use `NavIgnore` and never participate. Alternative considered: a fully open global graph plus runtime checks only. This was rejected because a path can cross a door before the runtime check rejects it.

## Risks / Trade-offs

- [Incorrect layer assignment makes a route look walkable or blocked] -> Inventory each changed object by role, visualize or inspect the graph in the Unity Editor, and run representative player and monster traversal checks.
- [Dog scale makes its collider footprint differ from the human baseline] -> Record world-space capsule dimensions from the prefab and configure or validate clearance against the larger relevant footprint.
- [Box bounds change a doorway or irregular traversal edge] -> The scene owner validates converted geometry and manually adjusts only affected BoxCollider bounds.
- [A source Prefab or other scene is changed] -> The tool filters every collider by the explicitly opened target scene and never calls Prefab save APIs.
- [A door opens a route for a monster] -> Assign monster graphs or graph tags per level, independently of player door state.

## Migration Plan

1. Let Unity compile `WhiteboxColliderTools.cs`, then run `Tools > SuperBreadMan > Replace Whitebox Mesh Colliders With Boxes` once.
2. Inspect the target scene and adjust only BoxCollider bounds that visibly break required whitebox passage.
3. Assign the new Layers manually to scene objects.
4. Add door controllers, player interaction triggers, level-bounded graphs, and monster graph restrictions in the later navigation implementation.

Rollback is file-scoped: revert `TagManager.asset`, `WhiteboxColliderTools.cs`, and only the target whitebox scene after it is converted. The validated isolated A* smoke-test scene, all source Prefabs, and other scenes remain untouched.

## Open Questions

- The exact world-space dog capsule footprint must be measured before its navigation clearance is configured.
- The owner must identify the exact whitebox objects that represent each door and define the level membership of each future monster.
