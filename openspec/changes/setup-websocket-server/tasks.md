## 1. Project Initialization

- [x] 1.1 Create the `Server` directory at the project root.
- [x] 1.2 Initialize a new Node.js project inside the `Server` directory using `npm init -y`.
- [x] 1.3 Install the `ws` library and `uuid` for client identification.

## 2. Server Implementation

- [x] 2.1 Create `Server/index.js` and set up a basic WebSocket server listening on port 8080.
- [x] 2.2 Implement client connection and disconnection logging.
- [x] 2.3 Implement the message broadcast logic to relay events to all connected clients.
- [x] 2.4 Add in-memory state tracking for players and bombs.

## 3. Testing and Validation

- [x] 3.1 Create a simple test client (e.g., `Server/test_client.js`) to verify the server can receive and broadcast messages.
- [x] 3.2 Verify that multiple clients can connect and receive broadcasts from each other.
- [x] 3.3 Verify the initial state message is sent to newly connected clients.
