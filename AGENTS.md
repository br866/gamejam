# Agent Guide

## Project Layout

- `UnityProject/` is the Unity project root.
- Runtime scripts, scenes, prefabs, and assets are under `UnityProject/Assets/`.
- Unity package dependencies are defined in `UnityProject/Packages/manifest.json`.
- Unity editor configuration is under `UnityProject/ProjectSettings/`.
- Do not manually edit generated Unity directories such as `UnityProject/Library/`, `UnityProject/Temp/`, `UnityProject/Logs/`, or `UnityProject/UserSettings/`.

## OpenSpec Workflow

- OpenSpec configuration and change artifacts live in `openspec/`.
- Shared OpenSpec agent skills live in `.agents/skills/`; keep them tool-agnostic.
- For a feature or behavior change, create and review an OpenSpec proposal before implementation.
- Use the installed CLI from the repository root: `openspec new change <name>`, `openspec status --change <name>`, and `openspec validate --all`.
- Keep implementation tasks and acceptance criteria in the relevant OpenSpec change artifacts.

## SuperBreadMan Level Scope

- Current planning change: `openspec/changes/align-superbreadman-level-scope/`.
- Target whitebox: `UnityProject/Assets/Scenes/Test/superbreadman.unity`.
- Target art scene: `UnityProject/Assets/Scenes/Test/superbreadman 1.unity`.
- These scenes are paired versions of one standalone route. Verify whitebox first, then align the corresponding existing art-scene objects.
- Required route: `Level1 -> Level2 -> Level3 -> Level4 -> Level4.5 -> Level5 -> exit`.
- Before implementation, read the change's `proposal.md`, `specs/superbreadman-level-alignment/spec.md`, `design.md`, and `tasks.md`.
- The initial implementation pass is restricted to existing scene-component fields, parameters, tags, layers, active states, and enabled states. Do not add, copy, delete, move, or rotate scene objects.
- Do not modify scripts, controls, collision, navigation, UI, audio assets, models, materials, lighting, or documentation in that pass. Record such blockers for a later change.
- Preserve `UnityProject/Assets/MoMing/Scenes/Test/superbreadman.unity` and all existing MoMing documents; they are reference material, not targets for this change.

## Unity Changes

- Preserve Unity `.meta` files alongside moved, added, or removed assets.
- Avoid unrelated scene or asset reserialization.
- Verify C# changes in the Unity Editor or with the project's supported Unity test workflow when available.
