## Purpose

Ensures the formal dog visibly and physically settles at rest, while forced following stays aligned with its configured walking pace.

## ADDED Requirements

### Requirement: Dog decelerates into idle after movement input ends
When the player provides no directional movement input while directly controlling the dog, the system SHALL reduce the dog’s horizontal movement speed smoothly until it is at rest. The dog SHALL play its Idle animation after reaching rest, and SHALL play its Walk animation while it retains meaningful horizontal movement.

#### Scenario: Player releases all movement input while controlling the dog
- **WHEN** the dog is moving and the player releases all directional movement input
- **THEN** the dog decelerates to a stop and transitions from Walk to Idle after it has stopped

#### Scenario: Dog is idle with no movement input
- **WHEN** the dog is already at rest and the player provides no directional movement input
- **THEN** the dog remains at rest and plays Idle

### Requirement: Forced follow uses a configurable multiple of dog walk speed
The system SHALL move the dog during forced follow at the dog actor’s configured walk speed multiplied by a configurable follow-speed multiplier. The multiplier SHALL default to 1.3.

#### Scenario: Dog follow begins with a customized walk speed
- **WHEN** forced follow is enabled for a dog whose configured walk speed differs from the default and the multiplier retains its default value
- **THEN** the dog follows at 1.3 times that configured walk speed

#### Scenario: Follow-speed multiplier is customized
- **WHEN** forced follow is enabled with a configured follow-speed multiplier
- **THEN** the dog follows at its configured walk speed multiplied by that multiplier

### Requirement: Forced follow reflects actual dog movement in animation
The system SHALL play the dog Walk animation while forced follow produces meaningful movement and SHALL play Idle when forced follow has no meaningful movement to perform.

#### Scenario: Dog reaches its forced-follow destination
- **WHEN** the dog reaches the current forced-follow destination and no further movement is required
- **THEN** the dog remains stationary and plays Idle
