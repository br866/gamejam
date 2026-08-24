## Purpose

Defines fast formal-route navigation and explicit additive shared-content ownership so levels can load, unload, and transition without removing art still needed by another route level.

## ADDED Requirements

### Requirement: Formal route catalog
The formal runtime SHALL use one ordered route catalog that maps every formal level identifier to its playable scene and its required shared additive content scenes. Each catalog entry SHALL use a unique level identifier and scene name.

#### Scenario: Resolving a catalog level
- **WHEN** formal runtime navigation receives a registered level identifier
- **THEN** it resolves the corresponding playable scene and declared shared content scenes from the route catalog

#### Scenario: Rejecting an unknown level
- **WHEN** formal runtime navigation receives a level identifier that is not registered
- **THEN** it reports that the level is unknown and does not change loaded scene ownership

### Requirement: Direct level loading and unloading APIs
The formal runtime SHALL provide synchronous and asynchronous APIs to load and unload a registered formal level by identifier. The asynchronous APIs SHALL report completion only after their scene operation and shared-content ownership reconciliation have finished.

#### Scenario: Synchronous level load
- **WHEN** a caller synchronously loads a registered formal level
- **THEN** its playable scene and required shared content are loaded before the call returns

#### Scenario: Asynchronous level load
- **WHEN** a caller asynchronously loads a registered formal level
- **THEN** the completion callback reports success only after its playable scene and required shared content are loaded

#### Scenario: Asynchronous level unload
- **WHEN** a caller asynchronously unloads a loaded registered formal level
- **THEN** the completion callback reports success only after the playable scene unloads and no longer-needed shared content is reconciled

### Requirement: Route navigation APIs and GM commands
The formal runtime SHALL provide next-level, previous-level, and direct-jump navigation based on route catalog order. Development builds and the Unity Inspector SHALL expose equivalent GM operations for loading, unloading, reloading, moving next, moving previous, and jumping to configured levels.

#### Scenario: Moving to the next level
- **WHEN** a caller or GM command requests the next level from a non-final registered level
- **THEN** the runtime starts transition to the immediately following catalog entry

#### Scenario: Moving to the previous level
- **WHEN** a caller or GM command requests the previous level from a non-initial registered level
- **THEN** the runtime loads the immediately preceding catalog entry and moves the shared players to its entrance anchors

#### Scenario: Navigating beyond a route boundary
- **WHEN** a caller requests next from the final level or previous from the initial level
- **THEN** the runtime reports that no adjacent level exists and preserves the current loaded route state

### Requirement: Shared additive content retention
The formal runtime SHALL retain a declared shared additive content scene while at least one loaded or transitional formal level references it. Unloading a formal level SHALL release only that level's reference and SHALL unload shared content only after no retained route level references it.

#### Scenario: Shared art across adjacent levels
- **WHEN** two retained formal route levels both declare the same shared additive content scene
- **THEN** that content remains loaded while either level remains retained

#### Scenario: Releasing last shared-art reference
- **WHEN** the final retained formal route level that references a shared additive content scene is unloaded
- **THEN** the runtime unloads that shared content scene

#### Scenario: Persistent art exclusion
- **WHEN** global art is owned by the persistent formal scene rather than a route-catalog shared content entry
- **THEN** route-level unload operations do not unload that persistent art
