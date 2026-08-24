## Purpose

Provide repeatable physical checks proving that Formal Level 3 art has usable floor support, boundaries, blocking collision, and baseline traversal before level-specific mechanics are implemented.

## ADDED Requirements

### Requirement: Level 3 floor and blocker coverage
Formal Level 3 SHALL provide non-trigger collision for its floor surfaces, outer boundaries, walls, and substantial fixed props needed by the approved baseline route.

#### Scenario: Player reaches a Level 3 boundary or wall
- **WHEN** a formal player capsule moves into the boundary or a fixed blocking object
- **THEN** the capsule is blocked instead of passing through or falling out of the level.

### Requirement: Both actors pass baseline route checks
Formal Level 3 SHALL validate human and dog capsule grounding, overlap, and route clearance from entrance to checkpoint and checkpoint to provisional exit.

#### Scenario: Baseline route is checked
- **WHEN** the human and dog capsule checks are run on each approved segment
- **THEN** both actors remain supported and no route segment is blocked by unintended collision.
