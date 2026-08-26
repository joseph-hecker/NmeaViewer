using Nmea.Exceptions;

namespace Nmea;

public class GSV : NmeaSentence
{
    private static readonly Dictionary<string, GSV?[]> ReassemblyByTalker = new();

    public string TalkerId { get; }
    public int TotalMessages { get; }
    public int MessageNumber { get; }
    public int SatellitesInView { get; }
    public List<Satellite> Satellites { get; }

    public GSV(string message) : base(message)
    {
        try
        {
            TalkerId = _parts[0][..2];
            TotalMessages = int.Parse(_parts[1]);
            MessageNumber = int.Parse(_parts[2]);
            SatellitesInView = int.Parse(_parts[3]);
            var constellation = ConstellationHelper.FromTalkerId(TalkerId);
            Satellites = [];
            for (int i = 4; i < _parts.Length - 1; i += 4)
            {
                _ = int.TryParse(_parts[i], out int prn);
                _ = int.TryParse(_parts[i + 1], out int elevation);
                _ = int.TryParse(_parts[i + 2], out int azimuth);
                _ = int.TryParse(_parts[i + 3], out int snr);
                if (prn != default || elevation != default || azimuth != default || snr != default)
                {
                    Satellites.Add(new(prn, elevation, azimuth, snr, constellation));
                }
            }
        }
        catch (Exception ex)
        {
            throw new NmeaException($"Error parsing GSV message. ({_message})", ex);
        }
    }

    public override void UpdateGPSData(GPSData data)
    {
        if (MessageNumber == 1)
        {
            ReassemblyByTalker[TalkerId] = new GSV[TotalMessages];
        }

        if (!ReassemblyByTalker.TryGetValue(TalkerId, out GSV?[]? sequence) || sequence is null)
        {
            return;
        }

        sequence[MessageNumber - 1] = this;

        if (MessageNumber != TotalMessages || !sequence.All(part => part is not null))
        {
            return;
        }

        data.SatellitesInView = sequence.Sum(part => part!.Satellites.Count);
        data.Satellites = ReassemblyByTalker.Values
            .Where(parts => parts.All(part => part is not null))
            .SelectMany(parts => parts.SelectMany(part => part!.Satellites));
    }

    public static void ResetReassembly() => ReassemblyByTalker.Clear();
}
