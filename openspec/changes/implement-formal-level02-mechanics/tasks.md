## 1. Formal Scene Audit

- [ ] 1.1 Load `FormalLevel02` and inventory every retained monster, gate, footprint marker, pressure plate, checkpoint, collider, and navigation component by formal-scene identity.
- [ ] 1.2 Confirm the accepted Level 2 route, exit-side safe space, monster region, and route gate from world-space layout before configuring gameplay.
- [ ] 1.3 Move, replace, or remove retained prototype runtime components that cannot be configured without source-scene dependencies; keep `superbreadman 1.unity` unchanged.

## 2. Traversal And First Activation

- [ ] 2.1 Configure environment collision and player traversal across the approved Level 2 route, including the exit-side safe space.
- [ ] 2.2 Configure the six Level 2 footprint markers to remain hidden for the human and visible only while the dog is active.
- [ ] 2.3 Configure the first pressure plate to accept only the dog and advance the route without human activation.

## 3. Monster And Cooperative Progression

- [ ] 3.1 Configure the Level 2 monster patrol, chase bounds, navigation graph, and physical blockers so it cannot enter or capture players inside the safe space.
- [ ] 3.2 Configure the second pressure plate to require both player characters after the first plate has completed.
- [ ] 3.3 Connect cooperative second-plate completion to the correct route gate and ensure the opened state persists until level reset.

## 4. Checkpoint And Handoff

- [ ] 4.1 Configure the Level 2 checkpoint to activate only after cooperative route progress and to save both character positions.
- [ ] 4.2 Configure the Level 2 exit trigger and successor scene handoff to activate only after the cooperative route is complete.
- [ ] 4.3 Verify reset behavior from before the first plate, after the first plate, after the second plate, and after the checkpoint.

## 5. Validation

- [ ] 5.1 Run Unity EditMode checks for configured component references and missing/null gameplay dependencies.
- [ ] 5.2 Run Unity PlayMode verification for dog-only footprints and first plate, safe-space monster exclusion, cooperative second plate, gate persistence, checkpoint reset, and successor handoff.
- [ ] 5.3 Inspect the Unity Console, verify source-scene preservation, and update the Level 2 manifest with final formal-scene identities and validation evidence.
