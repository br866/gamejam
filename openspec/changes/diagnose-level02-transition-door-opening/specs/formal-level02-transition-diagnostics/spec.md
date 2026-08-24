## Purpose

Provides Console evidence that identifies the runtime source and call path that opens the Formal Level 2 to Level 3 shared transition door.

## ADDED Requirements

### Requirement: Attributable Level 2 transition diagnostics

When a Level 2 completion source requests progression or the registered Level 2 to Level 3 transition door changes to open, the runtime SHALL emit a diagnostic record that identifies the active level, source object or operation, target door, and call path.

#### Scenario: Completion source requests advancement
- **WHEN** any Level 2 runtime completion source requests route advancement
- **THEN** the Unity Console records the active route level, source context, and call path before the transition is performed

#### Scenario: Shared door opens
- **WHEN** the registered Level 2 to Level 3 shared transition door is opened
- **THEN** the Unity Console records the door identity, its scene, and the call path that caused the open operation

### Requirement: Direct Level 2 startup remains valid

The diagnostic capability SHALL support reproducing the issue when the persistent formal route is configured to start directly at Formal Level 2, without changing the configured initial scene or route behavior.

#### Scenario: Boot directly into Level 2
- **WHEN** the persistent formal route starts at Formal Level 2
- **THEN** diagnostics remain available and the route starts with its existing configured behavior unchanged
