## Why

Formal notice boards need role-specific multi-page notices without retaining obsolete single-sprite configuration after migration is complete.

## What Changes

- Allow each `FormalNoticeBoard` to provide an ordered set of notice sprites for the human and for the dog.
- Show the selected actor's complete set through the existing `FormalTutorialPopup`, including its current previous/next controls and keyboard navigation.
- Remove the migrated legacy single-sprite fields so notice-board content has one authoritative configuration per role.

## Capabilities

### New Capabilities

- `formal-notice-board-pages`: Role-specific, repeatable multi-page notice-board reading.

### Modified Capabilities

- None.

## Impact

- `UnityProject/Assets/MoMing/Scripts/LevelRuntime/FormalNoticeBoard.cs`
- Legacy single-sprite references are removed; page lists are the sole content source and no popup UI hierarchy changes.
