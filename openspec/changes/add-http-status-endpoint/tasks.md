## 1. Implementation

- [x] 1.1 Import the `http` module in `Server/index.js`.
- [x] 1.2 Create an `http.Server` that handles the `/status` route with CORS headers.
- [x] 1.3 Refactor the `WebSocketServer` to use the new `http.Server` instead of its own port.
- [x] 1.4 Start the server using `server.listen(8080)`.

## 2. Validation

- [x] 2.1 Verify the `/status` endpoint via a browser at `http://localhost:8080/status`.
- [x] 2.2 Verify that the WebSocket test client still connects and functions correctly.
