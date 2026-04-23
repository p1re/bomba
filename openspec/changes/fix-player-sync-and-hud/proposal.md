## Why

The current player synchronization logic fails to distinguish between two human players in online multiplayer mode. Both players are assigned the same ID (0) and color (Red), leading to overlapping HUD stats (lives, area percentage) and incorrect score calculation. This change ensures unique IDs and visual feedback for both online and local (AI) modes.

## What Changes

- **Unique Player Identification**: Implements a robust ID assignment logic that uses `OwnerClientId` for online matches and manual IDs for local AI training.
- **Dynamic Color Assignment**: Links player colors directly to their assigned IDs (P1/ID 0 = Red, P2/ID 1 = Blue) to ensure visual distinction.
- **HUD Synchronization**: Fixes the HUD controller to correctly map and display stats for both players independently based on their unique IDs.
- **Score Calculation**: Fixes `TilePainter` to correctly attribute painted tiles to the appropriate player based on their unique colors.
- **Mode Persistence Fix**: Ensures `IsLocalMode` is correctly reset when switching between local and online game modes.

## Capabilities

### New Capabilities
- `player-identification`: System to assign and manage unique IDs and colors for players across different game modes (Online vs Local).

### Modified Capabilities
- `match-management`: Updating the HUD and scoring logic to support independent tracking of multiple players.

## Impact

- `PlayerData.cs`: Core logic for ID assignment.
- `PlayerColorProvider.cs`: Color logic based on player ID.
- `HUDController.cs`: UI mapping for player stats.
- `TilePainter.cs`: Scoring logic based on tile colors.
- `LobbyManager.cs`: Mode state management.
