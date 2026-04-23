## ADDED Requirements

### Requirement: HTTP Status Endpoint
The system MUST respond to GET requests on the `/status` route.

#### Scenario: Successful Status Request
- **WHEN** a client makes a GET request to `http://localhost:8080/status`
- **THEN** the server returns a 200 OK status code and a JSON object containing `online: true`.

### Requirement: CORS Compatibility
The HTTP response MUST include CORS headers to allow requests from any origin.

#### Scenario: Header Presence
- **WHEN** the `/status` endpoint is requested
- **THEN** the response headers include `Access-Control-Allow-Origin: *`.
