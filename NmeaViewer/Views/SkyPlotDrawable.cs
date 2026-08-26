using Nmea;

namespace NmeaViewer.Views;

public class SkyPlotDrawable : IDrawable
{
    public IList<Satellite> Satellites { get; set; } = [];
    public Satellite? SelectedSatellite { get; set; }
    public IReadOnlyList<int> ActivePrns { get; set; } = [];
    public Action<Satellite?>? SatelliteTapped { get; set; }

    private readonly List<(Satellite Satellite, PointF Center, float Radius)> _hitTargets = [];

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _hitTargets.Clear();
        float cx = dirtyRect.Center.X;
        float cy = dirtyRect.Center.Y;
        float maxRadius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2f - 8;

        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawCircle(cx, cy, maxRadius);
        canvas.DrawCircle(cx, cy, maxRadius * 2 / 3);
        canvas.DrawCircle(cx, cy, maxRadius / 3);

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;
        canvas.DrawString("N", cx - 4, cy - maxRadius - 14, HorizontalAlignment.Left);

        foreach (Satellite satellite in Satellites)
        {
            float radius = maxRadius * (1 - satellite.Elevation / 90f);
            double radians = (satellite.Azimuth - 90) * Math.PI / 180;
            float x = cx + radius * (float)Math.Cos(radians);
            float y = cy + radius * (float)Math.Sin(radians);
            float dotRadius = 6;

            Color fill = GetSnrColor(satellite.SNR);
            if (!satellite.HasSignal)
            {
                canvas.StrokeColor = Colors.Gray;
                canvas.StrokeSize = 2;
                canvas.DrawCircle(x, y, dotRadius);
            }
            else
            {
                canvas.FillColor = fill;
                canvas.FillCircle(x, y, dotRadius);
            }

            if (SelectedSatellite?.Equals(satellite) == true)
            {
                canvas.StrokeColor = Colors.White;
                canvas.StrokeSize = 2;
                canvas.DrawCircle(x, y, dotRadius + 2);
            }

            canvas.FontColor = Colors.White;
            canvas.FontSize = 8;
            canvas.DrawString(satellite.PRN.ToString(), x - 8, y - 4, 16, 12, HorizontalAlignment.Center, VerticalAlignment.Center);

            _hitTargets.Add((satellite, new PointF(x, y), dotRadius + 4));
        }
    }

    public Satellite? HitTest(PointF point)
    {
        foreach (var (satellite, center, radius) in _hitTargets)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            if (dx * dx + dy * dy <= radius * radius)
            {
                return satellite;
            }
        }

        return null;
    }

    private static Color GetSnrColor(int snr) => snr switch
    {
        >= 40 => Colors.LimeGreen,
        >= 30 => Colors.Gold,
        > 0 => Colors.OrangeRed,
        _ => Colors.Gray,
    };
}
