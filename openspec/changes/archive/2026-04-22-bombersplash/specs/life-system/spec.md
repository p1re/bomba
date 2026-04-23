## ADDED Requirements

### Requirement: Player Lives
Each player SHALL start the match with 3 lives.

#### Scenario: Initial Lives Setup
- **WHEN** the match starts
- **THEN** every player has a life counter set to 3

### Requirement: Damage Penalty
A player SHALL lose exactly 1 life when caught within the explosion range of a bomb.

#### Scenario: Player Hit by Explosion
- **WHEN** a player is within the blast radius of an exploding bomb
- **THEN** their life counter is decremented by 1

### Requirement: Respawn and Reset
When a player's life count reaches 0, the system SHALL reset their power-ups and teleport them to their initial spawn point.

#### Scenario: Player Reaches Zero Lives
- **WHEN** a player's life count becomes 0
- **THEN** their extra bombs, blast range, and movement speed power-ups are reset to defaults
- **AND** the player is moved to their starting spawn point
- **AND** their life count is reset to 3
