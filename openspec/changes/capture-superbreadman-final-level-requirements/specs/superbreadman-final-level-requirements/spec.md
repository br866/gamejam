## Purpose

Defines the agreed five-level gameplay contract so later scene and runtime work shares one validated behavior target rather than inferring requirements from prototypes.

## ADDED Requirements

### Requirement: Requirements-source reconciliation
The project SHALL treat `Docs/关卡计划.pdf`, decisions captured by this change, playable evaluation, and available project assets as inputs to final level requirements. The PDF SHALL NOT be treated as the sole or highest-priority source, and existing runtime code SHALL NOT be treated as evidence that a requirement is satisfied.

#### Scenario: Reconcile a requirement source conflict
- **WHEN** the PDF, an agreed decision, the playable scene, or existing code disagree about a level behavior
- **THEN** the conflict is resolved explicitly before implementation acceptance instead of assuming the existing code is authoritative

### Requirement: Per-level checkpoint and progress lifetime
Every level SHALL provide a checkpoint that either the human or dog can activate independently. Activating the checkpoint SHALL establish the current level respawn position for both characters.

Keys and movable-object positions SHALL be temporary progress and SHALL reset after a death. Completed mechanisms, opened doors, and activated checkpoints SHALL be permanent progress for the current level lifetime and SHALL remain complete after a death.

#### Scenario: Either character activates a checkpoint
- **WHEN** either the human or dog enters the current level checkpoint
- **THEN** subsequent deaths respawn both characters at that checkpoint

#### Scenario: Death resets temporary but not permanent progress
- **WHEN** a player dies after collecting a key, moving an object, completing a mechanism, opening a door, and activating a checkpoint
- **THEN** the key and movable object reset while the completed mechanism, opened door, and checkpoint remain active

### Requirement: Formal level composition and ownership
Whitebox scenes SHALL remain prototype validation material and SHALL NOT be treated as the authority for formal art placement or collision. The formal route SHALL use one persistent Unity scene for players, camera, UI, audio, and global flow, plus separately additive-loaded Unity scenes for each playable level.

Each formal level scene SHALL own its level controller, spawn points, checkpoints, level-local gameplay, and a level-content root. Reusable room, corridor, architectural segment, interactive-object, and gameplay prefabs SHALL be instantiated or arranged under the owning level scene. Decorative mesh fragments SHALL remain within their owning prefab unless they have an independent placement, interaction, reuse, or lifecycle requirement.

#### Scenario: Load a new level before its checkpoint
- **WHEN** players cross from the current level into a subsequent level
- **THEN** the subsequent level scene loads additively while the current level scene remains loaded until the subsequent level checkpoint is activated

#### Scenario: Release a prior level after checkpoint handoff
- **WHEN** players activate the subsequent level checkpoint
- **THEN** the project clears or replaces persistent references to prior-level objects and may unload the prior level scene with all of its level-local content

### Requirement: Art-aligned collision proxies
Formal collision SHALL be authored against intended art boundaries, rather than copied from whitebox collision or automatically applied to every render mesh. Static architectural content SHALL use simple or compound primitive colliders where they preserve required traversal; static non-convex mesh colliders SHALL be limited to irregular surfaces where those proxies cannot do so.

Dynamic Rigidbody gameplay objects SHALL use primitive or compound colliders and SHALL NOT rely on non-convex mesh colliders. Checkpoints, interaction triggers, safe spaces, monster restrictions, and other gameplay boundaries SHALL use explicit stable colliders. Monster navigation SHALL be authored or baked against the formal collision arrangement.

#### Scenario: Art geometry changes after formal collision is authored
- **WHEN** a formal art prefab changes in a way that affects visible traversable or blocked space
- **THEN** its collision proxy and dependent navigation are reviewed and updated as needed before the level is accepted

#### Scenario: Unload a level with instantiated content
- **WHEN** a level scene is unloaded after its successor checkpoint is active
- **THEN** its level-local prefab instances, collision proxies, gameplay objects, and controller are released with that scene while players, camera, UI, audio, and global flow remain available

### Requirement: Level-scoped door lifecycle
A closed door SHALL physically block passage. When all of its configured required mechanisms are permanently completed, the door SHALL play its opening animation, remove its blocking physics, and remain open for the current level lifetime.

A door SHALL support one or more configured required mechanisms; completion of all configured mechanisms SHALL be required before it opens. A previously opened door SHALL not close because of player death.

After players enter a subsequent level and establish that level's checkpoint, the project SHALL be allowed to close, unload, or destroy prior-level doors and content at a safe time.

#### Scenario: A door with multiple required mechanisms
- **WHEN** a door is configured with two required mechanisms and only one is completed
- **THEN** the door remains closed and physically blocks passage

#### Scenario: Opened door survives a death
- **WHEN** all configured mechanisms open a door and a player subsequently dies in the same level
- **THEN** the door remains visually open and non-blocking after respawn

