## Purpose

Define how Formal Level 3 pressure plates complete and open doors, covering role eligibility, prerequisite completion modes, direct interior-door references, and shared-transition-door control that follows the conventions already used in Levels 1 and 2.

## ADDED Requirements

### Requirement: Plate role eligibility

Formal Level 3 plates SHALL support dog-only, human-only, and human-plus-dog-together completion modes through the existing trigger configuration. Existing configured plates SHALL keep their current behavior after this change.

#### Scenario: Dog-only plate rejects the human

- **WHEN** the human actor enters a dog-only plate alone
- **THEN** the plate does not complete

#### Scenario: Both-player plate requires simultaneous occupancy

- **WHEN** only one of the two actors occupies a both-player plate
- **THEN** the plate does not complete until both occupy it together

### Requirement: No prerequisite gating on plates

Plates SHALL complete immediately when an eligible actor satisfies their occupancy requirement, without any prerequisite list, prerequisite mode, or completion-state reporting. Legacy serialized prerequisite data SHALL be ignored by the component.

#### Scenario: Plate completes without any gating

- **WHEN** an eligible actor enters a plate whose actuators reference a door
- **THEN** the plate completes and opens that door regardless of any other plate states

### Requirement: Plates open interior doors by direct reference

Following the Level 1 and Level 2 convention, a completing plate SHALL open interior doors that are directly referenced in its actuator list within the same scene. A non-permanent plate SHALL close those doors again when the level resets; a permanent plate SHALL leave them open until an explicit route reset.

#### Scenario: Completing plate opens its wired interior door

- **WHEN** an eligible actor satisfies a plate whose actuator list references an interior door
- **THEN** the referenced door opens

#### Scenario: Level reset restores a non-permanent plate result

- **WHEN** the active level resets after a non-permanent plate opened its door
- **THEN** the door closes again and the plate becomes completable anew

### Requirement: Plate can open the shared transition door

A plate MAY declare that completing it also opens the shared boundary door toward the route successor. The opening SHALL go through the same route-flow mechanism used by the Level 1 key pickup and checkpoint handoff, resolved by name across loaded scenes. Existing checkpoint auto-open behavior SHALL remain unchanged.

#### Scenario: Completing plate opens the shared Level 3 / Level 4 door

- **WHEN** a plate with the transition option enabled completes inside Formal Level 3
- **THEN** the shared door between Level 3 and Level 4 opens permanently through the route flow controller

#### Scenario: Checkpoint handoff stays functional

- **WHEN** the player activates the Level 3 successor-registration checkpoint without pressing the transition plate
- **THEN** the existing automatic shared-door opening and successor loading still occur as before
