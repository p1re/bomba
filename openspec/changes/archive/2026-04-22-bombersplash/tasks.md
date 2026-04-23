## 1. Project Setup & Identity

- [x] 1.1 Rename project and main menu UI headers to "BomberSplash"
- [x] 1.2 Define player color mapping (e.g., Player 1: Red, Player 2: Blue) and assign to player prefabs

## 2. Life System Implementation

- [x] 2.1 Update `Player` script with `NetworkVariable<int> CurrentLives` (default 3) and `SpawnPoint`
- [x] 2.2 Create UI overlay to display player names and their current life count in screen corners
- [x] 2.3 Modify bomb collision/explosion logic to trigger a `ServerRPC` to decrement player lives
- [x] 2.4 Implement server-side respawn logic: reset power-ups (bombs, range, speed) and sync position across clients
- [x] 2.5 Add a 2-second invulnerability period with synchronized visual feedback across the network

## 3. Floor Painting & Area Calculation

- [x] 3.1 Implement a networked tilemap system to track the "owner" of each floor tile via Netcode
- [x] 3.2 Extend `Bomb` explosion logic to use `ServerRPC` for updating tile ownership in range
- [x] 3.3 Ensure explosions correctly overwrite and synchronize tile colors for all clients
- [x] 3.4 Create a background manager to calculate real-time scores per player color on the server

## 4. Match Management & Win Condition

- [x] 4.1 Implement a 60-second countdown timer in a new `MatchManager` component
- [x] 4.2 Display the match timer prominently in the top center of the UI
- [x] 4.3 Implement end-of-match logic: freeze gameplay when timer reaches 0
- [x] 4.4 Calculate the winner based on the highest tile count and display a "Winner: [Player Name]" screen

## 5. Networking & Integration

- [x] 5.1 Integrate a WebSocket client library into the Unity project
- [x] 5.2 Create a `WebSocketManager` to handle server-side connection persistence and monitoring
- [x] 5.3 Implement a `MatchResultReporter` using `UnityWebRequest` to POST match stats to an external endpoint upon match completion

## 6. Testing & Validation

- [x] 6.1 Verify that being hit by a bomb reduces lives correctly and does not destroy the player object
- [x] 6.2 Test that respawning correctly resets power-ups to initial values
- [x] 6.3 Validate that floor tiles are painted correctly and synchronized across all clients in a networked session
- [x] 6.4 Confirm the 60-second timer triggers the winner screen and sends the WebRequest payload correctly
