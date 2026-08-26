namespace Nmea;

public class GsvReassembler
{
    private readonly Dictionary<string, GSV?[]> _partsByTalker = new();

    public void Add(GSV gsv)
    {
        if (gsv.MessageNumber == 1)
        {
            _partsByTalker[gsv.TalkerId] = new GSV[gsv.TotalMessages];
        }

        if (_partsByTalker.TryGetValue(gsv.TalkerId, out GSV?[]? sequence) && sequence is not null)
        {
            sequence[gsv.MessageNumber - 1] = gsv;
        }
    }

    public IEnumerable<Satellite> GetMergedSatellites()
    {
        return _partsByTalker.Values
            .Where(sequence => sequence.All(part => part is not null))
            .SelectMany(sequence => sequence.SelectMany(part => part!.Satellites));
    }

    public int GetSatellitesInView() => GetMergedSatellites().Count();
}
