# nmea-parsing Specification

## Purpose

Parse NMEA 0183 sentences from log files and assemble them into one-second epochs anchored on GGA messages for downstream replay and visualization.

## Requirements

### Requirement: Extract NMEA sentences from log lines

The system SHALL locate and extract NMEA sentences from lines that may contain non-NMEA prefixes before the `$` character. Lines without a valid `$`…`*HH` NMEA sentence SHALL be skipped without error.

#### Scenario: Line with log prefix

- **WHEN** a line contains text before `$GPGGA,...*6A`
- **THEN** the system extracts and parses only the NMEA sentence starting at `$`

#### Scenario: Non-NMEA line

- **WHEN** a line contains no `$` character (e.g. proprietary `$>JI` without standard format, or blank line)
- **THEN** the line is skipped

### Requirement: Parse supported sentence types

The system SHALL parse GGA, GSV, GSA, GST, RMC, VTG, and ZDA sentences with checksum validation. Invalid checksums SHALL cause the sentence to be rejected.

#### Scenario: Valid GGA sentence

- **WHEN** a `$GPGGA` sentence with a valid checksum is parsed
- **THEN** the system extracts UTC time, latitude, longitude, fix quality, satellites used, HDOP, and altitude

#### Scenario: Invalid checksum

- **WHEN** a sentence has an incorrect checksum
- **THEN** the system rejects the sentence and does not include it in epoch data

### Requirement: GGA-anchored epoch assembly

The system SHALL build a timeline of epochs at 1 Hz, one per GGA sentence. All non-GGA sentences between consecutive GGA messages SHALL belong to the earlier GGA's epoch. Orphan sentences appearing before the first GGA in a file SHALL be discarded.

#### Scenario: Standard epoch cycle

- **WHEN** GSA/GSV/GST sentences appear before GGA at time T1, followed by VTG/RMC after GGA at T1, then GGA at T2
- **THEN** all sentences between GGA(T1) and GGA(T2) are grouped into the epoch for T1

#### Scenario: Orphan preamble

- **WHEN** sentences appear before the first GGA in the file
- **THEN** those sentences are discarded and not assigned to any epoch

### Requirement: Multi-talker GSV reassembly

The system SHALL reassemble multi-part GSV messages independently per talker ID (e.g. `$GPGSV`, `$GLGSV`, `$GAGSV`, `$GBGSV`) and merge all completed satellite lists for each epoch.

#### Scenario: Four-constellation GSV in one epoch

- **WHEN** an epoch contains `$GPGSV`, `$GLGSV`, `$GAGSV`, and `$GBGSV` multipart sequences
- **THEN** the epoch's satellite list includes satellites from all four talkers

### Requirement: Position availability detection

An epoch SHALL be considered to have a position when the GGA sentence contains non-empty latitude and longitude fields, regardless of fix quality.

#### Scenario: Invalid fix with coordinates

- **WHEN** GGA fix quality is 0 but latitude and longitude fields are populated
- **THEN** the epoch is marked as having a position with an invalid fix

#### Scenario: No coordinates

- **WHEN** GGA latitude or longitude fields are empty
- **THEN** the epoch is marked as having no position
