## Context

`FormalTutorialPopup` owns one `Page` Image and already handles multi-page presentation. It currently stores three shared collections (opening, checkpoint, and level introduction); notice boards pass their own collections directly to the same popup.

## Goals / Non-Goals

**Goals:**

- Add two ordered page collections per FormalUI tutorial type.
- Resolve the collection from `FormalPlayerControl.IsDogActive` exactly when FormalUI begins displaying it.
- Keep notice-board and other callers that pass an explicit collection unchanged.
- Preserve each existing FormalPersistent shared collection as its matching Human collection without duplicating its data into the new Dog collection.

**Non-Goals:**

- Change how any trigger decides when to request a tutorial.
- Change the single popup hierarchy, input, pause behavior, PlayerPrefs timing, or page controls.

## Decisions

### Resolve role inside FormalUI at presentation time

Internal opening, checkpoint, and level-introduction presentation paths choose Human or Dog collections by querying the current controller only when they open. The selected `Sprite[]` becomes the popup's active sequence, so it cannot change while visible.

Alternative: require every trigger to pass an actor. Rejected because the requested behavior is owned by FormalUI and would expand unrelated trigger APIs.

### Preserve explicit content calls

The existing API that accepts a supplied page collection remains a direct presentation API. This keeps notice-board pages role-specific at the board and avoids replacing them a second time inside FormalUI.

### Serialization-compatible Human field renames

Rename the three existing shared arrays to Human arrays using Unity serialization rename metadata, then add three separate Dog arrays. This preserves existing scene assignments as Human content without requiring a scene rewrite or showing duplicate fields. Dog arrays begin empty and are intentionally never populated from Human content.

## Risks / Trade-offs

- [Dog collection is omitted] → Do not show a blank or human tutorial; log which dog tutorial type is missing.
- [Builder is rerun] → Its defaults seed the Human arrays with the current shared source assets and leave Dog arrays empty; dog art must be assigned in the Inspector.

## Migration Plan

1. Load FormalPersistent and confirm each existing collection is shown under its renamed Human array.
2. Assign role-specific dog art to the three Dog arrays in the Inspector.
3. Verify each tutorial once while controlling each role.
