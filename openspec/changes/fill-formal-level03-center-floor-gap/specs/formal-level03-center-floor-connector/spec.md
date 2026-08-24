## Purpose

Provide uninterrupted floor support through the Formal Level 3 center connector between the existing north and west-south floor volumes.

## ADDED Requirements

### Requirement: Continuous center floor support
The system SHALL provide enabled non-trigger floor collision across the complete gap between Floor_CenterNorth and Floor_CenterWestSouth.

#### Scenario: Player crosses the center connector
- **WHEN** a formal player actor moves through the center connector between the north and west-south floor regions
- **THEN** the actor remains supported by floor collision without falling through an uncovered gap.
