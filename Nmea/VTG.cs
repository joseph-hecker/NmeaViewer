using Nmea.Exceptions;

namespace Nmea;

public class VTG : NmeaSentence
{
    public double TrueHeading { get; }
    public double SpeedKnots { get; }
    public double SpeedKmh { get; }

    public VTG(string message) : base(message)
    {
        try
        {
            _ = double.TryParse(_parts[1], out double trueHeading);
            TrueHeading = trueHeading;
            _ = double.TryParse(_parts[5], out double speedKnots);
            SpeedKnots = speedKnots;
            _ = double.TryParse(_parts[7], out double speedKmh);
            SpeedKmh = speedKmh;
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing VTG message. ({_message})", ex);
        }
    }

    public override void UpdateGPSData(GPSData data)
    {
        data.HeadingDegrees = TrueHeading;
        data.SpeedKnots = SpeedKnots;
    }
}
