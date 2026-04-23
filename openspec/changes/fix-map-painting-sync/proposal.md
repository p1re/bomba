## Why

Map painting is currently inconsistent in multiplayer sessions. When a bomb explodes, the resulting paint is often only visible to the player who placed the bomb or the host, leading to visual desynchronization and incorrect scoring for other players. This change ensures a unified, reliable painting state across all connected clients.

## What Changes

- Refactor `TilePainter` to use a hybrid approach: `ClientRpc` for immediate visual feedback and `NetworkList` for persistent state synchronization.
- Unify coordinate rounding logic across all components to prevent alignment issues.
- Ensure that late-joining players or players who refresh their view correctly see the current state of the map.

## Capabilities

### New Capabilities
- `map-state-synchronization`: Reliable synchronization of the tilemap state between the server and all clients, including persistent storage of painted tiles.

### Modified Capabilities
- `painting-mechanic`: Added requirement for multi-client visual consistency and reliable state reporting for scoring.

## Impact

- `Assets/Scripts/TilePainter.cs`: Core logic for network-synced painting.
- `Assets/Scripts/BombController.cs`: How bombs trigger painting events.
- `Assets/Scripts/Explosion.cs`: Timing and execution of paint calls.
- `Assets/Scripts/MatchManager.cs`: Scoring logic based on the unified map state.
