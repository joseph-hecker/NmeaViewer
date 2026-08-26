# track-visualization Specification

## Purpose

Visualize GPS position tracks from replayed NMEA epochs on an interactive north-up map for spatial debugging of handheld device output.

## Requirements

### Requirement: North-up track plot

The system SHALL render a north-up map displaying epoch positions at their geographic latitude and longitude. North SHALL point toward the top of the map.

#### Scenario: Load session with movement

- **WHEN** a log file with multiple positioned epochs is loaded
- **THEN** the map displays polylines connecting epoch positions at their lat/lon coordinates with north at the top

### Requirement: Gap-separated polylines

The system SHALL break the track polyline into separate segments when consecutive positioned epochs are separated by one or more epochs without position data.

#### Scenario: Position gap in log

- **WHEN** positioned epochs exist before and after a run of no-position epochs
- **THEN** the track view shows two separate polyline segments with no line connecting across the gap

### Requirement: Position marker with fix indication

The system SHALL display the current epoch's position as a dot. A valid fix (fix quality ≠ 0) SHALL be shown as a blue dot. An invalid fix with coordinates present SHALL be shown as a red dot.

#### Scenario: Valid fix at playhead

- **WHEN** the current epoch has fix quality 1 and coordinates
- **THEN** a blue dot marks the position on the track view

#### Scenario: Invalid fix at playhead

- **WHEN** the current epoch has fix quality 0 and coordinates
- **THEN** a red dot marks the position on the track view

### Requirement: GST accuracy ring

When GST data is available for the current epoch, the system SHALL draw an accuracy ring around the position dot scaled from the GST estimated accuracy. When GST is not available, no accuracy ring SHALL be drawn.

#### Scenario: GST present

- **WHEN** the current epoch includes a valid GST sentence
- **THEN** a ring centered on the position dot reflects the estimated horizontal accuracy from GST

#### Scenario: GST absent

- **WHEN** the current epoch has no GST data
- **THEN** the position dot is shown without an accuracy ring

### Requirement: Fix type label

The system SHALL display the current fix type (e.g. Invalid, GPS, DGPS, RTK Fix, RTK Float) in a top-right corner overlay on the track view, clear of map zoom controls.

#### Scenario: RTK fix displayed

- **WHEN** the current epoch GGA fix quality is 4
- **THEN** the overlay shows "RTK Fix (4)" in the top-right corner of the map

### Requirement: Interactive pan and zoom

The track view SHALL support user panning and zooming. Maximum zoom SHALL be 19 for Street basemap and 21 for Satellite and Hybrid basemaps.

#### Scenario: User zooms in on track

- **WHEN** the user scrolls the mouse wheel over the map on Satellite basemap
- **THEN** the map zooms in up to level 21

#### Scenario: User pans away from playhead

- **WHEN** the user drags the map
- **THEN** the view pans without resetting until the user clicks **Fit Track** or loads a new file

### Requirement: Basemap selection

The system SHALL provide a basemap picker with at least Street (OpenStreetMap), Satellite (Esri World Imagery), and Hybrid (satellite imagery with street labels) options.

#### Scenario: Switch to satellite view

- **WHEN** the user selects Satellite from the basemap picker
- **THEN** the map displays Esri satellite imagery tiles

### Requirement: Fit track

The system SHALL provide a control to fit the map view to the full loaded track bounds. On initial file load, the map SHALL automatically fit to the track.

#### Scenario: Fit track button

- **WHEN** the user clicks **Fit Track** after panning away
- **THEN** the map zooms and centers to show all track segments with padding

#### Scenario: Auto-fit on load

- **WHEN** a log file with positioned epochs is loaded
- **THEN** the map fits to the track bounds without manual interaction

### Requirement: Velocity arrow

The system SHALL display a vector arrow at the current position indicating heading direction and scaled by speed. At zero speed, the arrow SHALL be hidden or shown as a minimal stub.

#### Scenario: Moving epoch

- **WHEN** the current epoch has speed 0.10 kn and heading 99.7°
- **THEN** an arrow at the position dot points approximately east-of-north at a length proportional to speed

#### Scenario: Stationary epoch

- **WHEN** the current epoch has zero speed
- **THEN** no full-length velocity arrow is displayed
