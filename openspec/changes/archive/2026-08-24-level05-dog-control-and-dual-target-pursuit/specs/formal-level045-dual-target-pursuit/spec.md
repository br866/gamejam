## Purpose

Makes the retained Level 4 pursuit threat respond to the nearer member of the human-and-dog pair throughout Level 4.5.

## ADDED Requirements

### Requirement: Forced pursuit targets the nearest active player actor
The system SHALL, after the existing delayed Level 4.5 pursuit begins, select the nearer active human or dog actor as the forced-chase target for each pursuit monster.

#### Scenario: Dog is nearer to a pursuing monster
- **WHEN** the dog is closer to a forced-pursuit monster than the human
- **THEN** that monster chases and attacks the dog

#### Scenario: Human becomes nearer during pursuit
- **WHEN** the human becomes closer than the dog to a forced-pursuit monster
- **THEN** that monster retargets the human without waiting for the forced pursuit to end

#### Scenario: Both players remain valid targets
- **WHEN** both the human and dog are active during forced pursuit
- **THEN** a monster continues evaluating both actors instead of permanently locking to its first target
