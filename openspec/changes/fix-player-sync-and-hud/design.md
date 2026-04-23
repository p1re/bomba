## Context

The project is a Bomberman-style game with two primary modes:
1.  **Online Multiplayer**: Uses Unity Netcode for GameObjects and Unity Services (Relay/Lobby).
2.  **Local Training (IA)**: Uses ML-Agents to train an AI against a human player in a local session.

Currently, the `PlayerData` and `PlayerColorProvider` components use a simple `isAI` check to assign IDs and colors. In online multiplayer, this results in both players (both human) getting ID 0 and Color Red. This causes stats like lives and area coverage percentage to overlap or be attributed only to Player 1 in the HUD.

## Goals / Non-Goals

**Goals:**
-   **Unique ID Assignment**: Ensure Player 1 (Host) gets ID 0 and Player 2 (Client or AI) gets ID 1.
-   **Consistent Coloring**: Link colors (Red for ID 0, Blue for ID 1) across all systems (Player, HUD, TilePainter).
-   **Independent HUD Tracking**: Ensure the HUD correctly displays stats for both players separately.
-   **Reliable Score Reporting**: Ensure `TilePainter` counts scores based on the unique colors assigned to each ID.

**Non-Goals:**
-   Implementing matchmaking for more than 2 players.
-   Refactoring the entire networking layer.
-   Adding new power-ups or game mechanics.

## Decisions

### 1. Unified ID Assignment Strategy
We will modify `PlayerData.cs` to use `OwnerClientId` when in online mode (`!LobbyManager.IsLocalMode`).
-   **Rationale**: `OwnerClientId` is the standard way Netcode identifies connections. Host is always 0, and the first client is 1.
-   **Local Mode Override**: If `LobbyManager.IsLocalMode` is true, we keep the existing `isAI` logic to ensure the AI (which is owned by the Host) gets ID 1 even though its `OwnerClientId` is 0.

### 2. ID-Based Color Assignment
Modify `PlayerColorProvider.cs` to set `playerColor` based on the `PlayerData.playerId.Value` instead of the `isAI` flag.
-   **Rationale**: This decouples visual representation from the "AI vs Human" distinction, making it purely about "Player 1 vs Player 2".

### 3. State Management in LobbyManager
Explicitly reset `IsLocalMode` in `OnPlayButtonClicked` (Online mode) and `OnPlayLocalButtonClicked` (Local mode).
-   **Rationale**: Prevents "state leak" where a previous local session's flag affects a subsequent online session.

### 4. TilePainter Scoring Accuracy
Ensure `TilePainter.GetScores` uses the exact colors assigned (Red for ID 0, Blue for ID 1).
-   **Rationale**: Currently, it might be using hardcoded indices or colors that don't match the new dynamic assignment.

## Risks / Trade-offs

-   **[Risk] Race Condition in OnNetworkSpawn** → Mitigation: `PlayerColorProvider` should read the ID from `PlayerData` during or after its own spawn. Since `playerId` is a `NetworkVariable`, it will eventually sync, but we should ensure the initial assignment on the server is correct.
-   **[Risk] ML-Agents Training Impact** → Mitigation: By keeping the `isAI` logic for local mode, we ensure the agent's ID (which it might use for observations) remains consistent with its previous training environment.
