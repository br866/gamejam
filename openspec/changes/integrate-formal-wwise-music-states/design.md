# Design: integrate-formal-wwise-music-states

## Context

- `FormalPersistent` remains loaded while formal level scenes are loaded and unloaded additively.
- `FormalAnxietyState` lives in `FormalPersistent` and exposes `Normalized` and `IsSeparated`; only `Normalized` drives music anxiety.
- `MonsterPatrol.IsChasing` is the authoritative public signal for player detection/chase. FormalLevel02 contains one monster and FormalLevel04 contains two; the other formal levels contain none.
- Wwise `Gameplay_Music` uses two global State Groups: `MusicMode` (`Explore`, `Combat`) and `AnxietyLevel` (`Low`, `Mid`, `High`).
- The existing `Audio_Music` object has `AkAmbient` with `Play_MusicMode` and `AkGameObj`.

## Decisions

### D1. Poll cached monsters, refresh only on scene topology changes

The controller caches active `MonsterPatrol` components and checks their inexpensive `IsChasing` property each frame. The cache is rebuilt at startup and after additive scene load/unload events. This provides immediate music response without performing `FindObjectsOfType` every frame or changing the monster implementation.

### D2. Use Wwise State names as serialized configuration

State Group and State names default to the authored values but remain serialized for Inspector visibility and future renaming. Runtime calls use `AkUnitySoundEngine.SetState(group, value)`, avoiding a requirement to create or drag five Unity Wwise State reference assets.

### D3. Set initial States before posting the event

The controller waits for `FormalAnxietyState`, applies `MusicMode` and `AnxietyLevel`, then posts `AkAmbient.data`. The component therefore requires `AkAmbient.Trigger On = Nothing`; leaving the automatic Start trigger enabled would double-post the music event.

### D4. Cache applied State bands and re-evaluate resets

Only changed State values are sent to Wwise. The controller reads current values every frame rather than relying solely on `FormalAnxietyState` events because `ResetAnxiety()` can clear separation without emitting `OnSeparationChanged`. This guarantees restart returns to the current monster/anxiety combination.

## Anxiety Bands

- Low: `Normalized < 0.45`
- Mid: `0.45 <= Normalized < 0.75`
- High: `Normalized >= 0.75`

Thresholds are serialized and clamped so `highThreshold` cannot fall below `midThreshold` at runtime.

## Risks / Trade-offs

- String State names are not compile-time checked. Serialized defaults and explicit validation logs make authoring mismatches visible.
- A monster retained in an additively loaded predecessor scene can keep Combat active while it is still chasing. This matches the formal route's retained-threat behavior.
- The legacy Unity `MusicManager` can still auto-spawn and play `Main Title.mp3`; disabling that legacy BGM path is a separate follow-up to avoid overlapping music.

