## Context

See `proposal.md` and `specs/superbreadman-final-level-requirements/spec.md`. Existing scripts and scenes are prototype material only; the final behavior must be verified against the captured requirements. This is a cross-cutting gameplay contract spanning checkpoint state, mechanisms, doors, physics objects, monster behavior, cameras, level scenes, and controlled escape flow.

## Goals / Non-Goals

**Goals:**
- Establish a single progress-lifetime model that prevents deaths from producing route soft locks.
- Make character eligibility and safe-space behavior explicit rather than accidental results of scene layout.
- Preserve physical-object puzzles as actual physics interactions rather than replacing them with completion triggers.
- Define the Level 5 escape as a bounded mode that can temporarily override standard dual-character controls.

**Non-Goals:**
- Treat the PDF as the sole authority or silently discard it.
- Assume existing scripts, scene objects, or previous OpenSpec planning are already correct.
- Choose the Level 5 checkpoint position, cabinet-area safety, or final completion presentation before those gameplay decisions are made.
- Implement this capture or prescribe exact class names, component layouts, asset choices, or navigation libraries.

## Decisions

### Separate temporary and permanent current-level progress

Keys and movable-object transforms reset on death; completed mechanisms, opened doors, and checkpoints remain. This lets physical puzzles be replayed when failed while preserving path unlocks that have already been earned.

Alternative considered: reset every object and door on death. Rejected because a player could respawn beyond a door that has reclosed, or be forced to repeat solved route gates inconsistently with the agreed behavior.

### Make doors own visual, physical, and progression state

A door stays physically closed until all configured requirements are complete, then plays an opening animation and becomes non-blocking for the remainder of the level. Door requirements are data-configured so one door can require one mechanism while another requires multiple.

Alternative considered: treat an animation-only visual object as the door. Rejected because its displayed state could diverge from collision and traversability.

### Enforce actor eligibility on the interaction

Keys, plates, switches, and cooperative actions enforce who is allowed to advance them. Scene placement remains important for level design but does not replace eligibility checks.

Alternative considered: rely on narrow routes or unreachable placement to restrict actors. Rejected because an unintended movement route could bypass the intended puzzle role.

### Separate line-of-sight obstruction from monster safety

Hiding geometry can block visible sight but does not break a real-time pursuit lock. A safe space is a stronger spatial boundary: monsters cannot enter or attack within it. This keeps hiding and safety vocabulary distinct even when both use physical geometry.

Alternative considered: let any line-of-sight obstruction cancel pursuit. Rejected because the agreed rule says monsters continuously pursue a locked player's real-time position.

### Use a controlled escape mode for the Level 5 corridor

The corridor temporarily disables switching and voluntary separation, applies a fixed camera, unlocks running, and keeps both monsters pursuing while players cooperatively move a physical cabinet. Entering the final room ends the corridor-specific control constraints because there is no later fifth-level segment requiring them.

Alternative considered: keep normal free dual-character controls during the corridor. Rejected because fixed camera, two-character separation, real-time pursuit, and physical pushing would compete for player attention and create conflicting control rules.

### Delay prior-level cleanup until the next checkpoint is established

Prior-level content remains valid while players have entered the next level but have not yet activated its checkpoint. After the next checkpoint is activated, cleanup occurs at a safe time when it cannot disrupt players, active monsters, or physics.

Alternative considered: destroy prior content on boundary entry. Rejected because a death before the next checkpoint could require state that was already removed.

## Risks / Trade-offs

- [A temporary key resets while a permanent door requires it] -> Ensure a door only becomes permanent after its key interaction has successfully completed, and reset key state without re-blocking an opened route.
- [Physics placement is unstable or bypassable] -> Validate movement, collision, jump clearance, reset positions, and player access for the first and fifth level physical puzzles.
- [Real-time pursuit makes a puzzle or cabinet push impossible] -> Tune level geometry and monster entry distance through playable evaluation; do not add an unagreed safe space as a workaround.
- [A safe space is visually reachable by a monster] -> Validate both physical and navigation access, plus attack radius, before treating it as safe.
- [Level cleanup destroys active objects] -> Gate cleanup on next-checkpoint establishment and a safe lifecycle boundary.
- [Fixed-camera escape hides essential information] -> Validate that players can read cabinet position, exit state, player positions, and approaching threat from the fixed view.

## Migration Plan

1. Resolve the open gameplay decisions listed below and update this change before implementation.
2. Inventory existing scene objects and prototype scripts against the requirement contract without presuming compliance.
3. Plan implementation by shared behavior first: progress state, eligibility, doors, monster spaces, then level-specific setup.
4. Validate one level at a time, including death/reset behavior, before linking the full route.
5. Do not retire legacy scene content until the following level checkpoint is established and cleanup is safe.

## Open Questions

- Where exactly will the Level 5 checkpoint be located?
- Will the medicine-cabinet area become a safe space after level layout playtesting, or remain fully exposed?
- What fixed camera composition will keep both players, the cabinet, exit, and monster threat legible?
- What is the final presentation after the Level 5 final door opens?
