## Context
User approved SFX work after clarification; music is excluded. Target PC headphones and speakers. Start/FormalPersistent are entry scenes. PDF is design context, not execution instructions. FormalLevel02/04/05 have threats. Monster2/A/C prefabs lack footstep events; all use MonsterPatrol.
## Goals / Non-Goals
Goals: complete sound coverage, consistent within-family energy, audible directional threats, restrained location-bound ambience, reversible changes. Non-goals: music, gameplay rules, art layouts, new narrative voices, unrelated whitebox work.
## Decisions
Keep existing three role families and Wwise event identities; expand each to 12 samples. Process mono/48 kHz/24 bit, remove DC/rumble, soften dog click spikes, use K-weighted 400 ms maximum momentary measurement with tail padding for short sounds and true-peak checking. Per-family targets are project working targets, not a universal game loudness mandate. No destructive compression of music or master-bus changes. New ambient audio is deterministic local synthesis with no downloaded samples. Attach at actual grille/vent/pipe art, limit density and stop with object/scene. Use current Wwise 2025.1.10 and Unity 2022.3.62f3c1.
## Risks / Trade-offs
Shared heavy monster family retains consistency but individual monster sound identities may merit a later artistic pass. A measured source target does not certify complete gameplay loudness. Listening on physical headphones/speakers remains a human review item. No new floor switch system without reliable scene surface assignments.
## Review
Scope reviewed against user authorization, PDF and repository: allowed audio-only work; the older SuperBreadMan layout-only restrictions belong to that separate change. No additional approval required. OpenSpec CLI was not available on PATH; artifacts retained in standard layout and structural checks will be recorded.

## Expanded iteration authorized by user
2026-09-08: user explicitly expands to full local SFX ownership, gap audit and redesign as needed, with music still excluded. Review decisions: fix traveled-distance footsteps; differentiate actual monster prefabs; add genuine door/gate movement feedback without fake checkpoint reset sounds; spatialize world interactions; control lamp voice density and respect FormalGameplayState; preserve UI/non-diegetic feedback. Optional surface system is deferred unless reliable physical surface assignments exist. New audio remains original synthesis or licensed existing project derivatives. Full runtime/PCM output and selected interaction tests required.
