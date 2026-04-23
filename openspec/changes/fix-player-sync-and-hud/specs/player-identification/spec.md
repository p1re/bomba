## ADDED Requirements

### Requirement: Unique Player ID Assignment
The system SHALL assign a unique ID (0 or 1) to each player upon joining a match.
In Online Multiplayer, the Host MUST be assigned ID 0 and the first Client MUST be assigned ID 1 (based on OwnerClientId).
In Local Training mode, the Human player MUST be assigned ID 0 and the AI player MUST be assigned ID 1.

#### Scenario: Online ID Assignment
- **WHEN** a Host joins a match
- **THEN** their playerId is set to 0
- **WHEN** a Client joins that match
- **THEN** their playerId is set to 1

#### Scenario: Local Training ID Assignment
- **WHEN** starting a local match with an AI
- **THEN** the Human player has playerId 0
- **AND** the AI player has playerId 1

### Requirement: ID-Based Color Assignment
The system SHALL assign a fixed color to each player based on their ID.
Player 1 (ID 0) MUST be assigned Red.
Player 2 (ID 1) MUST be assigned Blue.

#### Scenario: Visual Feedback by ID
- **WHEN** Player 1 spawns
- **THEN** their color is set to Red
- **WHEN** Player 2 spawns
- **THEN** their color is set to Blue
