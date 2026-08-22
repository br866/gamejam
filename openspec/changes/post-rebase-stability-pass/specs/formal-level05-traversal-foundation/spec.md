## ADDED Requirements

### Requirement: Level 5 interior physical separation
The system SHALL provide enabled non-trigger interior walls that physically separate the Level 5 escape corridor from the final hall, and the final hall's right-room and left-room halves, with doorway gaps only where the left-room door and final door are placed.

#### Scenario: Doors gate real passages
- **WHEN** the left-room door or final door is closed
- **THEN** players cannot walk from one separated area to the next except by passing through the open door gap.

#### Scenario: Monster navigation respects new walls
- **WHEN** the L05 monsters path after the walls are added
- **THEN** their navigation areas remain connected (no sealed pockets) or their patrol areas are adjusted so they never freeze on unreachable paths.
