## Purpose

Provide distinct, ordered FormalUI tutorial pages for the human and dog while preserving one shared popup and its established paging controls.

## ADDED Requirements

### Requirement: Role-specific FormalUI page collections
The system SHALL provide independent ordered human and dog page collections for the opening tutorial, checkpoint tutorial, and level-introduction tutorial.

#### Scenario: Different opening tutorial pages
- **WHEN** FormalUI presents the opening tutorial while the human is controlled
- **THEN** it displays the configured human opening page collection in authored order

#### Scenario: Different checkpoint tutorial pages
- **WHEN** FormalUI presents the checkpoint tutorial while the dog is controlled
- **THEN** it displays the configured dog checkpoint page collection in authored order

### Requirement: Active role is fixed for an open popup
The system SHALL select a FormalUI tutorial collection from the role currently controlled when the popup opens, and SHALL retain that selected collection until the popup closes.

#### Scenario: Player switches role after opening
- **WHEN** a role-specific tutorial popup is already open and the controlled role changes after it closes
- **THEN** the open popup retains its originally selected pages and the next popup selects from the newly controlled role

### Requirement: Preserve shared collections as human content
The system SHALL preserve every existing shared opening, checkpoint, and level-introduction collection as the corresponding human collection when role-specific collections are introduced. The system SHALL provide a distinct dog collection for each tutorial type without copying or falling back to human content. Externally supplied page collections SHALL continue to display exactly as supplied.

#### Scenario: Existing tutorial becomes human content
- **WHEN** a FormalUI tutorial has a configured shared collection before the role-specific update
- **THEN** those same images appear in the matching Human Pages collection after the update

#### Scenario: Dog content has not been configured
- **WHEN** the dog is controlled and the matching Dog Pages collection is empty
- **THEN** FormalUI does not open that tutorial and reports the missing dog-specific content

#### Scenario: Notice board supplies pages
- **WHEN** a notice board supplies an explicit page collection to FormalUI
- **THEN** FormalUI displays that supplied collection without replacing it based on the controlled role
