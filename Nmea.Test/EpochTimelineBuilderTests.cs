namespace Nmea.Test;

public class NmeaLineExtractorTests
{
    [Test]
    public void ExtractsBareSentence()
    {
        const string line = "$GPGGA,175539.00,4032.09886864,N,10502.48855827,W,1,05,2.5,1515.360,M,-20.906,M,,*6A";
        Assert.That(NmeaLineExtractor.ExtractSentence(line), Is.EqualTo(line));
    }

    [Test]
    public void ExtractsSentenceWithPrefix()
    {
        const string line = "2025-07-21 17:55:39 $GPGGA,175539.00,4032.09886864,N,10502.48855827,W,1,05,2.5,1515.360,M,-20.906,M,,*6A";
        Assert.That(NmeaLineExtractor.ExtractSentence(line), Is.EqualTo("$GPGGA,175539.00,4032.09886864,N,10502.48855827,W,1,05,2.5,1515.360,M,-20.906,M,,*6A"));
    }

    [Test]
    public void ReturnsNullWhenNoSentence()
    {
        Assert.That(NmeaLineExtractor.ExtractSentence("not nmea"), Is.Null);
    }
}

public class SpeedConverterTests
{
    [Test]
    public void ConvertsKnotsToMph()
    {
        Assert.That(SpeedConverter.ConvertFromKnots(1, SpeedUnit.MilesPerHour), Is.EqualTo(1.15078).Within(0.0001));
    }
}

public class EpochTimelineBuilderTests
{
    private static string TestDataPath =>
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "nmea", "Arrow100_SBAS_SHORT.txt");

    [Test]
    public void BuildsEpochsFromSampleFile()
    {
        var epochs = EpochTimelineBuilder.BuildFromFile(TestDataPath);
        Assert.That(epochs, Is.Not.Empty);
        Assert.That(epochs.All(e => e.RawSentences.Any(s => s.StartsWith("$GPGGA"))), Is.True);
    }

    [Test]
    public void DiscardsOrphanPreamble()
    {
        string[] lines =
        [
            "$GNGSA,A,3,10,15,18,23,29,,,,,,,,5.2,2.5,4.6,1*35",
            "$GPGGA,175539.00,4032.09886864,N,10502.48855827,W,1,05,2.5,1515.360,M,-20.906,M,,*6A",
        ];

        var epochs = EpochTimelineBuilder.BuildFromLines(lines);
        Assert.That(epochs, Has.Count.EqualTo(1));
        Assert.That(epochs[0].RawSentences, Has.Count.EqualTo(1));
    }

    [Test]
    public void GroupsSentencesBetweenGgaMessages()
    {
        string[] lines =
        [
            "$GPGGA,175539.00,4032.09886864,N,10502.48855827,W,1,05,2.5,1515.360,M,-20.906,M,,*6A",
            "$GPVTG,105.20,T,97.68,M,0.04,N,0.08,K,A*19",
            "$GPGST,175539.00,2.000,4.530,2.012,18.194,4.350,2.378,9.157*69",
            "$GPGGA,175540.00,4032.09902185,N,10502.48844572,W,1,07,1.1,1514.778,M,-20.906,M,,*6B",
        ];

        var epochs = EpochTimelineBuilder.BuildFromLines(lines);
        Assert.That(epochs, Has.Count.EqualTo(2));
        Assert.That(epochs[0].RawSentences, Has.Count.EqualTo(3));
        Assert.That(epochs[0].Data.SpeedKnots, Is.EqualTo(0.04).Within(0.001));
        Assert.That(epochs[0].Data.EstimatedAccuracy, Is.GreaterThan(0));
    }
}
