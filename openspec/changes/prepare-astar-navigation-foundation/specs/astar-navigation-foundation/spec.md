## Purpose

Provides an isolated, reversible way to confirm that the selected A* package can be imported and used for basic obstacle navigation before any production game systems depend on it.

## ADDED Requirements

### Requirement: Preserve default package import structure

The package SHALL be imported from `D:\MyDownload2\A Pathfinding Project Pro 5.4.6.unitypackage` using Unity's normal package import flow and SHALL remain in the package's default import locations. Imported runtime code, Editor tools, documentation, examples, demo scenes, assets, and associated metadata SHALL be preserved.

#### Scenario: Import the package

- **WHEN** the package is imported into the Unity project
- **THEN** Unity places its contents using the package's default paths and the project retains all imported package content

#### Scenario: Package content is reviewed

- **WHEN** the imported package contains examples, documentation, or Editor-only content
- **THEN** those contents remain available and are not moved or deleted during this change

### Requirement: Isolate and retain the click-movement test scene

The change SHALL create and retain a standalone test scene for A* validation. The test scene SHALL not modify or require the SuperBreadMan whitebox scene, art scene, legacy game scenes, or production gameplay objects.

#### Scenario: Create the isolated test

- **WHEN** the A* click-movement test is prepared
- **THEN** it is created in an isolated temporary scene with only minimal test geometry, navigation setup, an agent, and a target

#### Scenario: Production scenes remain unchanged

- **WHEN** the smoke test is run or configured
- **THEN** the two SuperBreadMan target scenes and their gameplay objects remain unchanged

#### Scenario: Test scene remains available

- **WHEN** the test scene has been configured and navigation data has been baked or scanned
- **THEN** the scene remains in the project for manual testing and future navigation experiments

### Requirement: Demonstrate click-driven obstacle navigation

The test scene SHALL demonstrate that an independent test agent can receive a clicked target and reach it by navigating around a simple obstacle in Play Mode. Navigation data SHALL be baked or scanned before manual testing. The test SHALL record whether the package, graph setup, click target input, agent movement, and target reachability work together.

#### Scenario: Agent routes to a clicked target

- **WHEN** the test agent is placed on one side of a blocking obstacle and the user clicks a target on the other side
- **THEN** the agent reaches the target by a path that does not pass through the obstacle

### Requirement: Demonstrate dynamic obstacle updates

The retained test scene SHALL provide a user-triggered way to move the existing test obstacle at runtime. When the obstacle changes position, the navigation data and the test agent's route SHALL update without rescanning or modifying a production scene.

#### Scenario: Move the obstacle during Play Mode

- **WHEN** the user triggers the test obstacle's runtime position change
- **THEN** the obstacle updates its navigation obstruction and the agent recalculates a route to its current destination

#### Scenario: Smoke test fails

- **WHEN** the agent cannot be configured, cannot calculate a path, or cannot reach the target
- **THEN** the failure is recorded with the failing stage and no production scene is changed to compensate

### Requirement: Temporary MCP session handling

The test scene setup SHALL use Unity MCP port `8086` only for the setup and validation session. After the test scene is ready for manual testing and the setup result is recorded, the temporary MCP connection SHALL be closed. The retained test scene SHALL not depend on the MCP connection at runtime.

#### Scenario: Operate through MCP

- **WHEN** the smoke-test scene is created or inspected through Unity MCP
- **THEN** the connection uses port `8086` and is treated as temporary test infrastructure

#### Scenario: Close setup connection after handoff

- **WHEN** the test scene is ready for the user to test manually
- **THEN** the MCP session on port `8086` is closed and the test scene remains available

### Requirement: Defer production navigation integration

This change SHALL NOT create a production navigation graph, scan a SuperBreadMan scene, replace monster movement, modify player movement, implement dog following, or change collision and navigation layers.

#### Scenario: A production integration opportunity is found

- **WHEN** the smoke test suggests a graph type or agent configuration for production
- **THEN** the result is recorded as follow-up design input and no production integration is performed in this change
