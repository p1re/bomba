## ADDED Requirements

### Requirement: WebSocket Server Initialization
The system MUST initialize a WebSocket server listening on port 8080.

#### Scenario: Server Start
- **WHEN** the `node index.js` command is executed in the `Server` directory
- **THEN** the server starts and logs "WebSocket Server started on port 8080"

### Requirement: Client Connection Management
The server MUST handle multiple client connections and track their lifecycle.

#### Scenario: Client Connects
- **WHEN** a client connects to `ws://localhost:8080`
- **THEN** the server logs "Client connected" and assigns a unique ID to the connection.

#### Scenario: Client Disconnects
- **WHEN** an active client closes its connection
- **THEN** the server logs "Client disconnected" and removes the client from its active registry.

### Requirement: Broadcast Message Handling
The server MUST be able to receive a message from one client and broadcast it to all other connected clients.

#### Scenario: Broadcast Event
- **WHEN** a client sends a JSON message with `{ "type": "player_moved", "payload": { ... } }`
- **THEN** the server receives the message and sends the exact same JSON to all other connected clients.

### Requirement: Game State Snapshot
The server SHOULD provide the current game state (players, bombs) to newly connected clients.

#### Scenario: New Client Initial State
- **WHEN** a new client connects
- **THEN** the server sends an `initial_state` message containing the current list of active players and their last known positions.
