## 1. UI and Transition Refinement

- [x] 1.1 Verify the binding of the "ENTRENAMIENTO IA" button in `LobbyManager.cs` and ensure it correctly sets `LobbyManager.IsLocalMode`.
- [x] 1.2 Enhance `LobbyManager.OnPlayLocalButtonClicked` to log the start of a training session and handle scene loading robustly.

## 2. Local Game Setup

- [x] 2.1 Refine `LocalGameManager.SetupLocalGame` to ensure the AI agent correctly identifies the human player as its `opponent`.
- [x] 2.2 Ensure `LocalGameManager` handles the spawning of the AI player prefab with the correct `BehaviorParameters` (checking if it's set to "Default" for training).

## 3. Agent Lifecycle and Rewards

- [x] 3.1 Update `BombermanAgent.OnEpisodeBegin` to ensure all dynamic objects (bombs, explosions) are cleared and players are reset to their spawn points.
- [x] 3.2 Add a check in `BombermanAgent` to notify the user via console if the agent is not connected to an external trainer.
- [x] 3.3 Verify that `NotifyDeath` in `LocalGameManager` correctly calls `EndEpisode()` on the agent with appropriate win/loss rewards.

## 4. Training Workflow Verification

- [x] 4.1 Document the specific command to start training: `.\mlagents_env\Scripts\activate; mlagents-learn bomberman_config.yaml --run-id=Bomberman_V4`.
- [x] 4.2 Perform a test run in the Unity Editor to verify that the "Entrenamiento IA" button triggers the training loop correctly when `mlagents-learn` is running.
