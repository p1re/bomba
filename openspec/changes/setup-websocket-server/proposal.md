## Why

The project currently lacks a centralized server to manage game state, player synchronization, and monitoring outside of the Unity Networking (Netcode) environment. A separate WebSocket server will allow for external monitoring, persistence, and potentially a lighter-weight synchronization layer that can be integrated with other web services.

## What Changes

A new root directory named `Server` will be created. This directory will house a Node.js application that runs a WebSocket server on port 8080. This server will be responsible for receiving and broadcasting game events (player connections, bomb placements, item pickups) in real-time.

## Capabilities

### New Capabilities
- `websocket-server`: A Node.js based server using the `ws` library to handle real-time communication.
- `game-event-handling`: A messaging protocol to process events like player join/leave, bomb spawning, and game status updates.

### Modified Capabilities
- None: This is a new standalone component that does not modify existing requirements of the Unity project.

## Impact

- **New Files**: `Server/package.json`, `Server/index.js`, `Server/node_modules/`.
- **Infrastructure**: Port 8080 must be available on the host machine.
- **Unity**: No changes to Unity code are planned for this specific change, although future tasks will involve connecting the Unity client to this server.
