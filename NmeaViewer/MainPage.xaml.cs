using Nmea;
using NmeaViewer.Helpers;
using NmeaViewer.Services;
using NmeaViewer.Views;

namespace NmeaViewer;

public partial class MainPage : ContentPage
{
    private readonly PlaybackService _playback = new();
    private readonly SkyPlotDrawable _skyPlotDrawable = new();
    private readonly TimelineDrawable _timelineDrawable = new();
    private readonly TrackMapController _trackMap = new();
    private List<IReadOnlyList<(double Lat, double Lon)>> _trackSegments = [];
    private bool _ignoreSliderChange;
    private bool _fitMapOnNextUpdate;

    public MainPage()
    {
        InitializeComponent();
        _playback.Initialize(Dispatcher);
        _playback.EpochChanged += OnEpochChanged;
        _playback.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlaybackService.IsPlaying))
            {
                PlayPauseButton.Text = _playback.IsPlaying ? "Pause" : "Play";
            }
        };

        _trackMap.Attach(MapWebView);
        SkyPlotView.Drawable = _skyPlotDrawable;
        TimelineView.Drawable = _timelineDrawable;

        var skyTap = new TapGestureRecognizer();
        skyTap.Tapped += OnSkyPlotTapped;
        SkyPlotView.GestureRecognizers.Add(skyTap);

        SpeedPicker.SelectedIndex = 1;
        UnitPicker.SelectedIndex = 0;
        MapStylePicker.SelectedIndex = 0;
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select NMEA log file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".txt", ".log"] },
            }),
        });

        if (result is null)
        {
            return;
        }

        using Stream stream = await result.OpenReadAsync();
        using StreamReader reader = new(stream);
        var lines = new List<string>();
        while (await reader.ReadLineAsync() is { } line)
        {
            lines.Add(line);
        }

        var epochs = EpochTimelineBuilder.BuildFromLines(lines);
        if (epochs.Count == 0)
        {
            await DisplayAlertAsync("No data", "No GGA sentences found in the selected file.", "OK");
            return;
        }

        _playback.LoadEpochs(epochs);
        _ignoreSliderChange = true;
        EpochSlider.Maximum = Math.Max(0, epochs.Count - 1);
        EpochSlider.Value = 0;
        _ignoreSliderChange = false;

        BuildTrackSegments(epochs);
        _fitMapOnNextUpdate = true;
        await UpdateAllViewsAsync();
    }

    private void OnPlayPauseClicked(object? sender, EventArgs e) => _playback.TogglePlayPause();

    private void OnSpeedPickerChanged(object? sender, EventArgs e)
    {
        _playback.PlaybackSpeed = SpeedPicker.SelectedIndex switch
        {
            0 => 0.5,
            2 => 2,
            3 => 5,
            _ => 1,
        };
    }

    private void OnUnitPickerChanged(object? sender, EventArgs e)
    {
        _playback.SpeedUnit = UnitPicker.SelectedIndex switch
        {
            1 => SpeedUnit.FeetPerSecond,
            2 => SpeedUnit.MetersPerSecond,
            3 => SpeedUnit.KilometersPerHour,
            _ => SpeedUnit.MilesPerHour,
        };
        UpdateTelemetry();
    }

    private async void OnMapStyleChanged(object? sender, EventArgs e)
    {
        string basemap = MapStylePicker.SelectedIndex switch
        {
            1 => "satellite",
            2 => "hybrid",
            _ => "street",
        };
        await _trackMap.SetBasemapAsync(basemap);
    }

    private async void OnFitTrackClicked(object? sender, EventArgs e) =>
        await _trackMap.FitTrackAsync();

    private void OnSkipCheckBoxChanged(object? sender, CheckedChangedEventArgs e) =>
        _playback.SkipNoPosition = e.Value;

    private void OnEpochSliderChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_ignoreSliderChange)
        {
            return;
        }

        _playback.CurrentIndex = (int)Math.Round(e.NewValue);
    }

    private void OnEpochChanged() =>
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _ignoreSliderChange = true;
            EpochSlider.Value = _playback.CurrentIndex;
            _ignoreSliderChange = false;
            await UpdateAllViewsAsync();
        });

    private void OnSkyPlotTapped(object? sender, TappedEventArgs e)
    {
        var point = e.GetPosition(SkyPlotView);
        if (point is null)
        {
            return;
        }

        var satellite = _skyPlotDrawable.HitTest(new Microsoft.Maui.Graphics.PointF((float)point.Value.X, (float)point.Value.Y));
        _skyPlotDrawable.SelectedSatellite = satellite;
        SkyPlotView.Invalidate();
        UpdateSatelliteDetail(satellite);
    }

    private async Task UpdateAllViewsAsync()
    {
        await UpdateMapAsync();
        UpdateSkyPlot();
        UpdateTelemetry();
        UpdateTimeline();
    }

    private void BuildTrackSegments(IReadOnlyList<Epoch> epochs)
    {
        List<(double Lat, double Lon)> currentSegment = [];
        List<IReadOnlyList<(double Lat, double Lon)>> segments = [];

        void Flush()
        {
            if (currentSegment.Count >= 2)
            {
                segments.Add(currentSegment.ToList());
            }

            currentSegment = [];
        }

        foreach (Epoch epoch in epochs)
        {
            if (epoch.HasPosition)
            {
                currentSegment.Add((epoch.Data.Latitude, epoch.Data.Longitude));
            }
            else
            {
                Flush();
            }
        }

        Flush();
        _trackSegments = segments;
    }

    private async Task UpdateMapAsync()
    {
        var epoch = _playback.CurrentEpoch;
        if (epoch is null || !epoch.HasPosition)
        {
            FixLabel.Text = "Fix: —";
            await _trackMap.UpdateTrackAsync(_trackSegments, null, _fitMapOnNextUpdate);
        }
        else
        {
            FixLabel.Text = $"Fix: {FixDisplayHelper.GetFixLabel(epoch.Data.Fix)}";
            await _trackMap.UpdateTrackAsync(
                _trackSegments,
                (epoch.Data.Latitude, epoch.Data.Longitude, epoch.HasValidFix,
                    epoch.Data.EstimatedAccuracy, epoch.Data.HeadingDegrees, epoch.Data.SpeedKnots),
                _fitMapOnNextUpdate);
        }

        _fitMapOnNextUpdate = false;
    }

    private void UpdateSkyPlot()
    {
        var epoch = _playback.CurrentEpoch;
        _skyPlotDrawable.Satellites = epoch?.Data.Satellites.ToList() ?? [];
        _skyPlotDrawable.ActivePrns = epoch?.Data.ActivePrns ?? [];
        SkyPlotView.Invalidate();
    }

    private void UpdateTelemetry()
    {
        var epoch = _playback.CurrentEpoch;
        if (epoch is null)
        {
            TimeLabel.Text = "Time: —";
            return;
        }

        var data = epoch.Data;
        TimeLabel.Text = $"Time: {data.UTC:HH:mm:ss}";
        PositionLabel.Text = epoch.HasPosition
            ? $"Lat: {data.Latitude:F6}\nLon: {data.Longitude:F6}\nAlt: {data.Altitude:F1} m"
            : "Position: —";
        double speed = SpeedConverter.ConvertFromKnots(data.SpeedKnots, _playback.SpeedUnit);
        SpeedLabel.Text = $"Speed: {speed:F2} {SpeedConverter.UnitLabel(_playback.SpeedUnit)}";
        HeadingLabel.Text = $"Heading: {data.HeadingDegrees:F1}°";
        FixDetailLabel.Text = $"Fix: {FixDisplayHelper.GetFixLabel(data.Fix)}";
        SatsLabel.Text = $"Sats: {data.SatellitesInUse} used / {data.SatellitesInView} in view";
        HdopLabel.Text = $"HDOP: {data.HDOP:F1}";
        AccuracyLabel.Text = data.EstimatedAccuracy > 0
            ? $"Accuracy: {data.EstimatedAccuracy:F2} m"
            : "Accuracy: —";
        RawSentencesLabel.Text = string.Join(Environment.NewLine, epoch.RawSentences);
    }

    private void UpdateTimeline()
    {
        _timelineDrawable.Epochs = _playback.Epochs;
        _timelineDrawable.CurrentIndex = _playback.CurrentIndex;
        TimelineView.Invalidate();
    }

    private void UpdateSatelliteDetail(Satellite? satellite)
    {
        SatelliteDetailLayout.Children.Clear();
        if (satellite is null)
        {
            SatelliteDetailPanel.IsVisible = false;
            return;
        }

        bool usedInFix = _playback.CurrentEpoch?.Data.ActivePrns.Contains(satellite.PRN) == true;
        SatelliteDetailLayout.Children.Add(new Label { Text = $"PRN: {satellite.PRN}", TextColor = Colors.White });
        SatelliteDetailLayout.Children.Add(new Label { Text = $"Constellation: {satellite.Constellation}", TextColor = Colors.White });
        SatelliteDetailLayout.Children.Add(new Label { Text = $"Elevation: {satellite.Elevation}°", TextColor = Colors.White });
        SatelliteDetailLayout.Children.Add(new Label { Text = $"Azimuth: {satellite.Azimuth}°", TextColor = Colors.White });
        SatelliteDetailLayout.Children.Add(new Label { Text = $"SNR: {(satellite.HasSignal ? $"{satellite.SNR} dBHz" : "—")}", TextColor = Colors.White });
        SatelliteDetailLayout.Children.Add(new Label { Text = $"Used in fix: {(usedInFix ? "Yes" : "No")}", TextColor = Colors.White });
        SatelliteDetailPanel.IsVisible = true;
    }
}
