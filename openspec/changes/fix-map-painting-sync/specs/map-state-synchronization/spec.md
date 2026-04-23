## ADDED Requirements

### Requirement: Global Map State Persistence
The server SHALL maintain a persistent list of all painted tiles and their respective colors. This list MUST be automatically synchronized with all connected clients.

#### Scenario: Late Joining Player Synchronization
- **WHEN** a player joins a match that is already in progress
- **THEN** the player's client MUST receive the complete list of painted tiles from the server
- **AND** the client MUST render all existing paint on the local Tilemap immediately upon connection.

### Requirement: Immediate Visual Feedback
The system SHALL provide immediate visual feedback for painting events to all clients simultaneously, ensuring that the delay between an explosion and the visible paint is minimized and consistent across the network.

#### Scenario: Instant Paint Update
- **WHEN** the server processes a painting event (e.g., from a bomb explosion)
- **THEN** it MUST broadcast an immediate update to all clients
- **AND** all clients MUST render the paint at the specified coordinates within milliseconds of the event.
