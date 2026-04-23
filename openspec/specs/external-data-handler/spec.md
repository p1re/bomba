# external-data-handler Specification

## Purpose
TBD - created by archiving change bombersplash. Update Purpose after archive.
## Requirements
### Requirement: Match Result Reporting
The system SHALL use UnityWebRequest to send match telemetry or results to an external web service.

#### Scenario: Reporting Winner to Web Service
- **WHEN** a match concludes and a winner is determined
- **THEN** the system sends a POST request via UnityWebRequest containing the match statistics to the configured endpoint

