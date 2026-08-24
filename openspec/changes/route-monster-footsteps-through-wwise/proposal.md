# Proposal: route-monster-footsteps-through-wwise

## Why

The three formal-route monsters use different visual models but share `MonsterPatrol`, and none currently plays footsteps. The authored Wwise event `Play_Footstep_Brutedoc` is already generated, so all current monster variants need one shared positional playback path.

## What Changes

- Add a serialized Wwise Event reference and distance-based footstep cadence to `MonsterPatrol`.
- Use separate patrol and chase stride distances while posting the same `Play_Footstep_Brutedoc` event.
- Assign the Event to the one FormalLevel02 monster, both FormalLevel04 monsters, and the reusable Enemy Monster prefab.
- Reset cadence after a level reset or reposition so teleports do not produce false steps.

## Impact

- Runtime code changes in `UnityProject/Assets/MoMing/Scripts/Enemy/MonsterPatrol.cs`.
- Narrow serialized hookups in FormalLevel02, FormalLevel04, and `Prefabs/Enemies/Monster.prefab`.
- No Wwise authoring or monster movement changes.
