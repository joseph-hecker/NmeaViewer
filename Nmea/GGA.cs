using Nmea.Exceptions;

namespace Nmea;

public class GGA : NmeaSentence, ILocation
{
    public TimeOnly Time { get; }
    public string Lat { get; }
    public string LatDir { get; }
    public string Lon { get; }
    public string LonDir { get; }
    public Fix Fix { get; }
    public int SatsUsed { get; }
    public double Hdop { get; }
    public double Alt { get; }
    public string AltUnits { get; }
    public double Undulation { get; }
    public string UndulationUnits { get; }
    public int? Age { get; }
    public int? StationID { get; }

    public GGA(string message) : base(message)
    {
        try
        {
            Time = ParseTime(_parts[1]);
            Lat = _parts[2];
            LatDir = _parts[3];
            Lon = _parts[4];
            LonDir = _parts[5];
            Fix = (Fix)int.Parse(_parts[6]);
            SatsUsed = int.Parse(_parts[7]);
            Hdop = double.Parse(_parts[8]);
            Alt = double.Parse(_parts[9]);
            AltUnits = _parts[10];
            Undulation = double.Parse(_parts[11]);
            UndulationUnits = _parts[12];
            if (int.TryParse(_parts[13], out int age)) Age = age;
            if (int.TryParse(_parts[14], out int stationID)) StationID = stationID;
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing GGA message. ({_message})", ex);
        }
    }

    public double Latitude()
    {
        int sign = LatDir == "N" ? 1 : -1;
        int degrees = int.Parse(Lat[..2]);
        double minutes = double.Parse(Lat[2..]) / 60;

        return sign * (degrees + minutes);
    }

    public double Longitude()
    {
        int sign = LonDir == "E" ? 1 : -1;
        int degrees = int.Parse(Lon[..3]);
        double minutes = double.Parse(Lon[3..]) / 60;

        return sign * (degrees + minutes);
    }

    public override void UpdateGPSData(GPSData data)
    {
        data.UTCTime = Time;
        if (!string.IsNullOrWhiteSpace(Lat) && !string.IsNullOrWhiteSpace(Lon))
        {
            data.Latitude = Latitude();
            data.Longitude = Longitude();
        }

        data.Altitude = Alt;
        data.Fix = Fix;
        data.HDOP = Hdop;
        data.SatellitesInUse = SatsUsed;
        data.Age = Age;
        data.StationID = StationID;
    }
}
