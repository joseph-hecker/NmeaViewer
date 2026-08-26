using Nmea.Exceptions;

namespace Nmea;

public class GST : NmeaSentence
{
    public TimeOnly Time { get; }
    public double Rms { get; }
    public double StdMajor { get; }
    public double StdMinor { get; }
    public double Orientation { get; }
    public double StdLatitude { get; }
    public double StdLongitude { get; }
    public double StdAltitude { get; }

    public GST(string message) : base(message)
    {
        try
        {
            Time = ParseTime(_parts[1]);
            Rms = double.Parse(_parts[2]);
            StdMajor = double.Parse(_parts[3]);
            StdMinor = double.Parse(_parts[4]);
            Orientation = double.Parse(_parts[5]);
            StdLatitude = double.Parse(_parts[6]);
            StdLongitude = double.Parse(_parts[7]);
            StdAltitude = double.Parse(_parts[8]);
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing GST message. ({_message})", ex);
        }
    }

    public double HorizontalAccuracy => Math.Sqrt(StdLatitude * StdLatitude + StdLongitude * StdLongitude);

    public override void UpdateGPSData(GPSData data)
    {
        data.EstimatedAccuracy = HorizontalAccuracy;
    }
}
