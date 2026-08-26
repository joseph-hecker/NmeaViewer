## Why

Handheld GPS devices emit NMEA sentence streams that are typically logged to text files for post-session analysis. There is no dedicated tool in this project to open those logs, replay them as if live, and visually inspect position, satellite sky view, and telemetry. A Windows MAUI viewer will make debugging NMEA output from devices like the Arrow 100 practical without relying on external tools.

## What Changes

- Add a Windows-only MAUI application that opens NMEA log/text files and replays them at 1 Hz (with speed control)
- Complete the `Nmea` parsing library: GGA, GSV, GSA, GST, RMC, VTG, ZDA, line-prefix stripping, and GGA-anchored epoch assembly
- Add `Nmea.Test` project with parsing tests migrated from the GNSS project
- Build UI with MAUI controls: `WebView` + Leaflet for interactive track map (Street/Satellite/Hybrid basemaps, pan/zoom); standard layout controls for dashboard/playback; `GraphicsView` only for the bullseye sky plot (no MAUI equivalent)
- Add playback scrub bar with valid-fix markers, skip-no-position toggle, and scrollable telemetry panel (two-column grid, raw sentences below)
- Trim `NmeaViewer` to Windows-only target framework

## Capabilities

### New Capabilities

- `nmea-parsing`: NMEA sentence parsing, checksum validation, line extraction, and GGA-anchored epoch timeline building
- `log-replay`: File selection, playback control (0.5x–5x), scrub bar, and epoch navigation
- `track-visualization`: Interactive north-up map (Leaflet in WebView) with gap-separated polylines, basemap selection, pan/zoom, fit-to-track, fix indicator overlay, GST accuracy ring, and velocity arrow
- `sky-plot`: Bullseye satellite view with SNR color coding and clickable satellite detail panel

### Modified Capabilities

_(none — greenfield project)_

## Impact

- **Nmea/** — extend parsers, fix GSV reassembly, add epoch loader
- **Nmea.Test/** — new test project (NmeaTests, GPSDataTests from GNSS.Test)
- **NmeaViewer/** — replace template UI with viewer dashboard; Windows-only csproj; `WebView` + `Resources/Raw/map.html` + `TrackMapController` for track view (no `Microsoft.Maui.Controls.Maps`); `GraphicsView` for sky plot only
- **NmeaViewer.slnx** — add Nmea.Test project reference
