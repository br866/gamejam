## Context

See `proposal.md` and `specs/superbreadman-final-level-requirements/spec.md`. Existing scripts and scenes are prototype material only; the final behavior must be verified against the captured requirements. This is a cross-cutting gameplay contract spanning checkpoint state, mechanisms, doors, physics objects, monster behavior, cameras, level scenes, and controlled escape flow.

## Goals / Non-Goals

**Goals:**
- Establish a single progress-lifetime model that prevents deaths from producing route soft locks.
- Make character eligibility and safe-space behavior explicit rather than accidental results of scene layout.
- Preserve physical-object puzzles as actual physics interactions rather than replacing them with completion triggers.
- Define the Level 5 escape as a bounded mode that can temporarily override standard dual-character controls.

**Non-Goals:**
- Treat the PDF as the sole authority or silently discard it.
- Assume existing scripts, scene objects, or previous OpenSpec planning are already correct.
- Choose the Level 5 checkpoint position, cabinet-area safety, or final completion presentation before those gameplay decisions are made.
- Implement this capture or prescribe exact class names, component layouts, asset choices, or navigation libraries.

## Decisions

### Separate temporary and permanent current-level progress

Keys and movable-object transforms reset on death; completed mechanisms, opened doors, and checkpoints remain. This lets physical puzzles be replayed when failed while preserving path unlocks that have already been earned.

Alternative considered: reset every object and door on death. Rejected because a player could respawn beyond a door that has reclosed, or be forced to repeat solved route gates inconsistently with the agreed behavior.

### Make doors own visual, physical, and progression state

A door stays physically closed until all configured requirements are complete, then plays an opening animation and becomes non-blocking for the remainder of the level. Door requirements are data-configured so one door can require one mechanism while another requires multiple.

Alternative considered: treat an animation-only visual object as the door. Rejected because its displayed state could diverge from collision and traversability.

### Enforce actor eligibility on the interaction

Keys, plates, switches, and cooperative actions enforce who is allowed to advance them. Scene placement remains important for level design but does not replace eligibility checks.

Alternative considered: rely on narrow routes or unreachable placement to restrict actors. Rejected because an unintended movement route could bypass the intended puzzle role.

### Keep the Level 1 physical-step puzzle human-only for the first formal pass

The Level 1 wooden crate is the sole movable object required to form the step beneath the broken-wall route. The human pushes the crate into its usable placement area, then uses it to access the human-only route and key. The selected stool assets remain non-interactive scenery with physical blocking collision. The dog is not a required participant in this first formal version of the puzzle.

Future dog participation, such as releasing a crate restraint or opening a dog-only access route, remains a separate gameplay decision and SHALL NOT be implied by the current crate behavior.

Alternative considered: require human-and-dog cooperative movement of a stool and crate. Deferred because the current confirmed art mapping provides one movable crate while both stool assets are scenery, and the existing human push action is sufficient for the initial vertical slice.

### Separate line-of-sight obstruction from monster safety

Hiding geometry can block visible sight but does not break a real-time pursuit lock. A safe space is a stronger spatial boundary: monsters cannot enter or attack within it. This keeps hiding and safety vocabulary distinct even when both use physical geometry.

Alternative considered: let any line-of-sight obstruction cancel pursuit. Rejected because the agreed rule says monsters continuously pursue a locked player's real-time position.

### Use a controlled escape mode for the Level 5 corridor

The corridor temporarily disables switching and voluntary separation, applies a fixed camera, unlocks running, and keeps both monsters pursuing while players cooperatively move a physical cabinet. Entering the final room ends the corridor-specific control constraints because there is no later fifth-level segment requiring them.

Alternative considered: keep normal free dual-character controls during the corridor. Rejected because fixed camera, two-character separation, real-time pursuit, and physical pushing would compete for player attention and create conflicting control rules.

### Delay prior-level cleanup until the next checkpoint is established

Prior-level content remains valid while players have entered the next level but have not yet activated its checkpoint. After the next checkpoint is activated, cleanup occurs at a safe time when it cannot disrupt players, active monsters, or physics.

Alternative considered: destroy prior content on boundary entry. Rejected because a death before the next checkpoint could require state that was already removed.

### Use additive level scenes with one persistent scene

The formal game uses a persistent scene for players, camera, UI, audio, and global flow. Each playable level is a separately additive-loaded Unity scene that owns its checkpoint, spawn points, level controller, gameplay triggers, and a level-content root. This allows the project to preserve the current and immediately prior level during checkpoint handoff, then unload the prior level as a complete unit.

Players, camera, UI, audio, and global flow must not be parented under a level-scene object or otherwise become owned by a level scene. Before unloading a prior level, persistent systems must release or replace references to its objects, including camera targets, monster targets, held items, and cached gameplay references.

Alternative considered: keep the entire route in one formal scene and toggle level roots. Rejected because additive scenes provide Unity-owned unloading of all level-local objects, including instantiated content, rather than requiring manual cleanup of every level root and cross-level reference.

### Assemble formal levels from reusable prefab units

Whitebox scenes remain functional experiments and are not the authority for formal geometry or collision. Formal level scenes define the level-specific arrangement, spawn points, triggers, and controller configuration. Reusable room, corridor, architectural segment, interactive-object, and gameplay prefabs provide the content instantiated within the level-content root.

Prefab boundaries follow independent placement, reuse, interaction, or lifecycle. Decorative model fragments remain inside their owning room or architectural prefab and are not independently loaded. A full level may use multiple room or corridor prefabs rather than requiring one monolithic level prefab or one prefab per mesh object.

