## Why

Level 4.5 temporarily forces dog-follow mode for the pursuit segment, but that restriction persists after entering Level 5. The pursuit monsters also stay locked to the human even when the dog is nearer, which does not match the intended cooperative threat.

## What Changes

- Restore normal human/dog character switching as soon as Level 5 is established at L05_Checkpoint.
- Make forced Level 4.5 pursuit continually target the nearer active player actor (human or dog), and retarget when their distance ordering changes.
- Preserve the existing 10-second forced-pursuit delay and L4.5 dog-follow behavior before Level 5.

## Capabilities

### New Capabilities

- `formal-level05-dog-control`: Restores player-controlled dog switching after L05_Checkpoint establishes Level 5.
- `formal-level045-dual-target-pursuit`: Defines nearest-player targeting for Level 4.5 forced monster pursuit.

### Modified Capabilities

- None.

## Impact

- `FormalGameFlowController`, `MonsterPatrol`, and focused edit/play-mode tests.
- No scene layout or Prefab asset changes are required.
