# Tasks: route-dog-footsteps-through-wwise

## 1. Runtime Integration

- [x] 1.1 Add a serialized Dog Wwise Event and Dog walk cadence to `FormalPlayerActor`.
- [x] 1.2 Select the posted Event and distance threshold by actor role.
- [x] 1.3 Preserve Wwise-authored random sample selection.
- [x] 1.4 Warn once when the role-specific Event is missing.

## 2. Unity Hookup and Verification

- [x] 2.1 Create the Unity Wwise reference for the existing `Play_Footstep_Dog` Event.
- [x] 2.2 Assign the Event and add `AkGameObj` to `FormalDogActor`.
- [ ] 2.3 Validate the change artifacts and C# compilation.
- [ ] 2.4 Verify Dog movement produces positional Wwise footsteps at the animation-matched interval in Play mode.
