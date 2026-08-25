## Context

`FormalTutorialPopup` already accepts a `Sprite[]`, renders a page count, and handles previous/next buttons plus Left/Right and A/D keys. `FormalNoticeBoard` has completed its migration to role-specific page arrays.

## Goals / Non-Goals

**Goals:**

- Give each actor an Inspector-configurable, ordered collection of notice pages.
- Reuse the popup's established paging, close, pause, input, and audio behavior.
- Keep one authoritative role-specific page-list configuration per board.

**Non-Goals:**

- Change popup controls, visual presentation, input bindings, save behavior, or scene object layout.
- Reintroduce or preserve deprecated single-page fields.

## Decisions

### Remove migrated legacy single-page fields

Remove the original single-sprite fields and all references to them. Runtime reading already selects only the page list, so removal eliminates Inspector noise and prevents stale data from being mistaken for active content.

Alternative: retain legacy fields as read-only references. Rejected because migration is complete and duplicated configuration can become stale.

### Normalize configured pages before opening the popup

Build the selected role's display sequence from non-null sprites in authored order. If no valid list page remains, report missing role-specific content instead of opening a popup. This prevents an accidentally empty list slot from becoming a blank or unexpected legacy page.

Alternative: pass raw arrays through. Rejected because null entries are easy to create in the Inspector and would produce broken pages.

### Keep notices repeatable

Continue calling the popup with no preference key. Notice boards remain re-readable, including multi-page notices.

## Risks / Trade-offs

- [Page normalization allocates during interaction] → The interaction is infrequent and the page counts are small; prioritize clear, safe behavior over persistent mutable buffers.

## Migration Plan

1. Confirm every board's Human Pages and Dog Pages are populated as intended.
2. Remove legacy single-page fields from the component.
3. Verify human and dog sequences independently, including previous and next navigation.
