## Purpose

Make the user's selected Level 4.5 visual route available in a standalone formal scene with source-faithful global placement.

## ADDED Requirements

### Requirement: User-selected Level 4.5 visual assembly
The system SHALL copy every Renderer from the current Level 4.5 user selection except prototype player-system renderers into FormalLevel045 with world Position, Rotation, and Lossy Scale preserved.

#### Scenario: Formal Level 4.5 loads
- **WHEN** FormalLevel045 is opened
- **THEN** copied selected visuals render at their source world placement.

### Requirement: Visual-only Level 4.5 content
The system SHALL remove prototype scripts, colliders, rigidbodies, audio, navigation, player objects, triggers, and mechanics from copied Level 4.5 visual content.

#### Scenario: Copied Level 4.5 content is inspected
- **WHEN** a copied visual object is inspected
- **THEN** it retains only components needed for its visual rendering.
