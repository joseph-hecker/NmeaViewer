using Nmea.Exceptions;

namespace Nmea;

public static class EpochTimelineBuilder
{
    public static IReadOnlyList<Epoch> BuildFromLines(IEnumerable<string> lines)
    {
        List<EpochGroup> groups = [];
        List<(NmeaSentence Sentence, string Raw)> betweenSentences = [];

        foreach (string line in lines)
        {
            string? raw = NmeaLineExtractor.ExtractSentence(line);
            if (raw is null)
            {
                continue;
            }

            NmeaSentence? sentence;
            try
            {
                sentence = NmeaSentence.Parse(raw);
            }
            catch (NmeaException)
            {
                continue;
            }

            if (sentence is null)
            {
                continue;
            }

            if (sentence is GGA gga)
            {
                if (groups.Count > 0)
                {
                    groups[^1].Sentences.AddRange(betweenSentences);
                    betweenSentences = [];
                }

                groups.Add(new EpochGroup(gga, [(sentence, raw)]));
            }
            else if (groups.Count > 0)
            {
                betweenSentences.Add((sentence, raw));
            }
        }

        if (groups.Count > 0 && betweenSentences.Count > 0)
        {
            groups[^1].Sentences.AddRange(betweenSentences);
        }

        return groups.Select((group, index) => group.ToEpoch(index)).ToList();
    }

    public static IReadOnlyList<Epoch> BuildFromFile(string path) =>
        BuildFromLines(File.ReadLines(path));

    private static GPSData BuildGpsData(IReadOnlyList<NmeaSentence> sentences)
    {
        GPSData data = new();
        GsvReassembler gsvReassembler = new();

        foreach (NmeaSentence sentence in sentences)
        {
            if (sentence is GSV gsv)
            {
                gsvReassembler.Add(gsv);
            }
            else
            {
                sentence.UpdateGPSData(data);
            }
        }

        var satellites = gsvReassembler.GetMergedSatellites().ToList();
        if (satellites.Count > 0)
        {
            data.Satellites = satellites;
            data.SatellitesInView = satellites.Count;
        }

        return data;
    }

    private sealed class EpochGroup(GGA anchor, List<(NmeaSentence Sentence, string Raw)> sentences)
    {
        public GGA Anchor { get; } = anchor;
        public List<(NmeaSentence Sentence, string Raw)> Sentences { get; } = sentences;

        public Epoch ToEpoch(int index)
        {
            var ordered = Sentences.Select(pair => pair.Sentence).ToList();
            return new Epoch
            {
                Index = index,
                Data = BuildGpsData(ordered),
                RawSentences = Sentences.Select(pair => pair.Raw).ToList(),
                LatField = Anchor.Lat,
                LonField = Anchor.Lon,
            };
        }
    }
}
