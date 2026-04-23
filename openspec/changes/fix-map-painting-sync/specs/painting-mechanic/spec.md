## MODIFIED Requirements

### Requirement: Tile Painting
Bomb explosions SHALL paint the floor tiles within their blast radius with the color assigned to the player who placed the bomb. This painting MUST be visible to all players in the match, regardless of who placed the bomb.

#### Scenario: Bomb Explodes on Floor for All Players
- **WHEN** a bomb placed by a player (e.g., Player 1) explodes
- **THEN** all floor tiles within the explosion range are colored with that player's specific color (e.g., Red) on all connected clients' screens.

### Requirement: Color Overwriting
If a tile is already painted, an explosion from a different player's bomb SHALL overwrite the existing color with the new player's color. This change MUST be synchronized across all clients to ensure scoring consistency.

#### Scenario: Overwriting Opponent Color for All Players
- **WHEN** Player 2's bomb explodes on tiles already painted Red by Player 1
- **THEN** those tiles are changed to Player 2's color (e.g., Blue) on all connected clients' screens.

## ADDED Requirements

### Requirement: Accurate Scoring Source
Scoring percentages SHALL be calculated from the centralized, server-authoritative map state rather than individual client visual representations.

#### Scenario: Scoring Calculation
- **WHEN** the match ends
- **THEN** the server MUST calculate the final area percentages based on its authoritative list of painted tiles
- **AND** broadcast these final percentages to all clients for display.
