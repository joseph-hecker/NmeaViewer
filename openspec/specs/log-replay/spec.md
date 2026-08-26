# log-replay Specification

## Purpose

Allow users to open NMEA log files and replay epoch data with playback controls for debugging handheld GPS device output.

## Requirements

### Requirement: Open log file

The system SHALL allow the user to select a `.txt` or `.log` file from disk and load it into an epoch timeline.

#### Scenario: Successful file load

- **WHEN** the user selects a valid NMEA log file
- **THEN** the system parses the file, builds the epoch timeline, and displays the first epoch

#### Scenario: Empty or unparseable file

- **WHEN** the user selects a file with no GGA sentences
- **THEN** the system displays an error indicating no replayable data was found

### Requirement: Playback speed control

The system SHALL support automatic playback at 0.5x, 1x, 2x, and 5x speed relative to the original 1 Hz epoch rate.

#### Scenario: Play at 1x

- **WHEN** the user starts playback at 1x
- **THEN** the system advances one epoch per second

#### Scenario: Play at 5x

- **WHEN** the user starts playback at 5x
- **THEN** the system advances five epochs per second

### Requirement: Manual scrubbing

The system SHALL provide a scrollbar/playhead that the user can drag to any epoch. All views SHALL update immediately to reflect the selected epoch.

#### Scenario: Scrub to middle of log

- **WHEN** the user drags the playhead to the middle of the timeline
- **THEN** all panels display data for that epoch without requiring playback

### Requirement: Skip no-position epochs

The system SHALL provide a toggle to skip epochs without position data during automatic playback. Manual scrubbing SHALL still allow navigating to no-position epochs regardless of toggle state.

#### Scenario: Skip enabled during playback

- **WHEN** skip-no-position is enabled and playback encounters an epoch without coordinates
- **THEN** playback advances past that epoch without pausing

#### Scenario: Skip disabled during playback

- **WHEN** skip-no-position is disabled and playback encounters an epoch without coordinates
- **THEN** playback stops on that epoch and displays its available data

### Requirement: Scrub bar fix indicators

The scrub bar SHALL display markers indicating epochs with a valid fix (fix quality ≠ 0) differently from epochs with an invalid fix or no position.

#### Scenario: Valid fix marker

- **WHEN** an epoch has a valid fix
- **THEN** the scrub bar shows a distinct marker for that epoch tick

#### Scenario: Invalid or no-position marker

- **WHEN** an epoch has an invalid fix or no position
- **THEN** the scrub bar shows a different marker (or no valid-fix marker) for that epoch tick

### Requirement: Speed unit selection

The system SHALL display speed in one of four user-selectable units: ft/s, mph, m/s, or km/h. Speed values SHALL be converted from the knots value stored in NMEA sentences.

#### Scenario: Display speed in mph

- **WHEN** the user selects mph and the current epoch has speed 0.10 knots
- **THEN** the displayed speed is approximately 0.12 mph

### Requirement: Raw sentence display

The system SHALL display the raw NMEA sentence(s) for the current epoch, including the GGA anchor sentence. Telemetry and raw sentences SHALL be shown in a scrollable panel that does not overlap labels.

#### Scenario: View raw data at playhead

- **WHEN** the user scrubs to an epoch
- **THEN** the raw GGA sentence and associated sentences for that epoch are visible in the scrollable telemetry area
