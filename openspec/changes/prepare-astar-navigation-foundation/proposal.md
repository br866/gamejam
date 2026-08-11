## Why

The current movement systems use direct physics and straight-line movement, which cannot establish whether A* Pathfinding Project Pro is a suitable foundation for future monster navigation. The package must be evaluated in isolation before it is introduced to the SuperBreadMan scenes, player controls, or monster behavior.

## What Changes

- Import `A Pathfinding Project Pro 5.4.6.unitypackage` from `D:\MyDownload2` using its default Unity import paths.
- Preserve every file imported by the package, including runtime code, Editor tools, documentation, examples, and assets.
- Create and retain an isolated A* test scene that proves a minimal independent agent can navigate around an obstacle to a clicked target.
- Use Unity MCP on port `8086` only for the temporary smoke-test session.
- Bake or scan the test scene's navigation data before handing it to the user for manual testing.
- Verify that an existing dynamic obstacle updates the test navigation route when moved.
- Record the package and smoke-test setup; close the temporary MCP connection after the test scene is ready for manual testing.
- Defer graph selection for production scenes, SuperBreadMan scene changes, monster behavior integration, player movement, dog following, collision changes, and navigation-layer design.

## Capabilities

### New Capabilities
- `astar-navigation-foundation`: Safely imports and independently smoke-tests the A* Pathfinding Project Pro package without changing game behavior.

### Modified Capabilities

- None.

## Impact

- Adds third-party A* Pathfinding Project Pro content under the package's default Unity import locations.
- Creates and removes an isolated temporary Unity test scene.
- Uses a temporary Unity MCP connection on port `8086` for test operation only.
- Does not modify `Assets/Scenes/Test/superbreadman.unity`, `Assets/Scenes/Test/superbreadman 1.unity`, game scripts, player behavior, monster behavior, collision, or production navigation configuration.
