## 1. Attachment velocity handling

- [x] 1.1 Clear the attaching actor's horizontal Rigidbody velocity before the initial crate interaction-point snap.
- [x] 1.2 Update mover-point synchronization to preserve only vertical Rigidbody velocity while an actor is attached.

## 2. Verification

- [x] 2.1 Verify in FormalLevel01 that a stationary engaged crate leaves FormalHumanActor with zero X/Z velocity and stable Z position.
- [x] 2.2 Verify existing crate pushing, pulling, release, gravity, and grounding behavior remain functional.
