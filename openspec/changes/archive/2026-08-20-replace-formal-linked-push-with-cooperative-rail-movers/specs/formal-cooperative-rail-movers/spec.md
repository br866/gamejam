## Purpose

Defines explicit two-actor cooperative movement for heavy formal-route objects without relying on free Rigidbody pushing or position-bound linked mode.

## ADDED Requirements

### Requirement: Cardinal cooperative engagement
A cooperative rail mover SHALL expose four cardinal direction groups. Each group SHALL contain an independently configurable Human point and Dog point GameObject. Each actor SHALL engage the group selected by the horizontal angle of the actor relative to the mover, and a mover SHALL remain locked until both actors have explicitly pressed the formal interaction control within their valid group ranges. The two actors SHALL NOT be required to occupy opposite sides.

#### Scenario: One actor is ready
- **WHEN** only one actor has engaged its assigned interaction node
- **THEN** the mover remains locked and does not move

#### Scenario: Both actors are ready
- **WHEN** the human and dog have each engaged their assigned interaction nodes
- **THEN** both actors attach to their nodes and the mover enters cooperative movement mode

### Requirement: Four-direction bounded movement
An engaged cooperative rail mover SHALL move along either of its two horizontal local axes, allowing movement toward all four cardinal directions. The active formal actor's movement input SHALL be projected onto the selected axis and the mover SHALL remain within its configured minimum and maximum travel limits on both axes.

#### Scenario: Move from a matching direction group
- **WHEN** an engaged mover receives movement input aligned with the axis selected by the shared direction group
- **THEN** it moves in either direction along that axis, both attached actors remain at their configured group points, and each actor uses push or pull animation according to the mover's movement relative to that actor

#### Scenario: Reach a travel limit
- **WHEN** an engaged mover reaches either configured travel limit on its selected axis
- **THEN** it stops at that limit and does not move beyond it

### Requirement: Position-matched animation
While an actor is attached, the actor SHALL face the mover and play a pushing animation when the mover moves away from that actor, or a pulling animation when the mover moves toward that actor. If a role's AnimatorController does not contain the requested named state, the actor SHALL use its configured locomotion fallback without throwing an error.

#### Scenario: Movement animation follows the actor side
- **WHEN** an engaged mover advances toward one attached actor and away from the other
- **THEN** the approaching actor plays pull, the receding actor plays push, and both face the mover

### Requirement: Engagement cancellation and reset
If either attached actor cancels, is interrupted, or a formal level reset occurs, the mover SHALL stop immediately, detach both actors, and return to its configured locked state. Level reset SHALL restore its initial position and rotation.

#### Scenario: Cancel engagement
- **WHEN** an attached actor presses the formal interaction control again
- **THEN** both actors detach and the mover locks at its current position

#### Scenario: Reset a mover
- **WHEN** the owning formal level resets during cooperative movement
- **THEN** both actors detach, mover velocity is zero, and the mover returns to its initial transform

### Requirement: Formal control separation
Formal-route Q input SHALL NOT position-bind the dog to the human. Q SHALL not be required to engage or move a cooperative rail mover.

#### Scenario: Toggle Q near a mover
- **WHEN** players press Q while near a locked cooperative rail mover
- **THEN** the mover remains locked until both actors explicitly engage their nodes
