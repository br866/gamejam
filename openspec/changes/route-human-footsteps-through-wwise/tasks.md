# Tasks: route-human-footsteps-through-wwise

## 1. Runtime Integration

- [x] 1.1 Add a serialized `AK.Wwise.Event` reference and distance cadence to `FormalPlayerActor`.
- [x] 1.2 Post the event from the grounded formal Human actor.
- [x] 1.3 Preserve the formal Dog actor and legacy `PlayerController` footstep paths.
- [x] 1.4 Warn once when the Human event reference is missing instead of spamming once per step.
- [x] 1.5 Match Walk and Run footstep cadence to their imported animation cycle lengths and current movement speeds.

## 2. Unity Hookup and Verification

- [x] 2.1 Assign `Play_Footstep_Human` to the Human Footstep Event field on `FormalHumanActor`.
- [x] 2.2 Add `AkGameObj` to `FormalHumanActor` and confirm the generated AutoBank exists.
- [ ] 2.3 Verify Human movement produces one positional Wwise footstep per existing distance interval.
- [ ] 2.4 Verify Dog footsteps remain unchanged.
