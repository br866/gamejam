## Purpose

Ensure character model rigs match their animation clips' rig type so state-driven animations visibly deform skeletons instead of silently freezing them.

## ADDED Requirements

### Requirement: Humanoid clips require humanoid-compatible avatars
Every Animator that plays Humanoid muscle-space animation clips SHALL reference a model whose imported rig type is Humanoid and whose generated avatar maps the clip's muscle set. A Generic or missing avatar SHALL NOT be used with Humanoid clips.

#### Scenario: Player plays walk animation
- **WHEN** the human player actor transitions into its walking state
- **THEN** the visible skeleton deforms (bone world positions change over time) while the animator state advances.

#### Scenario: Monster plays chase animation
- **WHEN** the L02 monster switches from patrol to chase movement
- **THEN** its skeleton deforms according to the running clip rather than holding a static pose.

### Requirement: Rig mismatch fails loudly at validation time
The project SHALL provide an editor-side check that reports any Animator whose controller contains Humanoid clips but whose avatar is not humanoid, so rig mismatches are detected without entering play mode.

#### Scenario: Editor audit detects a frozen-rig configuration
- **WHEN** the rig audit is executed in the editor
- **THEN** any animator pairing Humanoid clips with a non-humanoid avatar is listed with its object path.

### Requirement: Existing generic pipelines keep working
Character setups that already animate correctly with Generic bone-path clips (the dog) SHALL NOT be converted or broken by the rig alignment work.

#### Scenario: Dog keeps animating after alignment
- **WHEN** the rig alignment changes are applied and the dog actor idles or moves
- **THEN** the dog's skeleton continues to deform as before the change.
