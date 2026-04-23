## Context

The current painting system relies on `ClientRpc` calls triggered by explosions. This is an "event-based" system that works well for transient effects (like fire) but fails for persistent state (like paint) because:
- Messages can be missed by clients.
- Late-joining players have no record of past events.
- Unity's Editor refreshes can wipe local transient Tilemap data.

## Goals / Non-Goals

**Goals:**
- Implement a hybrid networking model for the `TilePainter`.
- Use `NetworkList<TileStatus>` to store authoritative map state on the server.
- Use `ClientRpc` to trigger immediate painting on all clients for low-latency visual feedback.
- Ensure all clients automatically synchronize their local Tilemap with the server's state upon joining.

**Non-Goals:**
- Implementing a persistent database for match results (out of scope for this sync fix).
- Refactoring the entire Netcode architecture.

## Decisions

### 1. Hybrid Sync Model (RPC + NetworkList)
- **Decision**: Use `NetworkList` to maintain state and a `ClientRpc` for immediate painting.
- **Rationale**: `NetworkList` ensures reliability and late-join synchronization, while `ClientRpc` provides the lowest possible latency for visual feedback, bypassing the overhead and eventual consistency of list synchronization.

### 2. Precise Coordinate Mapping
- **Decision**: Use `Mathf.FloorToInt(pos) + 0.1f` for grid mapping.
- **Rationale**: Avoids "Banker's Rounding" issues that cause desynchronization between world coordinates and grid cells.

### 3. Server-Authoritative Scoring
- **Decision**: Calculate scores directly from the `NetworkList` on the server.
- **Rationale**: Prevents score discrepancies between clients due to dropped visual packets or local rendering issues.

## Risks / Trade-offs

- **[Risk]**: `NetworkList` can be performance-heavy if the list grows very large (e.g., thousands of tiles).
  - **Mitigation**: A standard Bomberman map is ~200 tiles, which is well within the safe limits for a `NetworkList`.
- **[Risk]**: Double-painting if the RPC and List Sync arrive at slightly different times.
  - **Mitigation**: The painting logic checks if a tile is already painted with the same color before applying flags, making the operation idempotent.
