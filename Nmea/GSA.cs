using Nmea.Exceptions;

namespace Nmea;

public class GSA : NmeaSentence
{
    public char Mode { get; }
    public FixMode FixMode { get; }
    public IReadOnlyList<int> ActivePrns { get; }
    public double PDOP { get; }
    public double HDOP { get; }
    public double VDOP { get; }

    public GSA(string message) : base(message)
    {
        try
        {
            Mode = _parts[1][0];
            FixMode = (FixMode)int.Parse(_parts[2]);
            List<int> prns = [];
            for (int i = 3; i <= 14; i++)
            {
                if (int.TryParse(_parts[i], out int prn))
                {
                    prns.Add(prn);
                }
            }

            ActivePrns = prns;
            PDOP = double.Parse(_parts[15]);
            HDOP = double.Parse(_parts[16]);
            VDOP = double.Parse(_parts[17]);
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing GSA message. ({_message})", ex);
        }
    }

    public override void UpdateGPSData(GPSData data)
    {
        data.FixMode = FixMode;
        data.PDOP = PDOP;
        data.HDOP = HDOP;
        data.VDOP = VDOP;
        data.ActivePrns = [.. data.ActivePrns, .. ActivePrns];
    }
}
