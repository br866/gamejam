## MODIFIED Requirements

### Requirement: Formal level gameplay coverage
The route SHALL provide the approved playable progression for every formal level: Level 1 cooperative rail movement of the wooden crate, human key, mechanism door, exit door, and checkpoint; Level 2 dog-guided plate, cooperative plate, monster safe route, checkpoint, and exit; Level 3 central cooperative plate, ordered dog route, final door, checkpoint, and exit; Level 4 split-role route, reunion plate, two monsters, checkpoint, and exit; Level 4.5 checkpoint and exit; and the Level 5 controlled escape and final-room sequence.

#### Scenario: Complete a formal level through its intended mechanics
- **WHEN** players satisfy a level's approved role, cooperative, ordered, or physical-puzzle requirements
- **THEN** its configured checkpoint and exit become reachable without a debug jump

#### Scenario: Complete the Level 1 cooperative crate step
- **WHEN** the human and dog engage the wooden crate's opposite interaction nodes and move it along its configured rail into its usable placement
- **THEN** the crate forms the physical step for the human-only key route while the dog remains unable to collect the key
