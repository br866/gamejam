## Purpose

Defines editor-time validation that detects missing formal-level scene contract elements and unsafe player entry setup before a route scene is accepted.

## ADDED Requirements

### Requirement: Contract validation coverage
Editor validation SHALL inspect every registered formal playable level and report missing required formal-level contract elements, duplicate formal player actors, and missing collision support at player entrance anchors.

#### Scenario: Validating registered formal levels
- **WHEN** the formal-level validation suite is run
- **THEN** every registered formal playable level is checked without requiring manual scene-by-scene inspection

#### Scenario: Missing player support
- **WHEN** a human or dog entrance anchor lacks supporting non-trigger collision geometry
- **THEN** validation fails and identifies the affected scene and anchor

### Requirement: Entry-space safety validation
Editor validation SHALL report an entrance anchor whose player capsule overlaps non-trigger collision geometry and SHALL preserve the existing separate-anchor and checkpoint-anchor support checks.

#### Scenario: Blocked entrance
- **WHEN** a formal player entrance anchor overlaps a wall or other non-trigger blocking collider
- **THEN** validation fails and identifies the blocked entrance anchor

#### Scenario: Valid separate entrances
- **WHEN** both player entrance anchors have supporting collision and clear capsule space
- **THEN** validation accepts the entrance setup without requiring the human and dog to share an anchor
