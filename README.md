# NmeaViewer

Windows desktop app for replaying and inspecting NMEA log files from handheld GPS receivers. Open a saved `.txt` or `.log` file, scrub through 1 Hz epochs, and visualize the track on a map, satellite sky plot, and telemetry panel.

## Requirements

- Windows 10 (17763+) or Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Internet access for map tiles (Leaflet loads from CDN; basemaps use OpenStreetMap and Esri)

## Build and run

```powershell
dotnet build NmeaViewer/NmeaViewer.csproj
dotnet run --project NmeaViewer/NmeaViewer.csproj
```

Run tests:

```powershell
dotnet test
```



## Usage

1. **Open File** — Select a NMEA log (`.txt` or `.log`). The file is parsed into epochs anchored on each GGA sentence at ~1 Hz.
2. **Scrub** — Drag the timeline slider to jump to any epoch. The colored strip above the slider marks valid-fix epochs.
3. **Play / Pause** — Replay epochs automatically. Choose speed (0.5x, 1x, 2x, 5x) from the toolbar.
4. **Skip no-position** — When checked, playback skips epochs without lat/lon. You can still scrub to those epochs manually.
5. **Map** — Pan (drag) and zoom (scroll wheel). Pick **Street**, **Satellite**, or **Hybrid** basemap. Click **Fit Track** to re-center on the full path. The fix-type badge appears in the top-right corner.
6. **Sky plot** — Bullseye view of satellites for the current epoch. Tap a satellite to see PRN, constellation, elevation, azimuth, SNR, and whether it is used in the fix (from GSA).
7. **Telemetry** — Scroll the panel below the map for time, fix, position, speed, heading, HDOP, accuracy, and raw NMEA sentences for the current epoch.
8. **Speed units** — Display speed as mph, ft/s, m/s, or km/h (converted from knots in VTG/RMC).



### Expected log format

Logs are plain text with one or more NMEA sentences per line. Lines may include a prefix before `$`; only standard `$…*HH` sentences are extracted. Typical 1 Hz cycles look like:

```
GSA / GSV / GST  →  GPGGA  →  GPVTG / GPZDA / GPRMC
```

Multi-constellation GSV (`GP`, `GL`, `GA`, `GB`) is supported. Sentences before the first GGA in a file are discarded.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  NmeaViewer (MAUI, Windows)                                 │
│  ┌─────────────┐  ┌──────────────────┐  ┌───────────────┐  │
│  │ MainPage    │  │ PlaybackService  │  │ TrackMap      │  │
│  │ (dashboard) │◄─┤ play/scrub/skip  │  │ Controller    │  │
│  └──────┬──────┘  └────────▲─────────┘  │ (WebView +    │  │
│         │                   │            │  Leaflet)     │  │
│  ┌──────┴──────┐            │            └───────────────┘  │
│  │ GraphicsView│            │                               │
│  │ sky + scrub │            │                               │
│  └─────────────┘            │                               │
└─────────────────────────────┼───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│  Nmea (class library)                                       │
│  NmeaLineExtractor → NmeaSentence.Parse → EpochTimelineBuilder│
│  Parsers: GGA, GSV, GSA, GST, RMC, VTG, ZDA                 │
│  Models: Epoch, GPSData, Satellite                          │
└─────────────────────────────────────────────────────────────┘
```



### Projects


| Project        | Role                                                  |
| -------------- | ----------------------------------------------------- |
| **NmeaViewer** | MAUI UI — map, sky plot, playback controls, telemetry |
| **Nmea**       | NMEA parsing and GGA-anchored epoch assembly          |
| **Nmea.Test**  | Unit tests for parsers, epoch builder, and helpers    |




### Data flow

1. User opens a file → lines read from disk.
2. `NmeaLineExtractor` strips log prefixes and finds `$…*HH` sentences.
3. `NmeaSentence.Parse` validates checksums and dispatches to sentence parsers.
4. `EpochTimelineBuilder` groups sentences into one epoch per GGA; non-GGA sentences between consecutive GGAs belong to the earlier epoch.
5. `PlaybackService` holds the timeline and drives the current index (manual scrub or timer-based playback).
6. Views subscribe to epoch changes:
  - **TrackMapController** pushes track segments and current position to Leaflet (`Resources/Raw/map.html`).
  - **SkyPlotDrawable** / **TimelineDrawable** render via MAUI `GraphicsView`.
  - Labels show telemetry and raw sentences.



### Epoch model

Each epoch is anchored on a GGA and merges related sentences (GSV across talkers, GSA active PRNs, GST accuracy, VTG/RMC speed and heading). Position is available when GGA lat/lon fields are populated, regardless of fix quality. Track gaps (epochs without position) produce separate map polylines.

## Specifications

Behavior is documented in `openspec/specs/`:

- `nmea-parsing` — sentence extraction, parsing, epoch assembly
- `log-replay` — file open, playback, scrub bar, units, raw display
- `track-visualization` — map track, basemaps, pan/zoom, accuracy ring
- `sky-plot` — bullseye plot, SNR colors, satellite detail

