## Context

The repo contains a fresh .NET MAUI template (`NmeaViewer`) and an incomplete `Nmea` class library ported from the GNSS project (GGA and GSV parsers exist; RMC, VTG, GSA, GST, ZDA are missing). Target logs are 1 Hz NMEA streams from handheld GPS devices (e.g. Arrow 100), typically a few KB to 10 MB. This project is intentionally a **MAUI practice exercise** — prefer built-in MAUI controls over custom rendering wherever a suitable control exists. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**

- Windows-only MAUI desktop app for view-and-replay of NMEA log files
- Complete parsing library with GGA-anchored epoch assembly
- Use WebView + Leaflet for track view (MAUI Maps is not available on Windows); standard MAUI layout/input controls for dashboard chrome
- Test coverage migrated from GNSS.Test for parsing code

**Non-Goals:**

- Live serial/TCP/Bluetooth streaming (driver code stays in GNSS project)
- GPX/KML export
- Multi-platform (Android/iOS/Mac) targets
- ENAV sentence support
- Custom-drawn equirectangular track plot (Leaflet handles geographic rendering)

## Decisions

### 1. GGA-anchored epoch model

**Decision:** One epoch per GGA; all non-GGA sentences between consecutive GGAs belong to the earlier epoch. Orphan lines before the first GGA are discarded.

**Rationale:** Matches the 1 Hz burst structure in Arrow logs. Pre-GGA sentences (GSA/GSV/GST) carry timestamps matching the previous GGA, not the upcoming one. Attaching orphan preamble to the first GGA would mix stale GST/GSV data.

**Alternative considered:** Timestamp each sentence independently from its own time field — rejected because GST/GSV often lack independent epoch identity and GGA is the reliable 1 Hz anchor.

### 2. WebView + Leaflet for track visualization

**Decision:** Use a MAUI `WebView` hosting an embedded Leaflet map (`Resources/Raw/map.html`, driven by `TrackMapController`) for the geographic track view. Use standard MAUI controls for all dashboard chrome (`Slider`, `Button`, `Picker`, `CheckBox`, `Label`, `FilePicker`, layouts).

**Rationale:** `Microsoft.Maui.Controls.Maps` with `.UseMauiMaps()` throws `NotImplementedException` on Windows (MAUI 10.0.20). Leaflet in a `WebView` provides pan/zoom, multiple basemap styles, and MapElements-equivalent overlays without a custom equirectangular canvas.

**Alternative considered:** MAUI `Map` control — rejected after verifying it does not render on Windows. Custom `GraphicsView` track plot — rejected in favor of real lat/lon tiles and user pan/zoom.

**Map element mapping:**

| Feature | Implementation |
|---------|----------------|
| Track segments | Leaflet `L.polyline` (one per gap-separated segment) |
| Current position | Leaflet `L.circleMarker` (blue valid / red invalid) |
| Accuracy ring | Leaflet `L.circle` centered on current position, radius from GST |
| Velocity arrow | Leaflet `L.polyline` (2-point line) from current position along heading |
| Fix type label | MAUI `Label` overlay, top-right corner of map `Grid` |
| Basemap style | MAUI `Picker` (Street / Satellite / Hybrid) → JS `setBasemap()` |
| Fit to track | MAUI `Button` + auto-fit on load → JS `fitTrack()` / `fitBounds` |
| Pan & zoom | Leaflet default controls (drag + scroll wheel); max zoom 19 (Street), 21 (Satellite/Hybrid) |

**Basemap sources:** OpenStreetMap (Street), Esri World Imagery (Satellite), Esri imagery + semi-transparent OSM labels (Hybrid). Leaflet and tile requests require network access at runtime.

### 3. GraphicsView only for sky plot

**Decision:** Draw the bullseye satellite sky plot using MAUI `GraphicsView` / `IDrawable`. No built-in MAUI control exists for a polar satellite chart.

**Rationale:** Sky plot is the one visualization that cannot be expressed with standard MAUI layout or Maps controls. `GraphicsView` is still a first-party MAUI control.

