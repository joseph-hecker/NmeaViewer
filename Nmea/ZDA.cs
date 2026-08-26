using Nmea.Exceptions;

namespace Nmea;

public class ZDA : NmeaSentence
{
    public TimeOnly Time { get; }
    public DateOnly Date { get; }

    public ZDA(string message) : base(message)
    {
        try
        {
            Time = ParseTime(_parts[1]);
            int day = int.Parse(_parts[2]);
            int month = int.Parse(_parts[3]);
            int year = int.Parse(_parts[4]);
            Date = new DateOnly(year, month, day);
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing ZDA message. ({_message})", ex);
        }
    }

    public override void UpdateGPSData(GPSData data)
    {
        data.UTCDate = Date;
        data.UTCTime = Time;
    }
}
