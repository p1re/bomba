## Context

The Bomberman project currently uses Unity Netcode for GameObjects for its networking layer. To provide a separate monitoring and synchronization layer, we are adding a standalone Node.js server that uses WebSockets to communicate with both the Unity game client and potentially other web-based clients.

## Goals / Non-Goals

**Goals:**
- **Centralized Event Hub**: Provide a server that can receive and broadcast game events in real-time.
- **WebSocket Protocol**: Use the `ws` library for high-performance, low-latency communication on port 8080.
- **Player & Game Monitoring**: Track connections and disconnections, and maintain a simple in-memory state of the current game (e.g., active bombs, player positions).
- **Extensibility**: Design a message schema that can be easily extended to support new game features.

**Non-Goals:**
- **Authoritative Server**: This server will *not* validate game logic initially. It acts as a relay and state monitor.
- **Unity Code Modification**: In this phase, we will *not* modify any Unity scripts. The client-side implementation for connecting to this server will be handled in a later change.
- **Persistent Database**: All data will be kept in memory for this initial version.

## Decisions

- **Node.js with `ws` library**:
    - *Rationale*: Node.js is excellent for real-time applications, and `ws` is a widely-used, lightweight, and fast WebSocket library.
    - *Alternatives*: Socket.io was considered, but `ws` provides a more direct and efficient WebSocket implementation which is easier to integrate with Unity's `System.Net.WebSockets`.
- **JSON Messaging Protocol**:
    - *Rationale*: Easy to parse in both JavaScript and C# (using JsonUtility or Newtonsoft.Json).
    - *Format*: `{ "type": "event_name", "payload": { ... } }`
- **Port 8080**:
    - *Rationale*: Standard development port for local servers, unlikely to conflict with standard web ports (80/443).

## Risks / Trade-offs

- **[Risk] Scalability**: Node.js is single-threaded; while it handles many connections well, CPU-intensive tasks could block the event loop.
    - *Mitigation*: Keep message processing logic minimal and offload any heavy computation.
- **[Risk] Synchronization Lag**: Since the server is not authoritative, the reported state might slightly lag behind the Unity game state.
    - *Mitigation*: Use timestamps in messages and treat the server state as "eventually consistent" for monitoring purposes.
- **[Risk] No Authentication**: For this initial version, anyone can connect.
    - *Mitigation*: Add basic token-based authentication in a future iteration.
