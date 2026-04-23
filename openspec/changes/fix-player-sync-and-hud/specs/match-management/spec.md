## MODIFIED Requirements

### Requirement: Area Scoring
The system SHALL calculate the total number of tiles painted by each player color (Red vs Blue) to determine the winner.
Player 1 score MUST correspond to Red tiles, and Player 2 score MUST correspond to Blue tiles.

#### Scenario: End of Match Winner
- **WHEN** the 60-second timer reaches zero
- **THEN** the system counts Red tiles for Player 1 and Blue tiles for Player 2
- **AND** the player with the highest tile count is declared the winner

### Requirement: Player Status Display
The UI SHALL display stats for each player (Lives, Area Percentage) in their respective slots on the HUD.
The stats for Player 1 (ID 0) MUST be shown in the P1 slot, and Player 2 (ID 1) MUST be shown in the P2 slot.

#### Scenario: Life Display Update
- **WHEN** any player loses a life
- **THEN** the HUD label corresponding to their playerId is updated immediately
- **AND** the other player's label remains unchanged
