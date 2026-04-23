## ADDED Requirements

### Requirement: Match Timer
A match SHALL have a fixed duration of 60 seconds.

#### Scenario: Timer Countdown
- **WHEN** the match begins
- **THEN** a 60-second countdown starts
- **AND** the match ends exactly when the timer reaches zero

### Requirement: Area Scoring
The system SHALL calculate the total number of tiles painted by each player to determine the winner.

#### Scenario: End of Match Winner
- **WHEN** the 60-second timer reaches zero
- **THEN** the system counts tiles for each player color
- **AND** the player with the highest tile count is declared the winner

### Requirement: Player Status Display
The UI SHALL display each player's name and their current life count in the corners of the screen.

#### Scenario: Life Display Update
- **WHEN** a player loses a life
- **THEN** the corresponding life counter in the corner of the screen is updated immediately
