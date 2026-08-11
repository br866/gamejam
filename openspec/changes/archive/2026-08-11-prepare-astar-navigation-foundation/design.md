## Context

See `proposal.md` for motivation and scope. The project currently contains direct Rigidbody-based player movement and direct `MoveTowards` monster movement. The selected A* package is available at `D:\MyDownload2\A Pathfinding Project Pro 5.4.6.unitypackage`; `Assets/ResSource` is not the package destination for this change.

## Goals / Non-Goals

**Goals:**
- Prove the package can be imported without path or compilation issues.
- Prove a minimal agent can navigate around a blocking obstacle in isolation.
- Leave a reusable click-movement test scene for manual testing and future navigation experiments.
- Close the temporary MCP session after the scene is handed off for manual testing.

**Non-Goals:**
- Select or configure the production graph for SuperBreadMan.
- Change any existing game scene, script, player motor, monster state machine, dog-follow behavior, collision, layer, or asset.

## Decisions

### Keep the package's default import paths

Unity's normal import flow is preferred over manual extraction or relocation. This preserves package metadata, Editor/runtime assembly relationships, and future upgrade compatibility. `Assets/ResSource` remains unrelated to this package import.

### Preserve all package content

Examples and documentation are useful for evaluating package capabilities and version-specific setup. Cleanup would add risk and remove evidence before the project has chosen a production integration strategy, so all imported content remains.

### Use a minimal isolated click-movement test

The retained test scene contains only simple geometry, the package's required navigation setup, one independent test agent, a click-to-target input path, and one target. A small deterministic scene answers the useful question, “Can this package calculate and execute a basic click-driven route here?” without conflating package problems with the existing hospital scenes.

```text
Agent  ─── blocked direct route ─── Obstacle ─── Target
                    │
                    └── expected A* detour
```

The exact graph type is intentionally not selected as a production decision. The test uses the smallest package-supported setup that can demonstrate click-driven obstacle navigation, and the selected setup is recorded as test evidence.

### Keep agent geometry explicit and test dynamic obstruction

The Agent's Capsule visual and CharacterController use the same radius and height, with the controller center aligned so its bottom rests on the ground plane. The existing central test obstacle is the only dynamic obstacle test subject. A user-triggered position toggle moves that object in Play Mode; the package's dynamic obstacle support is responsible for refreshing the affected graph area, and the agent then repaths to its current destination.

### Keep MCP temporary

Unity MCP port `8086` is an operational channel, not a project dependency. It is used for creating and inspecting the test scene, then closed after the scene is ready for manual testing. No project code or permanent configuration hardcodes the port.

## Risks / Trade-offs

- [Package import path or license/setup issue] -> Stop before production work and record the exact Unity error or missing prerequisite.
- [Test passes but production geometry is unsuitable] -> Treat the result as package viability only; defer production graph and layer design to a later change.
- [Dynamic obstacle behavior differs in production] -> Treat this as dynamic-update viability only; evaluate production update cadence and obstacle bounds later.
- [Imported examples add project noise] -> Preserve them for this change and decide on cleanup only in a separate documented change.
- [MCP session cannot be closed cleanly] -> Record the session state and close it through the supported Unity/MCP control path; do not alter unrelated project files.

## Migration Plan

1. Import the package through Unity's normal package import flow.
2. Create and configure the isolated click-movement test scene through MCP `8086`.
3. Bake or scan the test scene's navigation data.
4. Record package compilation, setup, and obstacle-navigation results for handoff.
5. Close the temporary MCP connection while retaining the test scene and imported package content.

Rollback consists of removing only the retained test scene and closing the MCP session if the user later requests cleanup. Package removal is not part of this change and requires a separate decision because it may affect generated metadata and project compilation.
