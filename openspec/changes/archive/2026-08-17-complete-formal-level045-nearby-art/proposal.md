## Why

The selected Level 4.5 art omitted five source wall tiles that bound the corridor, leaving visible wall gaps despite the otherwise validated user-selected migration.

## What Changes

- Add only the five missing Level 4.5 wall tile visuals identified by world Bounds comparison.
- Preserve existing L045_Content renderers and their verified global transforms.
- Revalidate the complete Level 4.5 corridor wall candidate set by world Bounds.

## Capabilities

### New Capabilities
- `formal-level045-nearby-art-completion`: Defines completion of missing corridor wall visuals adjacent to the selected Level 4.5 art.

### Modified Capabilities
- None.

## Impact

- Affects L045_Content and FormalLevel045 visual content only.
- Does not modify source art, collision, player ownership, or mechanics.
