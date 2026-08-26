using Nmea;

namespace NmeaViewer.Views;

public class TimelineDrawable : IDrawable
{
    public IReadOnlyList<Epoch> Epochs { get; set; } = [];
    public int CurrentIndex { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Epochs.Count == 0)
        {
            return;
        }

        float width = dirtyRect.Width;
        float height = dirtyRect.Height;
        float step = width / Epochs.Count;

        for (int i = 0; i < Epochs.Count; i++)
        {
            var epoch = Epochs[i];
            Color color = !epoch.HasPosition ? Colors.DarkGray :
                epoch.HasValidFix ? Colors.DodgerBlue : Colors.Red;
            canvas.FillColor = color;
            canvas.FillRectangle(i * step, 0, Math.Max(1, step), height);
        }

        if (CurrentIndex >= 0 && CurrentIndex < Epochs.Count)
        {
            float x = CurrentIndex * step;
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawLine(x, 0, x, height);
        }
    }
}
