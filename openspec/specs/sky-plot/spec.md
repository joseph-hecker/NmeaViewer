# sky-plot Specification

## Purpose

Display satellite sky view from GSV sentence data on a bullseye plot to help debug satellite tracking behavior of handheld GPS devices.

## Requirements

### Requirement: Bullseye sky plot

The system SHALL render a bullseye (polar) plot with the horizon at the outer edge and zenith at the center. Azimuth SHALL be measured clockwise from north with north at the top. Elevation SHALL map to radius (0° at edge, 90° at center).

#### Scenario: Satellites in view

- **WHEN** the current epoch contains GSV data with satellites at known elevation and azimuth
- **THEN** each satellite is plotted at the correct position on the bullseye

### Requirement: SNR color coding

Satellites with SNR values SHALL be color-coded by signal strength: green for SNR ≥ 40 dBHz, yellow for 30–39 dBHz, orange/red for below 30 dBHz. Satellites without SNR (not actively tracked) SHALL be shown as gray hollow markers.

#### Scenario: Strong signal satellite

- **WHEN** a satellite has SNR 49 dBHz
- **THEN** it is rendered in green on the sky plot

#### Scenario: Untracked satellite

- **WHEN** a satellite has elevation and azimuth but no SNR value
- **THEN** it is rendered as a gray hollow marker

### Requirement: Constellation identification

Each satellite SHALL be identifiable by constellation derived from the GSV talker ID (GPS, GLONASS, Galileo, BeiDou).

#### Scenario: Mixed constellation epoch

- **WHEN** the epoch contains `$GPGSV` and `$GLGSV` satellites
- **THEN** both GPS and GLONASS satellites appear on the same sky plot with distinguishable labels or colors

### Requirement: Satellite detail on click

The system SHALL display a detail panel when the user clicks a satellite marker, showing PRN, constellation, elevation, azimuth, and SNR.

#### Scenario: Click satellite

- **WHEN** the user clicks a satellite marker on the sky plot
- **THEN** a panel shows PRN, constellation name, elevation in degrees, azimuth in degrees, and SNR in dBHz

### Requirement: Used-in-fix indication

When GSA data is available for the epoch, the satellite detail panel SHALL indicate whether the satellite's PRN appears in the active satellite list.

#### Scenario: Satellite used in fix

- **WHEN** the user clicks a satellite whose PRN is listed in the epoch's GSA active satellites
- **THEN** the detail panel indicates the satellite is used in the current fix
