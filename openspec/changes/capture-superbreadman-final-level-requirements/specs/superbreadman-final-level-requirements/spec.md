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

- Level 1 SHALL require cooperative movement of movable objects to form a usable physical step, human-only entry through the broken-wall route, human-only key collection, a configured mechanism-controlled room door, a permanent key-controlled exit door, and a checkpoint.
- Level 2 SHALL include a monster, dog-only visibility of a static footprint route to the first plate, a dog-only first plate, a monster-inaccessible exit-side safe space, a two-character second plate, and a checkpoint.
- Level 3 SHALL include an inspectable completion hint, a central two-character plate, dog-only ordered plates one through four that open the successive route, a permanent final door, and a checkpoint.
- Level 4 SHALL include two monsters, dog-only first-plate activation from a left safe area, separate dog and human routes, human-only second-plate activation, a reunion, a two-character third plate, a permanent exit door, and a checkpoint.
- Level 5 SHALL include the controlled escape and final-room behavior defined separately in this specification.

#### Scenario: Complete Level 1 physical puzzle
- **WHEN** the human and dog cooperatively move the small stool and box from their starting area to the broken-wall area
- **THEN** the moved objects provide a physical route that lets the human, but not the dog, reach the key area

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
