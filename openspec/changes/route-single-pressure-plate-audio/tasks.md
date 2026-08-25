# Tasks: route-single-pressure-plate-audio

## 1. Runtime Playback

- [x] 1.1 Add optional successful-completion audio to `FormalActuatorTrigger`.
- [x] 1.2 Ensure rejected actors and incomplete cooperative occupancy stay silent.
- [x] 1.3 Allow individual Pedal instances to opt out.

## 2. Unity Hookup

- [x] 2.1 Create the Unity Wwise reference for `Play_PressurePlate`.
- [x] 2.2 Configure the shared Pedal prefab and Level 2 cooperative single plate.
- [x] 2.3 Disable the Event on all five Formal Level 3 multi-plate instances.

## 3. Verification

- [x] 3.1 Verify Event GUID, ShortID, generated banks, and serialized references.
- [x] 3.2 Run static diff checks and compile the Unity runtime assembly.
- [ ] 3.3 Verify representative single plates and the excluded Level 3 chain in Play mode.
