# Delta: formal-route-advance-protocol

## Purpose

Defines one authoritative behavior for "finish this level, go to the next": every gameplay source requests route advancement through a single entry point, the successor is derived only from the route catalog, requests made during scene operations are never lost, transition doors are resolved through a registered name per route edge, and per-edge policies govern predecessor retention, arrival sequences, and arrival cleanup.

## ADDED Requirements

### Requirement: Single route-advance entry point

All gameplay completion sources (key pickup, actuator plate, exit checkpoint, crate door trigger) SHALL request route advancement through one flow-controller operation instead of opening shared doors or loading scenes directly. The successor level SHALL be determined exclusively by the route catalog position of the currently active level; serialized successor hints on gameplay objects MUST NOT influence which level loads or which door opens.

#### Scenario: Key pickup advances from Level01

- **WHEN** the human collects the Level01 key
- **THEN** the shared Level01→Level02 transition door opens permanently and FormalLevel02 loads additively

#### Scenario: Stale successor hint is ignored

- **WHEN** a gameplay object carries an outdated serialized successor scene reference that disagrees with the route catalog
- **THEN** the catalog successor of the current level is loaded anyway

#### Scenario: Crate trigger advances from Level04.5

- **WHEN** a pushable crate enters the Level04.5 crate door trigger
- **THEN** the local crate door opens and the same advance operation opens the registered Level04.5→Level05 shared door and loads FormalLevel05

### Requirement: Advance requests are never silently dropped

If an advance is requested while a scene load or unload operation is in progress, the flow controller SHALL retain exactly one pending request (with its originating level) and execute it after the running operation finishes. Repeated requests SHALL collapse into one execution. A pending request whose originating level is no longer the active level when drained SHALL be discarded rather than executed.

#### Scenario: Request during a running load executes afterwards

- **WHEN** a completion source requests advance while a scene operation is still running
- **THEN** the door opens and the successor loads once that operation completes, with no further player input

#### Scenario: Repeated requests execute once

- **WHEN** two or more sources request advance while busy and the drained edge matches
- **THEN** the successor transition happens exactly once

#### Scenario: Stale pending request is discarded

- **WHEN** the pending request originates from a level that is no longer the active level at drain time
- **THEN** the request is dropped without loading any additional scene

### Requirement: Registered transition-door lookup

The transition door for a route edge SHALL be resolved by matching the registered door-name token stored in the route catalog against doors inside the shared-art scenes common to both levels. If the registered token cannot be resolved, the flow controller SHALL log a warning and continue without disabling gameplay (no exception, no wrong door).

#### Scenario: Token resolves the intended door per edge

- **WHEN** an advance runs on each route edge
- **THEN** exactly the registered door for that edge opens (e.g., token `ToLevel02` on the Level01→Level02 edge)

#### Scenario: Unresolvable token degrades safely

- **WHEN** a registered token matches no door in the loaded shared-art scenes
- **THEN** a warning is logged, no door changes state, and no error interrupts play

### Requirement: Edge policy governs predecessor retention

Route advancement SHALL consult a per-edge retention policy: on edges marked as retaining the predecessor (currently the Level04→Level04.5 edge), the predecessor level stays loaded across arrival; on all other forward edges, arrival keeps only the new level and its direct predecessor, closes the interior doors of the previous two levels, and unloads older route levels.

#### Scenario: Entering Level04.5 retains Level04

- **WHEN** the route advances into FormalLevel045
- **THEN** FormalLevel04 remains loaded (its hostile actors included) and its interior doors are not force-closed

#### Scenario: Entering Level03 retires older levels

- **WHEN** the route advances into FormalLevel03
- **THEN** the direct predecessor stays loaded, the grand-predecessor level unloads, and interior doors of the previous two levels close

### Requirement: Arrival sequence triggers from the level checkpoint

For a level whose edge declares an arrival-sequence policy (currently Level04.5), activating that level's checkpoint SHALL start the sequence: player control restricts to the human actor with the companion orbiting them immediately, and hostile actors begin a forced chase after a fixed delay. Checkpoints in levels without the policy SHALL NOT start any sequence. Resetting such a level SHALL restore hostile patrol behavior and then restart the sequence.

#### Scenario: Touching the Level04.5 save point starts the chase clock

- **WHEN** a player activates the Level04.5 checkpoint
- **THEN** control becomes human-only with the dog orbiting, and after the fixed delay all retained hostiles switch to forced chase

#### Scenario: Ordinary checkpoints do not trigger sequences

- **WHEN** a checkpoint activates in a level without an arrival-sequence policy
- **THEN** only respawn anchoring occurs

#### Scenario: Reset replays the arrival sequence

- **WHEN** the current level is reset while its arrival-sequence policy is active
- **THEN** retained hostiles return to patrol first, then the sequence restarts

### Requirement: Final-level arrival cleans up retained predecessors

When the route advances into the final level (FormalLevel05), the flow controller SHALL unload every older route level including the retained predecessor-of-predecessor (FormalLevel04), removing any hostile actors living in it, while keeping the final level and its direct predecessor loaded.

#### Scenario: Reaching Level05 removes Level04 and its monsters

- **WHEN** FormalLevel05 becomes the active level via a normal advance
- **THEN** FormalLevel04 unloads together with its hostile actors, and FormalLevel05 plus FormalLevel045 remain loaded

### Requirement: GM travel uses the same protocol

Developer fast-travel keys SHALL mirror gameplay semantics: the next-level key performs the identical advance operation (open registered transition door, then load); the previous-level key returns to the earlier level and places players at that level's spawn point; the dedicated Level04.5 debug assembly and its supporting helpers are removed entirely.

#### Scenario: Next-level key matches gameplay advance

- **WHEN** the developer presses the next-level fast-travel key
- **THEN** the resulting door state and loaded-scene set are identical to a gameplay-triggered advance on the same edge

#### Scenario: Previous-level key lands players at spawn

- **WHEN** the developer presses the previous-level fast-travel key
- **THEN** players are placed at the target level's spawn point instead of remaining at their world coordinates

#### Scenario: Legacy debug assembly no longer exists

- **WHEN** the developer attempts the removed Level04.5 debug jump input
- **THEN** no scene assembly occurs because the input binding and its implementation are gone
