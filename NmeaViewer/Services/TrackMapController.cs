using System.Text.Json;

namespace NmeaViewer.Services;

public class TrackMapController
{
    private WebView? _webView;
    private bool _isReady;
    private string _pendingBasemap = "street";
    private string? _pendingUpdateJson;
    private bool _pendingFitBounds;

    public void Attach(WebView webView)
    {
        _webView = webView;
        _isReady = false;
        _webView.Navigated += OnNavigated;

        Task.Run(async () =>
        {
            string html = await LoadMapHtmlAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _webView.Source = new HtmlWebViewSource { Html = html };
            });
        });
    }

    public async Task SetBasemapAsync(string basemap)
    {
        _pendingBasemap = basemap;
        if (!_isReady || _webView is null)
        {
            return;
        }

        await _webView.EvaluateJavaScriptAsync($"setBasemap('{basemap}')");
    }

    public async Task UpdateTrackAsync(
        IEnumerable<IEnumerable<(double Lat, double Lon)>> segments,
        (double Lat, double Lon, bool ValidFix, double AccuracyM, double Heading, double SpeedKnots)? current,
        bool fitBounds)
    {
        var payload = new
        {
            segments = segments.Select(s => s.Select(p => new { lat = p.Lat, lon = p.Lon }).ToList()).ToList(),
            current = current is null ? null : new
            {
                lat = current.Value.Lat,
                lon = current.Value.Lon,
                validFix = current.Value.ValidFix,
                accuracyM = current.Value.AccuracyM,
                heading = current.Value.Heading,
                speedKnots = current.Value.SpeedKnots,
            },
            fitBounds,
        };

        _pendingUpdateJson = JsonSerializer.Serialize(payload);
        _pendingFitBounds = fitBounds;

        if (!_isReady || _webView is null)
        {
            return;
        }

        await _webView.EvaluateJavaScriptAsync($"updateTrack({_pendingUpdateJson})");
    }

    public async Task FitTrackAsync()
    {
        if (!_isReady || _webView is null)
        {
            return;
        }

        await _webView.EvaluateJavaScriptAsync("fitTrack()");
    }

    private async void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (_webView is null || e.Result != WebNavigationResult.Success)
        {
            return;
        }

        _isReady = true;
        await SetBasemapAsync(_pendingBasemap);

        if (_pendingUpdateJson is not null)
        {
            await _webView.EvaluateJavaScriptAsync($"updateTrack({_pendingUpdateJson})");
        }
    }

    private static async Task<string> LoadMapHtmlAsync()
    {
        await using Stream stream = await FileSystem.OpenAppPackageFileAsync("map.html");
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
}
