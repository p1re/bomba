## ADDED Requirements

### Requirement: Trigger Local AI Training Session
The system MUST allow the user to initiate a local AI training session from the main menu by clicking the "ENTRENAMIENTO IA" button. This SHALL transition the game state to "Local Mode" and load the training scene.

#### Scenario: User clicks ENTRENAMIENTO IA button
- **WHEN** the user is in the Lobby Menu and clicks the "ENTRENAMIENTO IA" button
- **THEN** the `LobbyManager` sets `IsLocalMode` to true, starts a local network host, and loads the "Bomberman" scene.

### Requirement: Setup Local Game with AI Agent
The system MUST correctly instantiate and configure an AI agent and a human player (as the opponent) when the training scene is loaded in Local Mode.

#### Scenario: Local Training Scene Loaded
- **WHEN** the "Bomberman" scene is loaded and `IsLocalMode` is true
- **THEN** the `LocalGameManager` instantiates the AI player prefab and assigns the local human player as the AI agent's opponent.

### Requirement: Episode Lifecycle Management
The system MUST handle the beginning of each training episode by resetting player positions and clearing dynamic objects (bombs, explosions) to ensure a clean state for the agent's next trial.

#### Scenario: Training Episode Begin
- **WHEN** a new training episode starts in `BombermanAgent`
- **THEN** both the agent and the opponent are teleported to their starting positions, and any existing bombs or explosions are destroyed.

### Requirement: Real-time Feedback and Rewards
The system MUST provide real-time rewards to the AI agent based on its actions, including wall destruction, item pickup, and survival, to guide its learning process.

#### Scenario: Agent destroys a wall
- **WHEN** the AI agent's bomb destroys a destructible wall
- **THEN** the `BombermanAgent` is awarded +0.5 reward points.

#### Scenario: Agent picks up an item
- **WHEN** the AI agent moves over a power-up item
- **THEN** the `BombermanAgent` is awarded +0.3 reward points.

### Requirement: Training Environment Support
The system MUST support the standard ML-Agents training workflow, allowing the Unity environment to communicate with an external Python trainer (`mlagents-learn`).

#### Scenario: Connection to Python Trainer
- **WHEN** the Unity environment is started with the `mlagents-learn` command active in a terminal
- **THEN** the `BombermanAgent` successfully connects to the trainer and starts receiving actions and sending observations.
