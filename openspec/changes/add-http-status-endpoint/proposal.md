## Why

A standard requirement for web-connected Unity applications is to verify server connectivity via `UnityWebRequest`. Adding an HTTP endpoint allows for simple health checks and metadata retrieval (like player count) without initiating a full WebSocket handshake.

## What Changes

The existing `Server/index.js` will be modified to include a built-in Node.js HTTP server. This server will handle a new `/status` route while also serving as the host for the existing WebSocket server.

## Capabilities

### New Capabilities
- `http-status-endpoint`: A GET route at `/status` that returns JSON metadata about the current game state.

### Modified Capabilities
- `websocket-server`: Transition from a standalone WebSocket server to one hosted within an HTTP server on port 8080.

## Impact

- **Server/index.js**: Code will be refactored to use `http.createServer`.
- **Port 8080**: Will now respond to both HTTP and WebSocket requests.
