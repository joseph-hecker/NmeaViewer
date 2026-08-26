using Nmea.Exceptions;

namespace Nmea;

public class RMC : NmeaSentence, ILocation
{
    public TimeOnly Time { get; }
    public bool IsValid { get; }
    public string Lat { get; }
    public string LatDir { get; }
    public string Lon { get; }
    public string LonDir { get; }
    public double SpeedKnots { get; }
    public double Course { get; }
    public DateOnly Date { get; }

    public RMC(string message) : base(message)
    {
        try
        {
            Time = ParseTime(_parts[1]);
            IsValid = _parts[2] == "A";
            Lat = _parts[3];
            LatDir = _parts[4];
            Lon = _parts[5];
            LonDir = _parts[6];
            _ = double.TryParse(_parts[7], out double speed);
            SpeedKnots = speed;
            _ = double.TryParse(_parts[8], out double course);
            Course = course;
            Date = ParseDate(_parts[9]);
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing RMC message. ({_message})", ex);
        }
    }

    public double Latitude()
    {
        if (string.IsNullOrEmpty(Lat)) return 0;
        int sign = LatDir == "N" ? 1 : -1;
        int degrees = int.Parse(Lat[..2]);
        double minutes = double.Parse(Lat[2..]) / 60;
        return sign * (degrees + minutes);
    }

    public double Longitude()
    {
        if (string.IsNullOrEmpty(Lon)) return 0;
        int sign = LonDir == "E" ? 1 : -1;
        int degrees = int.Parse(Lon[..3]);
        double minutes = double.Parse(Lon[3..]) / 60;
        return sign * (degrees + minutes);
    }

    public override void UpdateGPSData(GPSData data)
    {
        data.UTCDate = Date;
        data.UTCTime = Time;
        if (!string.IsNullOrEmpty(Lat) && !string.IsNullOrEmpty(Lon))
        {
            data.Latitude = Latitude();
            data.Longitude = Longitude();
        }

        if (SpeedKnots > 0)
        {
            data.SpeedKnots = SpeedKnots;
        }

        if (Course > 0)
        {
            data.HeadingDegrees = Course;
        }
    }
}
