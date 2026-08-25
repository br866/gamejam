# Proposal: integrate-death-cause-wwise-audio

## Why

The formal route already distinguishes anxiety deaths from monster catches, but
both paths currently show the death UI without changing Wwise playback. The
newly authored death stinger and state-driven death music need one lifecycle so
they cannot overlap persistent gameplay music or survive a restart.

## What Changes

- Add a focused `FormalWwiseDeathAudio` component to the existing registered
  `Audio_Music` GameObject.
- Map Unity `DeathCause.Anxiety` to Wwise `COD/Anxiety` and
  `DeathCause.Caught` to Wwise `COD/Eliminated` before posting death music.
- Stop gameplay music, post `Play_PlayerDeath_Stinger`, and post
  `Play_DeathCause_Music` once when the death screen opens.
- Stop both death music and the active stinger when the player restarts or
  returns to the title scene.
- Reuse the existing centralized gameplay-music restart path after a level
  reset.

## Impact

- One new runtime audio component and its Unity meta file.
- Small, localized calls in `FormalDeathScreen` and
  `FormalWwiseMusicController`.
- One localized component block on `Audio_Music` in `FormalPersistent`.
- Unity Wwise Event reference assets for the three generated Events.
- No edits to Wwise Work Units, original audio, or generated SoundBanks.

