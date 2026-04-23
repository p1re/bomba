## 1. Core Logic: Player ID Assignment

- [x] 1.1 Modify `PlayerData.cs` to assign `playerId` based on `OwnerClientId` for online sessions and `isAI` for local sessions.
- [x] 1.2 Modify `PlayerColorProvider.cs` to set `playerColor` based on the unique `playerId`.
- [x] 1.3 Ensure `LobbyManager.cs` correctly resets `IsLocalMode` when starting an online or local session.

## 2. HUD and UI Fixes

- [x] 2.1 Update `HUDController.cs` to ensure stat labels are updated only for the player matching the slot's ID.
- [x] 2.2 Verify `MatchManager.cs` correctly handles score updates using the `UpdateAreaScores` method.

## 3. Scoring and Area Calculation

- [x] 3.1 Update `TilePainter.cs` to ensure tile counts are accurately mapped to the player colors (Red/Blue).
- [x] 3.2 Ensure `GetScores` correctly calculates percentages for both players based on the new ID/Color mapping.

## 4. Verification

- [ ] 4.1 Test Local AI Mode: Verify P1 is Red, P2 (AI) is Blue, and HUD shows both correctly.
- [ ] 4.2 Test Online Mode (Host): Verify Host is Red (ID 0).
- [ ] 4.3 Test Online Mode (Client): Verify Client is Blue (ID 1).
- [ ] 4.4 Test Online Mode Score: Verify painting tiles updates the correct area percentage on both clients.
