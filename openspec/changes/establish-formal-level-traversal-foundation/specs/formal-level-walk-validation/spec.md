## Purpose

Defines repeatable physics and walkability validation for human and dog actors between verified entrance, checkpoint, and exit anchors in formal levels.

## ADDED Requirements

### Requirement: Supported anchor placement
Every formal entrance and checkpoint anchor SHALL provide non-trigger ground support for both player actor capsules and SHALL not overlap a blocking collider when the actor is placed there.

#### Scenario: Validate an anchor
- **WHEN** a human or dog actor is placed at a formal entrance or checkpoint anchor
- **THEN** the actor is grounded, non-overlapping with blockers, and remains supported after physics simulation

### Requirement: Walkable route segments
Each approved route segment between a formal entrance, checkpoint, or exit anchor SHALL provide a traversable path for the roles intended to use that segment. Blocking environment colliders SHALL prevent passage through walls, furniture, and closed gates, while trigger volumes SHALL not physically block the actors.

#### Scenario: Traverse an approved segment
- **WHEN** an intended player actor moves from a verified anchor toward the next approved route anchor
- **THEN** the actor can complete the segment without falling out of the level or passing through a blocker

#### Scenario: Reach a trigger on a route
- **WHEN** an intended player actor enters a route trigger volume
- **THEN** the actor can enter and leave the trigger without physical blocking

### Requirement: Traversal validation evidence
Formal levels under active development SHALL record their validated anchor positions, route segments, and collision exceptions so later gameplay work can use them as stable test starting points.

#### Scenario: Prepare subsequent gameplay work
- **WHEN** a later level-specific mechanism change begins
- **THEN** it can identify the validated entrance or checkpoint anchor and route segment required for its play-mode verification
