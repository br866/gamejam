## Purpose

Provide a reliable collision and navigation foundation for SuperBreadMan's whitebox so both playable characters and monsters can traverse the intended level route without physics conflicts or obstacle clipping.

## ADDED Requirements

### Requirement: Whitebox collision classification

The whitebox SHALL classify existing gameplay geometry into walkable ground, static navigation blockers, dynamic navigation blockers, actor bodies, interaction triggers, or navigation-ignored objects. Classification SHALL allow actors to collide with intended physical surfaces while triggers remain detectable without becoming ground or navigation obstacles.

Every MeshCollider in `Assets/MoMing/Scenes/Test/superbreadman.unity` SHALL be replaced with a BoxCollider based on its source mesh bounds. The conversion SHALL preserve the Collider's enabled state, trigger state, physics material, and contact offset, and SHALL NOT modify source Prefab assets or any other scene.

#### Scenario: Actor crosses walkable ground
- **WHEN** the human or dog moves across intended whitebox ground
- **THEN** the actor remains supported and does not collide with non-gameplay presentation objects as ground

#### Scenario: Actor approaches a blocking object
- **WHEN** the human or dog moves into an intended wall, closed gate, or physical obstacle
- **THEN** the actor is blocked without passing through the object

#### Scenario: Actor enters an interaction volume
- **WHEN** an actor enters an existing interaction trigger
- **THEN** the interaction remains detectable and the trigger does not become an unintended physical or navigation obstacle

#### Scenario: Convert whitebox colliders
- **WHEN** the scene-specific whitebox collider conversion is run
- **THEN** it replaces MeshCollider components only in `Assets/MoMing/Scenes/Test/superbreadman.unity` and does not edit a Prefab asset or another scene

### Requirement: Stable playable-character movement

The whitebox SHALL move each active playable character through one consistent physics movement path. Character following, switching, and linked-mode movement SHALL preserve collision behavior and SHALL NOT mix direct transform position updates with Rigidbody-controlled motion for the same actor.

Ground detection SHALL filter gameplay ground from triggers and non-ground objects. Existing character switching, linked movement, interaction controls, and checkpoint recovery behavior SHALL remain available.

#### Scenario: Move an active character against an obstacle
- **WHEN** the player moves an active character into an intended blocking object
- **THEN** the character remains physically blocked without position jitter, tunneling, or collider bypass

#### Scenario: Follow the active character
- **WHEN** the inactive linked character follows the active character
- **THEN** it moves through the same collision-consistent path and does not teleport through intended blockers

#### Scenario: Switch playable characters
- **WHEN** the player uses the existing character-switch control while both characters are valid
- **THEN** control transfers and the newly active character can move, ground, and collide normally

### Requirement: Player-controlled level connections

Each door connecting whitebox levels SHALL remain controlled by its existing or later-assigned gameplay controller. A player character, including the dog, SHALL only cross the connected level boundary after that controller permits passage.

#### Scenario: Dog reaches a locked level door
- **WHEN** the dog attempts to cross a level connection whose door controller is closed
- **THEN** the door blocks passage into the next level

#### Scenario: Dog reaches an unlocked level door
- **WHEN** the dog attempts to cross a level connection whose door controller is open
- **THEN** the dog can enter the connected level through that door

### Requirement: Whitebox navigable space

The whitebox SHALL provide a navigation representation that covers the intended walkable route from the level start through the final exit and excludes static blocking geometry. The representation SHALL be generated or refreshed when the whitebox starts so scene edits are reflected without relying on stale baked data.

The navigable space SHALL respect the verified collision footprint of each navigated actor. Routes that do not provide sufficient clearance for an actor SHALL not be presented as traversable for that actor.

#### Scenario: Find a route through the main path
- **WHEN** a navigated actor is assigned a reachable destination on the intended whitebox route
- **THEN** it receives a route that remains on walkable space and avoids static blockers

#### Scenario: Reject an enclosed destination
- **WHEN** a navigated actor is assigned a destination isolated by static blockers or insufficient clearance
- **THEN** it does not pass through blocking geometry and reports or remains in a non-arrived state

### Requirement: Dynamic obstacle replanning

The whitebox navigation representation SHALL reflect existing dynamic physical obstacles while they are active. A navigated actor approaching a newly blocking object SHALL re-evaluate its route and either detour through available walkable space or remain blocked without crossing the obstacle.

#### Scenario: Detour around a moved obstacle
- **WHEN** an existing dynamic physical obstacle moves into an actor's current route and an alternate route exists
- **THEN** the actor replans and reaches its destination without crossing the obstacle

#### Scenario: Stop at a fully blocked route
- **WHEN** an existing dynamic physical obstacle removes all routes to the destination
- **THEN** the actor remains outside the obstacle and does not report successful arrival

### Requirement: Level-bounded monster behavior

Monsters in the whitebox SHALL use navigable space for patrol and chase movement within their assigned level only. A monster SHALL preserve its existing room-boundary, detection, capture, reset, and audio behavior while avoiding static and dynamic obstacles during movement.

A monster SHALL NOT cross a door or level boundary, including when the door is open for the player. A monster that cannot reach its current patrol or chase destination within its assigned level SHALL remain valid for later state updates and SHALL NOT move through colliders to force arrival.

#### Scenario: Patrol around a static blocker
- **WHEN** a monster's patrol destination requires navigating around an intended static blocker
- **THEN** the monster follows a valid route without crossing the blocker

#### Scenario: Player crosses to another level
- **WHEN** a detected player crosses an unlocked door into a different level
- **THEN** the monster remains in its assigned level and does not use the door to continue the chase

#### Scenario: Chase target becomes unreachable
- **WHEN** a detected player is separated from a monster by blocking geometry or a level boundary with no valid route in the monster's assigned level
- **THEN** the monster does not pass through the geometry and continues to evaluate its existing patrol or chase state normally

### Requirement: Whitebox-only delivery boundary

This change SHALL implement collider conversion only in `Assets/MoMing/Scenes/Test/superbreadman.unity` and provide an implementation-ready interaction and navigation design. Whitebox object Layer assignment remains the scene owner's responsibility. This change SHALL NOT alter the art scene, Prefab assets, route sequencing, UI, audio assets, models, materials, lighting, or unrelated documentation content.

#### Scenario: Review the paired art scene
- **WHEN** this change is reviewed after whitebox verification
- **THEN** `Assets/Scenes/Test/superbreadman 1.unity` remains unchanged for a later alignment change
