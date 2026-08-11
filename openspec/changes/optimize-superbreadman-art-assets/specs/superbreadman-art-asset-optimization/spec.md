## Purpose

Defines a safe, measurable workflow for reducing SuperBreadMan runtime art-asset size and Unity memory use while preserving recoverable high-quality source assets.

## ADDED Requirements

### Requirement: Source assets remain recoverable outside Unity imports
The project SHALL retain an unmodified high-quality source counterpart for every approved optimized runtime asset under `Art/BackUp`, using the runtime asset's path relative to `UnityProject/Assets`. `Art/BackUp` SHALL remain outside Unity's `Assets` import root and SHALL NOT be ordinarily tracked in Git; team-shared source-asset versioning SHALL use Git LFS if it is later required.

#### Scenario: A runtime texture is approved for replacement
- **WHEN** an approved runtime texture is optimized
- **THEN** its original high-quality file exists at the corresponding `Art/BackUp` path before the runtime file is replaced

#### Scenario: Unity scans project assets
- **WHEN** Unity refreshes the project
- **THEN** assets under `Art/BackUp` are not imported as Unity assets

### Requirement: Runtime asset replacement preserves references
The project SHALL replace approved optimized textures and models at their existing paths and retain their existing `.meta` files. Replacement SHALL preserve the asset GUID used by Unity scenes, prefabs, materials, and other serialized references.

#### Scenario: An approved texture is replaced
- **WHEN** an optimized texture replaces its original runtime file
- **THEN** the file path and `.meta` GUID remain unchanged and its existing material reference resolves

#### Scenario: An approved static model is replaced
- **WHEN** an optimized static model replaces its original runtime file
- **THEN** the file path and `.meta` GUID remain unchanged and existing scene or prefab references resolve

### Requirement: Assets receive risk-appropriate optimization
The project SHALL classify each candidate as a texture set, static model, or skinned/animated model before replacement. Texture budgets SHALL be assigned by visual importance and maximum on-screen use. Static models SHALL use an approved triangle budget. Skinned or animated models SHALL be individually reviewed and validated rather than included in automatic bulk replacement.

#### Scenario: A regular static scene prop is prepared
- **WHEN** a static model is selected for optimization
- **THEN** its assigned texture and triangle budgets are recorded before an optimized version is produced

#### Scenario: A character model is selected
- **WHEN** a model has a skeleton, animation, or blend shapes
- **THEN** it is excluded from automatic bulk replacement and receives individual compatibility validation

### Requirement: Optimization batches are measurable and reversible
The project SHALL maintain a manifest for every optimization batch that identifies source and runtime paths, asset classification, pre- and post-optimization file sizes, target texture or triangle budgets, and validation outcome. An accepted batch SHALL demonstrate that no intended Unity references are missing and SHALL remain recoverable by restoring its corresponding `Art/BackUp` files.

#### Scenario: A batch is proposed
- **WHEN** a group of assets is prepared for replacement
- **THEN** a manifest records every candidate and its planned budget before replacements occur

#### Scenario: A batch is validated
- **WHEN** Unity imports the optimized batch
- **THEN** the recorded validation confirms the intended materials, prefabs, scenes, and relevant animations contain no missing references

#### Scenario: An optimized asset regresses
- **WHEN** an accepted optimized asset causes a visible or reference regression
- **THEN** its original source asset can be restored from `Art/BackUp` at the same runtime path while retaining the existing `.meta` file

### Requirement: PC texture imports limit runtime memory
For approved runtime textures, the project SHALL configure Unity Standalone import settings to match the approved resolution budget, use mipmaps for world-space textures, and preserve the correct texture interpretation: color textures as sRGB, normal maps as Normal Map data, and metallic, roughness, emission, and mask textures as linear data where their shaders require it.

#### Scenario: A world-space color texture is imported for PC
- **WHEN** Unity imports an approved world-space color texture for Standalone
- **THEN** its importer uses the approved maximum resolution, mipmaps, and sRGB interpretation

#### Scenario: A normal map is imported for PC
- **WHEN** Unity imports an approved normal texture for Standalone
- **THEN** its importer identifies it as a Normal Map and preserves its normal-data interpretation

### Requirement: Optimization does not alter active gameplay or rendering contracts
The optimization pass SHALL NOT modify runtime scripts, scene object layouts or transforms, colliders, navigation, materials, shaders, lighting, or the active SuperBreadMan route and whitebox changes. It SHALL exclude UI textures, TextMesh Pro assets, and unapproved asset roots from bulk optimization.

#### Scenario: A candidate falls outside the approved art scope
- **WHEN** a potential optimization would require a scene, script, material, shader, UI, collision, or navigation change
- **THEN** it is recorded as a follow-up and excluded from the batch
