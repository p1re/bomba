## Why

The user wants to restore the AI training functionality that existed in a previous version of the project. This allows for local training of the Bomberman agent using ML-Agents by simply clicking the "Entrenamiento IA" button in the main menu, facilitating iterative improvement of the agent's behavior.

## What Changes

- **LobbyManager.cs**: Enhance the `OnPlayLocalButtonClicked` method to ensure the environment is correctly primed for a local training session.
- **LocalGameManager.cs**: Refine `SetupLocalGame` to ensure the player and the AI agent are correctly spawned, assigned, and ready for training (e.g., setting the opponent reference).
- **BombermanAgent.cs**: Verify and ensure the agent's observation and reward logic is optimal for the training tasks.
- **Documentation/Scripts**: Provide or integrate the necessary commands to launch the ML-Agents training process (`mlagents-learn`) from the project's virtual environment.

## Capabilities

### New Capabilities
- `ai-training-workflow`: Implements the full cycle of starting a local training session where a human player can interact with or observe an ML-Agent being trained.

### Modified Capabilities
- (none)

## Impact

- `LobbyManager.cs`: Changes to the UI-to-game transition logic.
- `LocalGameManager.cs`: Changes to the local game initialization and agent management.
- `BombermanAgent.cs`: Potential refinements to rewards/observations.
- `bomberman_config.yaml`: Potential adjustments to trainer hyperparameters.
