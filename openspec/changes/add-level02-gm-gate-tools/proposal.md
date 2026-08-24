## Why

Level 2's cooperative door must be tested quickly and its missing prerequisite must be visible when it does not open. Testers also need a temporary way to move the dog through the level more quickly.

## What Changes

- Add a GM diagnostic command that reports the current Level 2 door-gate status and every missing prerequisite.
- Add a GM command on keypad 7 that toggles the dog's walk speed between its normal value and five times that value.
- Keep the commands scoped to runtime testing and avoid changing the intended cooperative door rules.

## Capabilities

### New Capabilities

- `formal-level02-gm-gate-tools`: Provides runtime GM visibility into the L2 cooperative door gate and a dog-speed testing toggle.

### Modified Capabilities

- None.

## Impact

- Formal player movement runtime and Level 2 cooperative door interaction diagnostics.
