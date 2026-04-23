# painting-mechanic Specification

## Purpose
TBD - created by archiving change bombersplash. Update Purpose after archive.
## Requirements
### Requirement: Tile Painting
Bomb explosions SHALL paint the floor tiles within their blast radius with the color assigned to the player who placed the bomb.

#### Scenario: Bomb Explodes on Floor
- **WHEN** a bomb placed by a player (e.g., Player 1) explodes
- **THEN** all floor tiles within the explosion range are colored with that player's specific color (e.g., Red)

### Requirement: Color Overwriting
If a tile is already painted, an explosion from a different player's bomb SHALL overwrite the existing color with the new player's color.

#### Scenario: Overwriting Opponent Color
- **WHEN** Player 2's bomb explodes on tiles already painted Red by Player 1
- **THEN** those tiles are changed to Player 2's color (e.g., Blue)

### Requirement: Destructible Interaction
Explosions SHALL continue to destroy destructible walls as per standard mechanics, while also painting the floor beneath them.

#### Scenario: Breaking Wall and Painting
- **WHEN** a bomb explodes next to a destructible wall
- **THEN** the wall is destroyed
- **AND** the floor tile previously under the wall is painted with the player's color

