## Purpose

Allow formal notice boards to present ordered, role-specific pages while requiring explicit migration from legacy single-image assignments.

## ADDED Requirements

### Requirement: Role-specific notice page sequences
The system SHALL allow a formal notice board to define an ordered sequence of notice sprites independently for the human and dog actors. When an actor reads a board with one or more valid pages for that actor, the system SHALL display those pages in authored order using the existing parchment popup navigation.

#### Scenario: Human reads a multi-page notice
- **WHEN** the human reads a board configured with three human notice pages
- **THEN** the popup opens on the first page and supports moving backward and forward through all three pages

#### Scenario: Dog reads role-specific pages
- **WHEN** the dog reads a board configured with dog notice pages that differ from the human pages
- **THEN** the popup displays only the dog page sequence

### Requirement: Authoritative role-specific page sequences
The system SHALL use role-specific page sequences as the only notice-board content configuration and SHALL not retain legacy single-page role assignments after migration is complete.

#### Scenario: Migrated board has no legacy fields
- **WHEN** a migrated board is inspected
- **THEN** it exposes only Human Pages and Dog Pages as notice-content fields

### Requirement: Missing notice content handling
The system SHALL not open a notice popup when the reading actor has no valid sequence pages, and SHALL report the missing actor-specific content for diagnosis.

#### Scenario: No page is configured for the reader
- **WHEN** an actor reads a board with no valid content for that actor
- **THEN** no popup is shown and the interaction reports that actor-specific notice content is missing
