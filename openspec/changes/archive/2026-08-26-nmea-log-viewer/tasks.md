## 1. Project Setup

- [x] 1.1 Trim `NmeaViewer.csproj` to Windows-only target (`net10.0-windows10.0.19041.0`) and verify the project builds
- [x] 1.2 Verify MAUI Maps on Windows; **result:** `.UseMauiMaps()` throws `NotImplementedException` — use `WebView` + Leaflet instead (no Maps package)
- [x] 1.3 Create `Nmea.Test` project (net10.0, NUnit), add to `NmeaViewer.slnx`, and verify `dotnet test` runs (empty pass)
- [x] 1.4 Migrate `NmeaTests.cs` and `GPSDataTests.cs` from GNSS.Test into `Nmea.Test` (update namespace and project reference) and verify all migrated tests pass

## 2. NMEA Parsing — Sentence Types

- [x] 2.1 Add `NmeaLineExtractor` to strip log prefixes and extract `$…*HH` sentences; add unit tests for prefixed and bare lines
- [x] 2.2 Implement `RMC` parser with tests for speed, course, date, and validity fields
- [x] 2.3 Implement `VTG` parser with tests for speed (knots, km/h) and true heading
- [x] 2.4 Implement `ZDA` parser with tests for date fields
- [x] 2.5 Implement `GSA` parser with tests for fix mode, active PRNs, and DOP values
- [x] 2.6 Implement `GST` parser with tests for accuracy fields; wire `EstimatedAccuracy` on `GPSData`
- [x] 2.7 Register all new sentence types in `NmeaSentence.Parse` and verify existing GGA/GSV tests still pass

## 3. NMEA Parsing — Epoch Assembly

- [x] 3.1 Fix GSV multi-part reassembly to use per-talker state instead of static array; verify multi-talker GSV test passes
- [x] 3.2 Add `Epoch` model and extend `GPSData` with speed, heading, and merged satellite list
- [x] 3.3 Implement `EpochTimelineBuilder` (GGA-anchored, discard orphan preamble) with tests using `Arrow100_SBAS.txt` sample
- [x] 3.4 Add tests for orphan preamble discard, inter-GGA sentence grouping, and no-position epoch detection

## 4. Playback Service

- [x] 4.1 Implement `PlaybackService` with load, play/pause, speed (0.5x/1x/2x/5x), scrub, and skip-no-position toggle
- [x] 4.2 Add speed unit conversion helper (knots → ft/s, mph, m/s, km/h) with unit tests
- [x] 4.3 Expose `CurrentEpoch` change notifications for MAUI data binding

## 5. Track Visualization (WebView + Leaflet)

- [x] 5.1 Add `WebView` + `map.html` (Leaflet) to dashboard; render gap-separated track segments as polylines from epoch lat/lon coordinates
- [x] 5.2 Add current-position circle marker (blue valid / red invalid), GST accuracy circle, and heading polyline arrow on the map
- [x] 5.3 Add fix-type `Label` overlay (top-right corner); basemap `Picker` (Street / Satellite / Hybrid); **Fit Track** button; auto-fit bounds on file load
- [x] 5.4 Enable pan/zoom (Leaflet defaults); max zoom 19 (Street), 21 (Satellite/Hybrid); wire map updates to `PlaybackService.CurrentEpoch`

## 6. Sky Plot Visualization (GraphicsView)

- [x] 6.1 Implement `SkyPlotDrawable` on `GraphicsView` with bullseye elevation/azimuth mapping and SNR color coding
- [x] 6.2 Add tap hit-testing on satellite markers; show MAUI `Border`/`Grid` detail panel (PRN, constellation, elev, az, SNR, used-in-fix)
- [x] 6.3 Wire sky plot to `PlaybackService.CurrentEpoch` and verify satellite positions update on scrub

## 7. Main UI Dashboard (MAUI Controls)

- [x] 7.1 Build dashboard layout with `Grid`/`VerticalStackLayout`: `FilePicker`/`Button` for open, `Button` play/pause, `Picker` speed/units, `CheckBox` skip toggle, `Slider` scrub bar with fix markers
- [x] 7.2 Add telemetry panel in a scrollable `ScrollView` (max height 200): two-column `Grid` of `Label` controls (time, fix, sats, HDOP, speed, heading, lat/lon, alt) with word wrap; raw sentence `Label` below a separator
- [x] 7.3 Integrate WebView map and sky plot `GraphicsView`; verify end-to-end load and replay of `Arrow100_SBAS.txt`

## 8. Final Verification

- [x] 8.1 Run full `dotnet test` on solution and confirm all tests pass
- [x] 8.2 Manual smoke test: open sample log, scrub through session, play at 2x, click satellites, toggle skip-no-position
