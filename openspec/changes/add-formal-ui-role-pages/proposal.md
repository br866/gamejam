## Why

The shared FormalUI tutorial popup currently presents the same pages regardless of which character the player controls. Role-specific instructions need distinct, ordered page sequences while retaining the existing shared popup and navigation.

## What Changes

- Add human and dog multi-page content sets for FormalUI's opening tutorial, checkpoint tutorial, and level-introduction tutorial.
- Select the content set from the role currently controlled when FormalUI is presented; keep that set fixed until the popup closes.
- Rename each prior shared content array into the corresponding human array without losing its serialized images, and add a separate dog array for new content.
- Keep externally supplied page sequences, including notice-board pages, unchanged.

## Capabilities

### New Capabilities

- `formal-ui-role-pages`: Role-specific, paged FormalUI tutorial content.

### Modified Capabilities

- None.

## Impact

- `UnityProject/Assets/MoMing/Scripts/UI/FormalTutorialPopup.cs`
- `UnityProject/Assets/MoMing/Scripts/Editor/FormalUIBuilder.cs`
- Existing `FormalPersistent` page assignments, which are retained as Human content through serialization-compatible field renames.
