## Purpose

Makes FormalLevel04 monster navigation react to door open/close state so monsters can chase players through opened doors, get cut off by closed doors instantly, and never freeze when a path is unavailable — while keeping every other level's behavior untouched behind an opt-in flag.

## ADDED Requirements

### Requirement: Door state drives monster path connectivity
A monster whose navigation has dynamic door navigation enabled SHALL treat the grid region occupied by a tracked door as walkable exactly when that door reports itself open, and unwalkable when it reports closed.

#### Scenario: Opened door becomes passable
- **WHEN** a tracked door transitions from closed to open and a player stands beyond it
- **THEN** a chasing monster paths through the doorway instead of stopping at it

#### Scenario: Closed door becomes impassable
- **WHEN** a tracked door transitions from open to closed
- **THEN** new monster paths do not cross the doorway region

### Requirement: Close blocks routing immediately
When a tracked door is commanded to close, monster routing SHALL exclude its doorway region on the same frame, without waiting for the door's closing animation or physics collider to re-enable.

#### Scenario: Mid-animation closure cuts the path
- **WHEN** a door begins its closing animation while a monster is approaching from the far side
- **THEN** the monster's active route stops crossing the doorway before the animation completes

### Requirement: Monster pushed out of newly blocked ground
If a monster occupies grid nodes inside a doorway region at the moment it becomes unwalkable, the monster SHALL slide to the nearest walkable node within roughly 0.2 seconds instead of standing inside the blocked region.

#### Scenario: Door closes with monster in the doorway
- **WHEN** a door closes while the monster stands inside its region
- **THEN** the monster ends up outside the doorway region within about 0.2 seconds

### Requirement: Failed paths never freeze monsters
Monster navigation SHALL recover from pathfinding failures by re-attempting after its repath interval rather than stalling permanently.

#### Scenario: Chase target unreachable
- **WHEN** no path exists between a chasing monster and its target
- **THEN** the monster abandons straight pursuit, resumes moving toward patrol waypoints, and keeps re-attempting pursuit while its chase state persists

### Requirement: Scripted forced pursuit uses navigation
The Level4.5 delayed all-monster pursuit SHALL move monsters along valid navigation routes, honoring walls and door connectivity, instead of straight-line movement through obstacles.

#### Scenario: Pursuit across the L4 to L4.5 route
- **WHEN** the delayed forced pursuit activates and players have opened a connecting door
- **THEN** monsters run along walkable routes toward the player and do not clip through closed doors or walls

### Requirement: Dynamic connectivity is opt-in per monster
Dynamic door navigation SHALL only affect monsters whose navigation component explicitly enables it; monsters without the flag, and all levels using default components, SHALL retain today's static-grid behavior with no observable change.

#### Scenario: Flag disabled monster ignores doors
- **WHEN** a door toggles near a monster whose flag is off
- **THEN** that monster's pathing behavior is unchanged

### Requirement: Editor-only GM test harness
An isolated editor test scene SHALL provide developer commands to toggle every scene door, order all monsters to pathfind to a clicked ground point, and trigger forced pursuit, without referencing or modifying production scenes.

#### Scenario: GM toggles a door
- **WHEN** the developer presses the key mapped to a listed door in the test scene
- **THEN** that door opens or closes through the same public entry points used by real mechanisms

#### Scenario: GM orders group movement
- **WHEN** the developer right-clicks a ground point in the test scene
- **THEN** all flagged monsters attempt to pathfind to that point using their live nav graph
