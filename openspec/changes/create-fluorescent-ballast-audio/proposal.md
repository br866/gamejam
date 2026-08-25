# Proposal: create-fluorescent-ballast-audio

## Why

The hospital environment needs an old fluorescent-light electrical presence
that can appear intermittently without reading as music or a modern electronic
UI tone.

## What Changes

- Create three related old-ballast buzz one-shots and one seamless continuous
  bed loop with SuperCollider.
- Build the sound from 50 Hz mains hum, restrained harmonics, transformer buzz,
  and subtle unstable high-frequency chatter.
- Keep the assets mono so Wwise can position them at individual light fixtures.
- Build the continuous bed from phase-coherent periodic components so it can
  loop beneath the intermittent variations without a boundary click.
- Save a reproducible SuperCollider source script alongside the generated-audio
  workflow.
- Leave random timing, spacing, pitch variation, and event routing to Wwise.

## Impact

- Four new 48 kHz, 24-bit mono WAV files under Wwise Originals/SFX.
- Two new `.scd` source files.
- No Unity scene, script, Wwise Work Unit, Event, or SoundBank edits.
