## Tasks

## 1. Component

- [x] 1.1 `FormalGroundVolume.cs`: topHeight/thickness fields, BoxCollider sync, gizmo, NavGround layer guard
- [x] 1.2 Instantiate global volume in `FormalPersistent.unity`; topHeight = 9.904 (measured L01 floor top), footprint 500x500 at (30,-15)

## 2. Editor tools (`Tools/Formal/Ground/`)

- [x] 2.1 Volume From Selection (create/refit) — verified footprint matches selected renderer bounds exactly
- [x] 2.2 Disable NavGround Colliders + journal + rollback command — verified: proxy disabled, volume carrier skipped, journal written, rollback re-enabled
- [x] 2.3 Audit Coverage (holes + misalignment report) — L01+SharedArt run: holes=0, misaligned=8 (7 heuristic noise, 1 real: SharedArt_L01_L02/floor2(2) top=9.850 vs 9.904, -5.4cm)
- [x] 2.4 Copy Top Height across selection — verified 123 → 9.904

## 3. Migration & verification

- ~~3.1 Retag Default→NavGround~~ — skipped by design (final approach uses one large volume, no legacy disable needed)
- ~~3.2 Disable pass + rollback verification~~ — skipped by design (same reason)
- [x] 3.3 Audit across level combinations — initial L01+SharedArt run done (holes=0); full-combination audit optional under new approach since volume covers everything
- [x] 3.4 A* height sampling — legacy colliders untouched so existing graphs still sample correctly; re-bake only if playtesting reveals issues
- [x] 3.5 Full route playtest: no fall-throughs, consistent floor height across transitions — stakeholder confirmed acceptance passed
