## Context

The current server setup only supports WebSockets. To support `UnityWebRequest` in Unity, the server must also be capable of responding to standard HTTP GET requests.

## Goals / Non-Goals

**Goals:**
- **Expose HTTP Status**: Create a `/status` endpoint returning game metadata.
- **Support CORS**: Ensure Unity (and browsers) can access the endpoint without Cross-Origin issues.
- **Maintain WebSocket Functionality**: Ensure existing real-time logic continues to work on the same port.

**Non-Goals:**
- **Web-based API**: We are not building a full REST API, only a minimal health check.
- **Port Change**: We will keep everything on port 8080 to minimize configuration changes.

## Decisions

- **Node.js `http` Module**: Use the native `http` module for minimal overhead.
- **CORS Headers**: Manually add `Access-Control-Allow-Origin: *` to responses to ensure Unity clients can read the JSON data from any origin.
- **JSON Metadata**: Return a schema including `online`, `players`, `bombs`, and `uptime`.

## Risks / Trade-offs

- **[Risk] Resource Contention**: Sharing the same server object for HTTP and WebSockets could theoretically lead to performance issues under extreme load.
    - *Mitigation*: The HTTP endpoint is lightweight and only intended for infrequent health checks.
