## Purpose

Defines how Level 2 art is selected from the source scene and assembled without duplicating or modifying the established formal Level 1 content.

## ADDED Requirements

### Requirement: Source-preserving Level 2 selection
The project SHALL preserve `Assets/Scenes/Test/superbreadman 1.unity` as a reference scene while classifying its selected objects for formal Level 2 assembly. Objects already represented by Level 1's formal content SHALL be excluded from Level 2 assembly.

#### Scenario: Level 1 duplicate appears in the selected source content
- **WHEN** a selected source object is confirmed in the Level 1 source manifest or formal Level 1 content
- **THEN** it remains unchanged in the source scene and is not included in Level 2 formal content

### Requirement: Level 2 large-scale art assembly
The project SHALL assemble accepted Level 2 architecture and set dressing under a Level 2-owned content root in `FormalLevel02`. The assembly SHALL provide the spatial environment only and SHALL not imply gameplay interactions or level completion behavior.

#### Scenario: Accepted Level 2 art is assembled
- **WHEN** source content is confirmed as Level 2 environment or decoration
- **THEN** it is represented in the formal Level 2 content assembly under Level 2 ownership

### Requirement: Explicit unresolved-content handling
The project SHALL leave unconfirmed, later-level, prototype-only, and runtime-owned source content out of the Level 2 assembly and record the reason for exclusion.

#### Scenario: Selected content has no confirmed Level 2 role
- **WHEN** a selected source object cannot be attributed to Level 2 art assembly
- **THEN** it is not migrated and its exclusion is recorded for later level-scoped work
