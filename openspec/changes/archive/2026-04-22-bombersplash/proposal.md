## Why

Transitioning from a traditional lethal Bomberman gameplay to "BomberSplash," a non-lethal area-control game. This shift aims to create a more competitive and strategic experience where the objective is painting the map rather than eliminating opponents, while also modernizing the networking stack with WebSockets.

## What Changes

- **Renaming**: Project identity changed from Bomberman to BomberSplash.
- **Non-Lethal Gameplay**: Bombs no longer kill players. Instead, players lose one of their 3 lives upon impact.
- **Life System**: Players start with 3 lives. When reduced to 0, power-ups (extra bombs, range, speed) are reset, and the player respawns at their original spawn point.
- **Painting Mechanic**: Bomb explosions paint the floor in the player's unique color (e.g., Red for Player 1, Blue for Player 2).
- **Match Objective**: A 60-second timer is introduced. The winner is the player who has painted the most surface area when time expires.
- **Networking**: Introduction of WebSockets for handling server-side user connections.
- **Data Integration**: Implementation of UnityWebRequest for external data interaction (e.g., fetching stats or reporting results).

## Capabilities

### New Capabilities
- `life-system`: Manage player lives, respawning logic, and power-up persistence/reset.
- `painting-mechanic`: System for applying color to tiles and calculating area coverage per player.
- `match-management`: 60-second match timer, score calculation based on painted area, and win condition handling.
- `websocket-networking`: Server-side connection management and synchronization using WebSockets.
- `external-data-handler`: Handling UnityWebRequests for backend communication and data fetching.

### Modified Capabilities
- None (Initial project specs do not exist).

## Impact

- **Player Logic**: Major updates to health, life tracking, and respawn mechanisms.
- **Bomb System**: Extended to include color-tagging for explosions and interaction with the floor grid.
- **Game Flow**: Introduction of a fixed-time match loop and area-based scoring.
- **Networking Architecture**: Shift towards a hybrid or WebSocket-based model for server connections.
- **UI**: Display of player lives, names, and the 60-second timer.
