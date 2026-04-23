## Context

The Bomberman project currently has a basic structure for ML-Agents training, but the "Entrenamiento IA" button's workflow needs refinement to ensure a smooth transition from the menu to a functional training session. The environment includes a specialized `BombermanAgent` and a `LocalGameManager` that handles the spawning of the AI.

## Goals / Non-Goals

**Goals:**
- **Seamless Transition**: Ensure that clicking the "Entrenamiento IA" button correctly sets up the environment for training without additional manual steps in the scene.
- **Robust Player/AI Setup**: Properly spawn the local player and the AI agent, ensuring they are correctly referenced for observations (like distance to opponent).
- **Automated Resets**: Refine the episode begin logic to ensure a clean state for every training trial.
- **Standard Workflow**: Provide a clear path for the user to start the Python-side training using the existing `mlagents_env`.

**Non-Goals:**
- **In-process Training**: Training will still require the external `mlagents-learn` process; we are not implementing a C#-based trainer.
- **Multi-agent Training**: This design focuses on a 1v1 (Human vs. AI) local training session.

## Decisions

- **Use IsLocalMode for Environment Selection**: We will continue using the `LobbyManager.IsLocalMode` flag to trigger the `LocalGameManager`'s special setup.
    - *Rationale*: This leverages the existing architecture and clearly separates local training logic from networked multiplayer.
- **Dedicated Teleportation and Reset Logic**: We will refine the `BombermanAgent.OnEpisodeBegin` to ensure it handles object destruction and player positioning safely within the Netcode context.
    - *Rationale*: Proper cleanup is essential to prevent cumulative errors in the physics engine and observation data over long training sessions.
- **Explicit Opponent Assignment**: The `LocalGameManager` will be responsible for finding the local player and assigning it as the AI's opponent.
    - *Rationale*: This ensures the agent's distance-based observations and rewards are always accurate.

## Risks / Trade-offs

- **[Risk] Python Side Not Running**: The agent will remain stationary if the Python trainer isn't running.
    - *Mitigation*: The `BombermanAgent` will log a warning if it cannot connect to a brain, and documentation will clearly state the requirement to start `mlagents-learn` first.
- **[Risk] Netcode Authority Conflicts**: Moving objects locally in a networked environment can cause issues.
    - *Mitigation*: We will use the `NetworkTransform.Teleport` method where applicable and ensure the server (host) has authority during local mode.
