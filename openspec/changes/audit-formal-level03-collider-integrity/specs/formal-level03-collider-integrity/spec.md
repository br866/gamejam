## Purpose

Keep Formal Level 3 collision physically useful by removing invalid or inappropriate proxy volumes while retaining valid environmental boundaries and traversal support.

## ADDED Requirements

### Requirement: Valid Level 3 collider coverage
The system SHALL retain only enabled non-trigger blocking colliders whose dimensions and placement represent intended traversable environment boundaries or substantial fixed obstacles.

#### Scenario: Collider integrity audit completes
- **WHEN** Formal Level 3 collider coverage is inspected
- **THEN** invalid, duplicate, visual-only, or unsupported blocking proxy volumes are removed or disabled while valid floor, boundary, wall, door, and fixed-prop coverage remains.

### Requirement: Level 3 entry remains clear
The system SHALL preserve supported, non-overlapping human and dog Pad-adjacent spawn anchors with clear initial movement directions after collider correction.

#### Scenario: Formal Level 3 starts after audit
- **WHEN** FormalPersistent loads FormalLevel03
- **THEN** the human and dog actors spawn near the Pad without initial blocking overlap and can begin moving away from it.
