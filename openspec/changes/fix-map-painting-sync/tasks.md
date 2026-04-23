## 1. Refactor TilePainter for State Persistence

- [x] 1.1 Implement `TileStatus` struct for network serialization
- [x] 1.2 Replace `paintedTiles` dictionary with `NetworkList<TileStatus>`
- [x] 1.3 Implement `PaintInstantClientRpc` for low-latency visual updates
- [x] 1.4 Update `Paint` method to handle server-side state recording and broadcasting
- [x] 1.5 Implement `RepaintAll` and call it in `OnNetworkSpawn` for late-join synchronization
- [x] 1.6 Verify coordinate rounding logic using `Mathf.FloorToInt`

## 2. Unify Explosion and Bomb Timing

- [x] 2.1 Update `Explosion.cs` to use the hybrid `Paint` method after a controlled delay
- [x] 2.2 Update `BombController.cs` to remove redundant paint calls and use the centralized `Paint` method
- [x] 2.3 Verify that painting triggers correctly on both local and remote clients simultaneously

## 3. Match Management and Scoring

- [x] 3.1 Update `MatchManager.cs` to calculate area percentages from `TilePainter.Instance.paintedTilesList`
- [x] 3.2 Implement `ShowEndScreenClientRpc` to display final scores calculated from the authoritative state
- [x] 3.3 Verify that all connected clients see identical scores at the end of the match

## 4. Final Polish and Testing

- [x] 4.1 Test late-join scenario: join a match mid-way and verify the map paints correctly
- [x] 4.2 Test "Editor Refresh" scenario: switch focus in Unity and verify paint persists
- [x] 4.3 Verify that "YOU WIN/LOSE" messages accurately reflect the server-calculated scores
