## Purpose

Defines the shared mechanics and per-level gameplay outcomes required for every stage of the approved formal route to be completable through player actions.

## ADDED Requirements

### Requirement: Reusable cooperative route mechanics
The formal route SHALL support role-restricted interactions, both-player occupancy, ordered progression, prerequisite-gated doors and exits, temporary versus permanent level progress, and deterministic reset behavior without relying on prototype player tags or debug commands.

#### Scenario: Reject an ineligible actor
- **WHEN** a non-eligible formal actor enters a role-restricted interaction
- **THEN** the interaction does not advance its progression state

#### Scenario: Satisfy a cooperative prerequisite
- **WHEN** a mechanism requires both formal actors and both occupy its configured interaction area
- **THEN** the mechanism completes once and its configured route result becomes available

#### Scenario: Reset route progress
- **WHEN** the active formal level resets after temporary and permanent progress have changed
- **THEN** temporary objects and resettable mechanisms return to their initial state while completed permanent mechanisms, opened permanent doors, and active checkpoints remain complete

### Requirement: Bounded monster safety
The formal route SHALL constrain every configured monster to its hostile region, prevent it from entering configured safe spaces, and prevent capture of a player inside a configured safe space. A pursuit already acquired outside a safe space SHALL continue against the player's real-time position until the player reaches a safe space or the monster is otherwise reset.

#### Scenario: Enter a safe space during pursuit
- **WHEN** a pursued player enters a configured safe space
- **THEN** the monster does not enter or capture the player within that safe space

#### Scenario: Pursue behind ordinary cover
- **WHEN** a pursued player moves behind ordinary visual cover outside a safe space
- **THEN** the monster continues pursuit and can capture the player if it reaches them

### Requirement: Formal level gameplay coverage
The route SHALL provide the approved playable progression for every formal level: Level 1 crate, human key, mechanism door, exit door, and checkpoint; Level 2 dog-guided plate, cooperative plate, monster safe route, checkpoint, and exit; Level 3 central cooperative plate, ordered dog route, final door, checkpoint, and exit; Level 4 split-role route, reunion plate, two monsters, checkpoint, and exit; Level 4.5 checkpoint and exit; and the Level 5 controlled escape and final-room sequence.

#### Scenario: Complete a formal level through its intended mechanics
- **WHEN** players satisfy a level's approved role, cooperative, ordered, or physical-puzzle requirements
- **THEN** its configured checkpoint and exit become reachable without a debug jump

### Requirement: Controlled Level 5 escape
The Level 5 escape corridor SHALL temporarily disable character switching and voluntary separation, use a fixed readable camera view, allow the escape running ability, and keep the configured monsters pursuing while players cooperatively move the cabinet to clear the corridor exit. Resetting during the escape SHALL restore the corridor's temporary state at the Level 5 checkpoint.

#### Scenario: Enter controlled escape
- **WHEN** players enter the configured Level 5 escape corridor
- **THEN** the controlled escape restrictions and camera apply until the players leave the corridor

#### Scenario: Fail the escape
- **WHEN** either player is caught during the Level 5 controlled escape
- **THEN** both players return to the Level 5 checkpoint and the escape cabinet, monsters, door state, and control mode reset consistently

#### Scenario: Complete the final room
- **WHEN** both players complete the configured right-room and then left-room cooperative plates
- **THEN** the final door opens permanently and final route completion becomes available
