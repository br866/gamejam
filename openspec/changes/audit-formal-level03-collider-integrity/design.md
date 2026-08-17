## Context

FormalLevel03 has dedicated floor, boundary, and route proxies plus a broad BoxCollider proxy group created from renderer world bounds. Renderer-derived axis-aligned proxies can overstate irregular, elevated, decorative, or source-misaligned geometry.

## Goals / Non-Goals

**Goals:**
- Inspect all Collider components, including dedicated and broad proxy groups.
- Remove confirmed broad-proxy defects while retaining valid traversal support and physical obstacles.

**Non-Goals:**
- Modify renderer hierarchy, visual transforms, source art, gameplay mechanics, or player control.

## Decisions

### Retain dedicated traversal foundation and audit broad proxies conservatively

Existing floor, boundary, and route wall colliders have separately validated responsibilities and remain unless directly invalid. Broad proxy colliders are checked against their source role and world placement; non-grounded decoration, labels, lights, carpets, signs, and non-obstructive detail do not receive blocking proxies.

Alternative considered: remove all broad coverage. Rejected because major furniture and doors need physical presence.

## Risks / Trade-offs

- [Removing a proxy makes a substantial prop passable] -> Keep proxies for architecture, doors, large furniture, and grounded fixed obstacles.
- [Keeping an oversized proxy blocks a corridor] -> Validate Pad spawn and baseline routes after each correction set.
