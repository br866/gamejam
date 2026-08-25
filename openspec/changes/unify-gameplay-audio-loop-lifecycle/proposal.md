# Proposal: unify-gameplay-audio-loop-lifecycle

## Why

Unity stops physics updates when menus, tutorials, or death screens set
`Time.timeScale` to zero, but Wwise loops already posted by gameplay continue
independently. The crate loop currently recognizes death explicitly, leaving the
same leak possible for every other gameplay-suspending UI.

## What Changes

- Add one Unity-side gameplay simulation gate shared by runtime systems.
- Treat any zero-timescale or known formal blocking UI state as suspended.
- Make both formal crate movers stop their active Wwise loop whenever gameplay
  becomes suspended.
- Do not automatically resume a stopped loop; actual crate movement must post
  it again after gameplay resumes.

## Impact

- One small static runtime class and its Unity meta file.
- Localized condition changes in the two formal crate mover scripts.
- No Wwise Work Unit, Event, SoundBank, scene, or prefab changes.

