using Nmea.Exceptions;

namespace Nmea;

public abstract class NmeaSentence : IEquatable<NmeaSentence>
{
    protected readonly string _message;
    protected readonly string[] _parts;

    public NmeaType ID { get; }

    public NmeaSentence(string message)
    {
        try
        {
            _message = message.Replace("\r\n", "");
            _parts = _message[1..^3].Split(',');
            ID = Enum.Parse<NmeaType>(_parts[0]);
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing NMEA message. ({_message})", ex);
        }
    }

    public abstract void UpdateGPSData(GPSData data);

    public void ValidateChecksum()
    {
        int index = _message.IndexOf('*');
        byte expected = Convert.ToByte(_message[(index + 1)..], 16);
        byte checksum = 0;
        for (int i = 1; i < index; i++)
        {
            checksum ^= Convert.ToByte(_message[i]);
        }

        if (checksum != expected)
        {
            throw new ChecksumException($"Expected checksum 0x{expected:X} ({expected}), but got 0x{checksum:X} ({checksum}). ({_message})");
        }
    }

    public bool Equals(NmeaSentence? other) => _message.Equals(other?._message);

    public override bool Equals(object? obj) => Equals(obj as NmeaSentence);

    public override int GetHashCode() => _message.GetHashCode();

    public override string ToString() => _message;

    public static NmeaSentence? Parse(string message)
    {
        try
        {
            int index = message.IndexOf(',');
            _ = Enum.TryParse(message[1..index], out NmeaType nmeaType);
            NmeaSentence? sentence = nmeaType switch
            {
                NmeaType.GGA => new GGA(message),
                NmeaType.GSV => new GSV(message),
                NmeaType.RMC => new RMC(message),
                NmeaType.VTG => new VTG(message),
                NmeaType.ZDA => new ZDA(message),
                NmeaType.GSA => new GSA(message),
                NmeaType.GST => new GST(message),
                _ => null,
            };

            sentence?.ValidateChecksum();
            return sentence;
        }
        catch (NmeaException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing NMEA message. ({message.Replace("\r\n", "")})", ex);
        }
    }

    protected static TimeOnly ParseTime(string time) => TimeOnly.ParseExact(time, "HHmmss.ff");

    protected static DateOnly ParseDate(string date) => DateOnly.ParseExact(date, "ddMMyy");
}
