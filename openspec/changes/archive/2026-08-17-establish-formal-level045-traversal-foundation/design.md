## Context

FormalLevel045 is currently art-only. The selected doorway at x approximately `-125.7` is a west corridor boundary visual. The Level 4.5 corridor occupies approximately x `-205..-126`, z `-8..8`, with source floor near y `9.8`.

## Goals / Non-Goals

**Goals:**
- Include the selected missing door and jamb visuals at their source world transforms.
- Establish scene-owned floor, boundaries, and separate respawn anchors.

**Non-Goals:**
- Add detailed prop collision, gameplay interactions, checkpoint triggers, or exit behavior.
- Add player objects directly to FormalLevel045.

## Decisions

### Keep collision separate from art

The formal scene owns a broad floor and boundaries under a collision root. L045_Content remains visual-only, preserving its global-transform validation and allowing later detailed collision work without changing art.

### Place respawn near the west doorway inside the corridor

The anchors are placed just east of the west boundary door, separated horizontally and validated against player capsules.
