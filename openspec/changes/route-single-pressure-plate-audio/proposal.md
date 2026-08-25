# Proposal: route-single-pressure-plate-audio

## Why

The authored Wwise Event `Play_PressurePlate` is present in generated
SoundBanks, but formal Pedal completion currently has no Unity audio hook.
Successful single-plate interactions need synchronized feedback without also
scoring the Level 3 multi-plate chain or non-visual trigger zones.

## What Changes

- Add an optional Wwise completion Event to `FormalActuatorTrigger`.
- Post it only when the associated `FormalActuatorTrigger` changes from
  incomplete to complete, so rejected actors and partial cooperative occupancy
  stay silent.
- Configure `Play_PressurePlate` on the shared Pedal prefab and the Level 2
  cooperative single-plate trigger.
- Disable completion audio on all five Pedal instances in the Formal Level 3
  multi-plate chain.

## Impact

- One Unity Wwise Event reference asset.
- One localized runtime script change.
- Shared Pedal prefab configuration, the Level 2 cooperative trigger, and five
  Level 3 prefab-instance overrides.
- No Wwise Work Unit, media, or generated SoundBank edits.
