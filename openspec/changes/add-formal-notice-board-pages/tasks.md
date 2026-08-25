## 1. Notice-board content configuration

- [x] 1.1 Remove the migrated legacy single-page fields and keep ordered, role-specific page lists as the only content configuration.
- [x] 1.2 Select and normalize the reading actor's valid page sequence while preserving the missing-content warning path when that sequence is empty.

## 2. Popup integration and verification

- [x] 2.1 Pass the selected complete sequence to `FormalTutorialPopup` without changing its repeatable-reading behavior.
- [x] 2.2 Verify compilation and confirm a multi-page sequence opens in order, supports previous/next navigation, and exposes no legacy single-page content fields.
