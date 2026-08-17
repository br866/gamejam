## Context

See `proposal.md` for motivation. The formal route already owns a persistent Human/Dog pair and additively loads route scenes through `FormalGameFlowController`. Each scene provides a `FormalLevelController`, paired spawn anchors, visual content, and collision geometry, but the structure is not fully consistent or automatically verified. The route also needs explicit navigation APIs and a way to retain art scenes shared by adjacent levels.

The repository also contains a prototype mechanic stack (`Puzzle/*`, `GameManager`, `PlayerManager`) built around a single loaded scene, `Player` tags, and a global reset event. Formal Level 01 instead has a small parallel set of formal scripts. The two lifecycles cannot safely share mechanics unchanged.

## Goals / Non-Goals

**Goals:**

- Make the formal route's existing scene and lifecycle rules explicit and editor-verifiable.
- Keep a single formal lifecycle owner: `FormalLevelController` for scene-local reset and `FormalGameFlowController` for cross-scene flow.
- Establish composable formal mechanics where eligibility, completion persistence, and environmental effects are configured independently.
- Migrate Level 01's existing formal interactions as the first compatibility proof.
- Make route navigation and shared-scene retention usable from code and GM/debug commands without introducing a command parser.

**Non-Goals:**

- Replacing, deleting, or modifying prototype `GameManager`, `PlayerManager`, or `Puzzle/*` behavior.
- Implementing anxiety, enemy, UI, audio, navigation, collectible inventory, timed interaction, sequence, or hold-switch gameplay.
- Changing completed art assemblies or rebuilding existing collision layouts beyond wiring needed formal mechanic objects.
- Making progress persist across application restarts; "permanent" means only for the loaded level session.
- Automatically discovering shared art by scanning renderer dependencies or object names.

## Decisions

### Keep scene-local state under the formal level controller

`FormalLevelController` remains the reset registry and spawn/checkpoint owner. Extend the current temporary/permanent state interfaces into a clear reset policy rather than introducing another singleton or global event bus.

This keeps a mechanism naturally scoped to the scene that owns it, which is necessary while predecessor and successor scenes coexist during additive transitions. The alternative of reusing `GameManager.OnLevelReset` couples formal scenes to prototype players and gives no safe scene ownership boundary.

### Treat the scene skeleton as a contract, not a generated prefab

Formal scenes will have a documented and validated root structure: one controller, two named entrance anchors, one visual content root, and one collision root. Existing scenes will be aligned through minimal scene wiring.

A mandatory scene prefab was rejected because each level already has independently assembled art and collision layouts; forcing a nested prefab would create avoidable YAML churn and make future art migration more difficult. Validation provides the same reliability without ownership conflicts.

### Separate trigger eligibility, state policy, and actuator behavior

Formal interactions will use three composable responsibilities:

```text
occupant / interaction
        -> eligibility policy
        -> mechanism state policy
        -> actuator target(s)
```

Eligibility identifies formal actor roles or supported physics occupants. State policy decides permanent versus resettable completion. Actuators apply an environmental result, starting with a reversible door. A key or pedal becomes a thin configuration of these common capabilities rather than a bespoke direct door script.

This is intentionally smaller than a generic event graph. A graph framework would add editor complexity before the route has more than the basic one-way and reversible interactions it needs.

### Formal player identity is authoritative

Formal triggers resolve a `FormalPlayerActor` in the entering collider's parent and evaluate its role. They do not inspect the prototype `Player` tag or `PlayerController`.

This supports persistent formal actors whose colliders and visual hierarchy can change, while preventing accidental activation by prototype-only objects.

### Door behavior supports both permanent and reversible control

The formal door retains opening animation and collider management, adding an explicit close/reset path. A permanent producer only opens it; a resettable producer can return it to closed state during formal reset.

The old `GateController` is not altered. It remains the prototype execution path until a later dedicated migration change.

### Validation expands the existing editor test suite

Extend `FormalTraversalValidationTests` with a single authoritative list of formal route scene paths. It will open scenes additively, inspect contract objects, run physics support/overlap checks, and close scenes without saving.

This reuses the established NUnit and `EditorSceneManager` pattern rather than introducing an editor window. An editor window was rejected because CI-friendly test failures are the needed feedback mechanism.

### Use an explicit route catalog for fast level management

Add a serialized route catalog to the persistent flow. Each entry contains a stable level id, scene name, and zero or more shared additive scene names. The catalog is the single source for next/previous navigation and direct jumps; it avoids addressable assets, asset scanning, and a second configuration format.

Expose `LoadLevel`, `LoadLevelAsync`, `UnloadLevel`, `UnloadLevelAsync`, `GoToNextLevel`, `GoToPreviousLevel`, and `JumpToLevel` on the flow controller. Synchronous methods complete the Unity scene operation before returning. Asynchronous methods start the operation and invoke a completion callback, while all methods use the same ownership reconciliation path.

### Retain shared art by explicit scene references and reference counts

Shared art is delivered as independent additive scenes. A route entry declares which shared scenes it needs. The runtime keeps a reference count for every loaded shared scene based on all active and transitional route entries. A shared scene unloads only when its count reaches zero; unloading a level never directly unloads its shared art.

The first implementation keeps both the predecessor and successor route entries in the retention set during a transition. This handles shared art between adjacent levels without hidden object ownership rules. Persistent art remains in `FormalPersistent` and is not part of the catalog.

### Keep GM commands as public methods plus Inspector commands

The first GM surface is intentionally small: public methods usable by an existing debug console or test, Unity `ContextMenu` entries for editor use, and development-only keyboard shortcuts for next, previous, and direct configured jump. A text command parser is deferred until an actual console exists.

## Risks / Trade-offs

- [Existing scenes do not share an identical hierarchy] -> Validate semantic components and named anchors/roots, not full hierarchy paths; repair only missing required contract objects.
- [Permanent state has no save-game persistence] -> Define it explicitly as current loaded-level-session permanence and defer cross-session persistence.
- [Physics occupancy can admit unintended rigidbodies] -> Start with an explicit formal pushable-object marker and do not accept arbitrary rigidbodies.
- [Closing a door can overlap players] -> Reset only repositions players after resettable state restoration; validation and Level 01 coverage will ensure entry anchors are clear. Future mid-play reversible doors need a separate obstruction policy before use.
- [Prototype and formal systems remain duplicated temporarily] -> Maintain a hard dependency boundary and migrate one mechanic family at a time.
- [A shared scene is accidentally omitted from a route entry] -> Validate every catalog entry and report missing build settings or duplicate scene ownership before play mode.
- [A failed async load leaves the old route partially retained] -> Reconcile ownership only after successful load/unload completion and expose the operation result through the callback.

## Migration Plan

1. Add the route catalog and explicit synchronous/asynchronous flow APIs while preserving the current serialized initial-level setting.
2. Add shared additive-scene retention and route-aware next/previous/direct-jump commands.
3. Add common formal interfaces/components and preserve existing serialized behavior where direct replacements would otherwise break Level 01 references.
4. Wire Level 01 key, pedal, door, and crate to the shared contracts; verify reset and human-only activation in play mode.
5. Normalize formal scene roots and add the contract/catalog validation suite for all six route scenes.
6. Run edit-mode validation and the existing traversal suite before adopting the new components for later level mechanics.

Rollback is limited to removing the new common components and restoring the Level 01 script references from version control; no data migration or external save format is involved.