Alternative considered: dynamically load every art GameObject as a separate prefab. Rejected because it multiplies loading, instantiation, placement, and reference-management points without providing independent lifecycle value.

### Use art-aligned simplified collision proxies

Formal collision is authored to match the intended visible boundaries of art assets, but it is not generated from whitebox collision or assumed to be identical to render meshes. Static architectural prefabs use simple or compound primitive colliders where possible. Static non-convex mesh colliders are reserved for a small number of irregular surfaces where primitive proxies cannot preserve required traversal. Dynamic Rigidbody gameplay objects use primitive or compound colliders rather than non-convex mesh colliders.

Gameplay triggers, checkpoints, safe-space boundaries, and monster access restrictions use explicit stable colliders. Navigation data is authored or baked from the collision proxy arrangement rather than inferred solely from render meshes.

Alternative considered: attach mesh colliders to all art meshes. Rejected because broad mesh-collider use makes physics cost, collider maintenance, and dynamic-object support less predictable.

### Level 1 formal asset mapping

The Level 1 formal asset inventory is limited to the explicitly selected art-scene GameObjects. Existing parent paths are source-tracking information only; the formal hierarchy SHALL be rebuilt around spatial and gameplay ownership rather than legacy names such as `wall5 (8)`.

The selected Level 1 assets map as follows:

| Formal role | Selected source object(s) | Formal treatment |
| --- | --- | --- |
| Mechanism door | `door5 (1)` | Independent Level 1 door prefab with visual content, blocking collision, and permanent opening state. |
| Level 2 exit door | `door4 (1)` | Independent Level 1 exit-door prefab with visual content, blocking collision, permanent opening state, and Level 2 handoff trigger. |
| Movable physical step | `wooden_crate (1)` | Human-pushable prefab with art-aligned primitive or compound collision, Rigidbody behavior, and a temporary reset anchor. |
| Mechanism visual | `Pedal1 (1)` | Visual content for the formal Level 1 mechanism; its interaction trigger is authored independently. |
| Decorative pedal | `Pedal2 (1)` | Non-interactive set dressing. |
| Decorative stools | `low wooden stool (1)`, `stool (1)` | Non-interactive set dressing with physical blocking collision. |
| Large blocking set dressing | `cabinet*`, `medical cabinet*`, `big metal locker*` | Non-interactive set dressing with simplified physical blocking collision. |
| Prototype-only gates | `Gate` objects and their related prototype controllers | Do not migrate to the formal level. |

The formal Level 1 architecture prefab contains the selected walls, floors, broken wall, door jambs, and other structural art under source-traceable visual children. Its collision children use purpose-based names such as `Collision_MainRoomBoundary`, `Collision_BrokenWallRoute`, and `Collision_ExitDoorway` rather than legacy mesh names or collider-type names. Set dressing may be grouped into room-level prefabs, while the crate, doors, checkpoint, and mechanism remain independently addressable gameplay prefabs.

The selected `PlayerSystem`, `boy`, and `dog` objects are not Level 1 content despite appearing in the selected set. They remain persistent-scene content and must survive Level 1 scene unloading. The selected `Level3/door2` source object is excluded from Level 1 migration.

## Risks / Trade-offs

- [A temporary key resets while a permanent door requires it] -> Ensure a door only becomes permanent after its key interaction has successfully completed, and reset key state without re-blocking an opened route.
- [Physics placement is unstable or bypassable] -> Validate the human push action, crate collision, placement area, jump clearance, reset position, and player access for the first and fifth level physical puzzles.
- [Real-time pursuit makes a puzzle or cabinet push impossible] -> Tune level geometry and monster entry distance through playable evaluation; do not add an unagreed safe space as a workaround.
- [A safe space is visually reachable by a monster] -> Validate both physical and navigation access, plus attack radius, before treating it as safe.
- [Level cleanup destroys active objects] -> Gate cleanup on next-checkpoint establishment and a safe lifecycle boundary.
- [Fixed-camera escape hides essential information] -> Validate that players can read cabinet position, exit state, player positions, and approaching threat from the fixed view.
- [Level unload leaves persistent systems pointing at destroyed objects] -> Clear or replace all prior-level references before calling Unity scene unload.
- [Art changes silently alter traversal] -> Review collision proxies and navigation after art-prefab geometry changes; do not couple collision directly to every render mesh.
- [Dynamic prefab content survives its level] -> Instantiate level-local content under a root owned by that level scene and avoid parenting it under persistent objects.

## Migration Plan

1. Resolve the open gameplay decisions listed below and update this change before implementation.
2. Inventory existing scene objects and prototype scripts against the requirement contract without presuming compliance.
3. Establish the persistent scene and additive level-scene ownership boundary before migrating formal gameplay content.
4. Create formal collision proxies from art requirements and validate traversal, physics, and monster navigation independently of whitebox collision.
5. Plan implementation by shared behavior first: progress state, eligibility, doors, monster spaces, then level-specific setup.
6. Validate one level at a time, including death/reset behavior and prior-level retention, before linking the full route.
7. Do not unload prior-level content until the following level checkpoint is established, persistent references have been transferred or cleared, and cleanup is safe.

## Open Questions

- Where exactly will the Level 5 checkpoint be located?
- Will the medicine-cabinet area become a safe space after level layout playtesting, or remain fully exposed?
- What fixed camera composition will keep both players, the cabinet, exit, and monster threat legible?
- What is the final presentation after the Level 5 final door opens?
