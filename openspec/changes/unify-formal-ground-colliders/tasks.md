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

- [ ] 3.1 Stakeholder manually retags Default-layer floor proxies → NavGround (audit list assists)
- [ ] 3.2 Run disable pass; verify walls/furniture colliders untouched
- [ ] 3.3 Audit across all level combinations; resolve reported holes/misalignments (first finding: floor2(2) 5.4cm low)
- [ ] 3.4 Verify A* height sampling sees new ground; re-bake monster/dog graphs
- [ ] 3.5 Full route playtest: no fall-throughs, consistent floor height across transitions
