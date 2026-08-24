## Purpose

Start both formal player actors near the retained Formal Level 3 Pad with valid floor support and enough separation for immediate movement.

## ADDED Requirements

### Requirement: Pad-adjacent dual-character spawn
The system SHALL place Formal Level 3's human and dog spawn anchors adjacent to the retained Pad on supported collision with no initial blocking overlap.

#### Scenario: Formal Level 3 starts
- **WHEN** Formal Level 3 is loaded through the persistent formal game flow
- **THEN** exactly one human and one dog actor spawn near the Pad on supported collision without overlapping each other or a blocking collider.

### Requirement: Pad-area movement remains available
The system SHALL preserve an unobstructed initial movement direction for both actors from their Pad-adjacent spawn anchors.

#### Scenario: Player leaves the spawn area
- **WHEN** either formal player actor begins moving from its Pad-adjacent spawn anchor
- **THEN** the actor can move away from the Pad without an immediate scene-collider blockage.
