# Proposal: enable-level03-pressure-plate-audio

## Why

Formal Level 3's five physical Pedal instances now use the same successful
press feedback contract as the other visible pressure plates. Their completion
already drives the shared press animation, so successful presses should also
post the authored `Play_PressurePlate` Event.

## What Changes

- Enable the existing completion-audio hook on all five physical Pedal
  instances in `L03_Content.prefab`.
- Preserve actor eligibility, actuator wiring, reset policy, animation logic,
  and Wwise authoring.
- Audit every formal pressure-plate completion source for agreement between
  visible press animation and completion audio; report exceptions without
  changing them.

## Impact

- Five existing prefab-instance overrides in `L03_Content.prefab`.
- No runtime-script, Wwise Work Unit, media, or SoundBank changes.