**Satellite detail panel:** Use MAUI `Border`/`Grid`/`Label` popup overlay (not custom drawing) when a satellite is tapped.

### 4. Accuracy ring from GST

**Decision:** Use GST horizontal accuracy (same calculation as the production GNSS app) for the `Circle` radius on the map. No circle when GST is absent for that epoch.

**Rationale:** User confirmed production app uses GST; consistent with existing tooling.

### 5. GSV reassembly keyed by talker ID

**Decision:** Replace the static `CompleteSentence` array in `GSV` with per-talker reassembly state (e.g. dictionary keyed by `$GPGSV` / `$GLGSV` / etc.), merged at epoch finalization.

**Rationale:** Current static state breaks when GP/GL/GA/GB GSV sequences interleave within one epoch.

### 6. Speed stored as knots, converted at display

**Decision:** Canonical speed in knots from VTG (primary) or RMC (fallback). Convert to ft/s, mph, m/s, km/h in the UI layer. Display via bound `Label` controls.

### 7. Windows-only target

**Decision:** Trim `NmeaViewer.csproj` to `net10.0-windows10.0.19041.0` only.

**Rationale:** Explicit user requirement; simplifies file picker and rendering.

### 8. Test project structure

**Decision:** New `Nmea.Test` project referencing `Nmea.csproj`. Migrate `NmeaTests.cs` and `GPSDataTests.cs` from `C:\Users\jheck\source\repos\GNSS\GNSS.Test`. Do not migrate driver tests.

**Rationale:** Keeps parsing tests co-located with the slim library the viewer depends on.

## Architecture

```
Nmea/                              NmeaViewer/
├── NmeaSentence.Parse             ├── MainPage (Grid layout — MAUI)
├── GGA, GSV, GSA, GST, ...        ├── WebView + map.html (Leaflet track view)
├── NmeaLineExtractor              ├── TrackMapController (JS bridge)
├── EpochTimelineBuilder           ├── GraphicsView (sky plot only)
└── GPSData, Epoch                 ├── Slider, Button, Picker, CheckBox
                                   ├── PlaybackService (INotifyPropertyChanged)
                                   └── Labels / ScrollView (telemetry, sat detail)
```

**Data flow:** File → `NmeaLineExtractor` → `NmeaSentence.Parse` → `EpochTimelineBuilder` → `List<Epoch>` → `PlaybackService` → MAUI views subscribe to `CurrentEpoch`; track updates via `TrackMapController.UpdateTrackAsync` → Leaflet JS.

## Risks / Trade-offs

- **[Map tiles and CDN on Windows]** → Leaflet JS/CSS load from unpkg CDN; basemap tiles from OSM/Esri require network. Acceptable for a desktop debugging tool.
- **[WebView vs native Map]** → Slightly heavier than MAUI Maps but required on Windows; `TrackMapController` queues JS calls until the WebView navigates.
- **[Velocity arrow on map]** → Implemented as a 2-point Leaflet polyline rather than a custom arrowhead; sufficient for heading indication.
- **[GSV split across GGA boundary]** → Unlikely in 1 Hz Arrow logs; if incomplete GSV at epoch boundary, discard partial sequence for that talker.
- **[Large files ~10 MB]** → Full in-memory index (~10k epochs) is acceptable; no streaming needed for v1.
- **[GLONASS PRN mapping for GSA match]** → GSA uses slot numbers for GLONASS; may need offset (+64) when matching sky plot PRN to GSA active list.

## Migration Plan

1. Complete `Nmea` library and add `Nmea.Test` — no breaking changes to existing code
2. Add `WebView` + Leaflet map (`map.html`, `TrackMapController`); do **not** use `Microsoft.Maui.Controls.Maps` on Windows
3. Replace MAUI template UI with viewer dashboard using MAUI controls
4. Trim csproj to Windows-only
5. Manual test with `Arrow100_SBAS.txt` sample file

## Open Questions

- _(none — GST horizontal accuracy uses √(σLat² + σLong²) from GST fields, matching production GNSS app formula)_