### Requirement: Hard character eligibility
Character-specific interactions SHALL enforce eligibility as a hard rule rather than relying on map layout. Only the human SHALL collect keys, the dog SHALL not collect or trigger a key, and character-specific mechanisms SHALL reject non-eligible characters.

Two-character mechanisms SHALL require both human and dog according to their configured condition. Cooperative physical pushing SHALL require the configured two-character cooperative state.

#### Scenario: Dog contacts a key
- **WHEN** the dog contacts or attempts to interact with a key
- **THEN** the key remains uncollected

#### Scenario: Non-eligible character enters a mechanism
- **WHEN** a character that is not eligible for a character-specific mechanism enters its interaction area
- **THEN** that character does not advance the mechanism

### Requirement: Monster visibility, pursuit, and safety
Ordinary hiding locations SHALL only provide visual obstruction. They SHALL NOT stop a monster that has locked onto a player from pursuing that player's real-time position, and they SHALL NOT prevent monster attacks.

A safe space SHALL be physically inaccessible to monsters. A monster SHALL not enter or attack a player within a safe space.

#### Scenario: Player hides behind an obstruction after detection
- **WHEN** a monster has locked onto a player and the player moves behind a line-of-sight obstruction
- **THEN** the monster continues pursuing the player's real-time position and can still attack if it reaches the player

#### Scenario: Player enters a safe space
- **WHEN** a pursued player enters a space inaccessible to the monster
- **THEN** the monster does not enter that space or attack the player there

### Requirement: Five-level route coverage
The intended route SHALL cover five level stages.

- Level 1 SHALL require the human to push one movable wooden crate into a usable physical step beneath the broken-wall route. The human-only route SHALL lead to human-only key collection, a configured mechanism-controlled room door, a permanent key-controlled exit door, and a checkpoint. Selected stool assets SHALL remain non-interactive scenery with physical blocking collision; the dog SHALL NOT be required to move the crate in this first formal version.
- Level 2 SHALL include a monster, dog-only visibility of a static footprint route to the first plate, a dog-only first plate, a monster-inaccessible exit-side safe space, a two-character second plate, and a checkpoint.
- Level 3 SHALL include an inspectable completion hint, a central two-character plate, dog-only ordered plates one through four that open the successive route, a permanent final door, and a checkpoint.
- Level 4 SHALL include two monsters, dog-only first-plate activation from a left safe area, separate dog and human routes, human-only second-plate activation, a reunion, a two-character third plate, a permanent exit door, and a checkpoint.
- Level 5 SHALL include the controlled escape and final-room behavior defined separately in this specification.

#### Scenario: Complete Level 1 physical puzzle
- **WHEN** the human pushes the movable wooden crate into its usable placement area beneath the broken-wall route
- **THEN** the crate provides a physical step that lets the human, but not the dog, reach the key area

#### Scenario: Dog follows the Level 2 footprint route
- **WHEN** the dog is the active character in Level 2
- **THEN** only the dog can see the static footprint route leading to the first plate

#### Scenario: Complete the Level 3 ordered route
- **WHEN** the dog activates ordered plates one, two, three, and four in that order after the central plate is complete
- **THEN** each successive route opens and the permanent Level 3 exit door opens after the fourth plate

#### Scenario: Complete the Level 4 split route
- **WHEN** the dog completes the first plate, the human completes the second plate, and both characters reunite at the third plate
- **THEN** the Level 4 exit door opens permanently

### Requirement: Controlled Level 5 escape
The Level 5 corridor SHALL be a controlled escape stage. On entering the stage, character switching and voluntary separation SHALL be disabled, the camera SHALL use the corridor's fixed view, and the Level 5 running ability SHALL become available. The two Level 4 monsters SHALL pursue the players' real-time positions through the corridor.

The corridor exit SHALL require cooperative physical pushing of the medicine cabinet while monster pursuit continues. A Level 5 checkpoint SHALL exist and deaths during the stage SHALL respawn players at that checkpoint with the escape stage reset. The cabinet area SHALL NOT be assumed to be a safe space until separately decided.

After the players leave the corridor, the final square room SHALL require a two-character right-room plate to permanently open the left-room door, followed by a two-character left-room plate to permanently open the final door.

#### Scenario: Enter the controlled corridor escape
- **WHEN** players enter the Level 5 corridor
- **THEN** they cannot switch characters or voluntarily separate, see the fixed corridor camera view, and can use the Level 5 running ability

#### Scenario: Escape failure resets the stage
- **WHEN** a monster catches either player during the Level 5 escape stage
- **THEN** both players respawn at the Level 5 checkpoint and the monsters, cabinet, exit door, and controlled escape state reset

#### Scenario: Escape through the cabinet route
- **WHEN** the two players cooperatively move the medicine cabinet far enough to clear the corridor exit while monsters continue pursuit
- **THEN** the exit becomes traversable and the players can enter the final square room

#### Scenario: Complete the Level 5 final room
- **WHEN** both characters complete the right-room plate and then the left-room plate
- **THEN** the left-room door and then the final door open permanently in that order
