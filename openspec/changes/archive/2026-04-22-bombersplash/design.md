## Context

The current project is a classic Bomberman implementation in Unity. The transition to "BomberSplash" requires moving from an elimination-based win condition to an area-control (painting) model. This involves significant changes to the player life cycle, bomb explosion behavior, and server-side connectivity.

## Goals / Non-Goals

**Goals:**
- Shift gameplay focus to floor painting and area coverage.
- Implement a non-persistent "death" via a 3-life system and respawning.
- Integrate WebSockets for server-side connection monitoring.
- Implement a 60-second match loop with automated result reporting via UnityWebRequest.

**Non-Goals:**
- Implementing ML-Agents or AI behavior at this stage.
- Replacing the core physics or movement system.
- Modifying the existing room/lobby connection logic beyond adding WebSocket monitoring.

## Decisions

- **Multiplayer Architecture**: The core gameplay (movement, bomb placement, tile painting) SHALL continue to use **Unity Netcode for GameObjects** to maintain existing cross-platform compatibility and low-latency synchronization.
- **Painting Implementation**: Use a grid-based system (Unity Tilemap) where each tile's "owner" is synchronized across the network. Bomb explosions will update the tile owner via ServerRPCs to ensure authority and consistency.
- **Area Calculation**: Instead of a full-map scan every frame, the game will maintain a running counter of tiles per player color, updating only when a tile's owner changes.
- **Life & Respawn**: The `Player` script will be extended with a `CurrentLives` variable (NetworkVariable) and a `SpawnPoint` reference. Respawning logic will be handled server-side.
- **WebSocket Integration**: Implement a `WebSocketManager` as a secondary monitoring layer that runs alongside the Netcode stack to track session persistence and server-side user presence.
- **Data Reporting**: Use `UnityWebRequest.Post` at the conclusion of the match to send a JSON payload containing the final score and winner to an external API.

## Risks / Trade-offs

- **[Risk] Synchronization of Painted Tiles** → [Mitigation] Use Unity's Netcode to synchronize the tilemap state across clients, treating the "owner" property as a networked variable.
- **[Risk] WebSocket Latency vs Game State** → [Mitigation] Keep WebSockets for high-level connection monitoring only; use standard Netcode for low-latency gameplay events like bomb placement and player movement.
- **[Risk] Power-up Reset Impact** → [Mitigation] Clearly signal power-up loss in the UI so players understand why their range or speed has decreased after respawning.
