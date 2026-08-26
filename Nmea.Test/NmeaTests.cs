using Nmea.Exceptions;

namespace Nmea.Test;

public class NmeaTests
{
    private static int NmeaIndex { get; set; }

    private static string FormatErrorMessage()
    {
        string? nmea = TestContext.CurrentContext.Test.Arguments.SingleOrDefault(arg => arg is string str && str.StartsWith('$')) as string;
        string message = $"Error parsing nmea message at element {NmeaIndex}.";
        if (nmea is not null)
        {
            message += $" ({nmea})";
        }

        return message;
    }

    private static string[] GetInvalidMedssageForEachNmeaType()
    {
        NmeaType[] types = [NmeaType.GGA, NmeaType.GSV, NmeaType.RMC, NmeaType.VTG, NmeaType.ZDA, NmeaType.GSA, NmeaType.GST];
        return [.. types.Select(id => $"${id},,,,,,*00")];
    }

    [SetUp]
    public void SetUp()
    {
        NmeaIndex = 0;
        GSV.ResetReassembly();
    }

    [Test, TestOf(nameof(GGA))]
    [TestCase("$GPGGA,172814.00,3723.46587704,N,12202.26957864,W,2,6,1.2,18.893,M,-25.669,M,2.0,0031*7F")]
    [TestCase("$GPGGA,220314.00,4032.11025544,N,10502.52948864,W,0,15,0.6,1514.592,M,-20.906,M,2.0,0029*4A")]
    public void TestGGA(string nmea)
    {
        GGA gga = new(nmea);
        Assert.Multiple(() =>
        {
            string[] expected = nmea[..^3].Split(',');
            Assert.That(gga.ID, Is.EqualTo(NmeaType.GGA), FormatErrorMessage);
            Assert.That(gga.Time.ToString("HHmmss.ff"), Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
            Assert.That(gga.Lat, Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
            Assert.That(gga.LatDir, Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
            Assert.That(gga.Lon, Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
            Assert.That(gga.LonDir, Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
            Assert.That(gga.Fix, Is.EqualTo(Enum.Parse<Fix>(expected[++NmeaIndex])), FormatErrorMessage);
            Assert.That(gga.SatsUsed, Is.EqualTo(int.Parse(expected[++NmeaIndex])), FormatErrorMessage);
            Assert.That(gga.Hdop, Is.EqualTo(double.Parse(expected[++NmeaIndex])), FormatErrorMessage);
            Assert.That(gga.Alt, Is.EqualTo(double.Parse(expected[++NmeaIndex])), FormatErrorMessage);
            Assert.That(gga.AltUnits, Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
            Assert.That(gga.Undulation, Is.EqualTo(double.Parse(expected[++NmeaIndex])), FormatErrorMessage);
            Assert.That(gga.UndulationUnits, Is.EqualTo(expected[++NmeaIndex]), FormatErrorMessage);
        });

        GPSData data = new();
        gga.UpdateGPSData(data);
        Assert.Multiple(() =>
        {
            Assert.That(data.UTC.TimeOfDay, Is.EqualTo(gga.Time.ToTimeSpan()));
            Assert.That(data.Latitude, Is.EqualTo(gga.Latitude()));
            Assert.That(data.Longitude, Is.EqualTo(gga.Longitude()));
            Assert.That(data.Altitude, Is.EqualTo(gga.Alt));
            Assert.That(data.Fix, Is.EqualTo(gga.Fix));
            Assert.That(data.HDOP, Is.EqualTo(gga.Hdop));
            Assert.That(data.SatellitesInUse, Is.EqualTo(gga.SatsUsed));
            Assert.That(data.Age, Is.EqualTo(gga.Age));
            Assert.That(data.StationID, Is.EqualTo(gga.StationID));
        });

        Assert.That(gga.ValidateChecksum, Throws.Nothing);
        Assert.That(NmeaSentence.Parse(nmea), Is.EqualTo(gga));
    }

    [Test, TestOf(nameof(GSV))]
    public void TestGSVPerTalkerReassembly()
    {
        string[] messages =
        [
            "$GPGSV,3,1,11,05,12,061,33,10,27,254,46,13,27,048,36,15,57,071,48,1*60",
            "$GPGSV,3,2,11,16,13,287,,18,75,011,49,23,66,285,49,24,10,128,36,1*61",
            "$GPGSV,3,3,11,26,04,256,,27,16,318,38,29,32,169,47,,,,,1*51",
        ];

        GPSData data = new();
        foreach (string message in messages)
        {
            GSV gsv = new(message);
            gsv.UpdateGPSData(data);
        }

        Assert.That(data.Satellites.Count(), Is.EqualTo(11));
    }

    [Test, TestOf(nameof(NmeaSentence.Parse))]
    [TestCase("$GPXXX,123456.00,3723.46587704,N,12202.26957864,W,2,6,1.2,18.893,M,-25.669,M,2.0,0031*7A")]
    public void TestParseUnknownSentence(string nmea)
    {
        Assert.That(NmeaSentence.Parse(nmea), Is.Null);
    }

    [Test, TestOf(nameof(NmeaSentence.Parse))]
    [TestCase("This is not a nmea sentence.")]
    [TestCaseSource(nameof(GetInvalidMedssageForEachNmeaType))]
    public void TestParseInvalidSentence(string nmea)
    {
        Assert.Throws<NmeaException>(() => NmeaSentence.Parse(nmea));
    }

    [Test, TestOf(nameof(NmeaSentence.ValidateChecksum))]
    [TestCase("$GPGGA,172814.00,3723.46587704,N,12202.26957864,W,2,6,1.2,18.893,M,-25.669,M,2.0,0031*7F", true)]
    [TestCase("$GPGGA,220314.00,4032.11025544,N,10502.52948864,W,0,15,0.6,1514.592,M,-20.906,M,2.0,0029*4A", true)]
    [TestCase("$GPGGA,172814.00,3723.46587704,N,12202.26957864,W,2,6,1.2,18.893,M,-25.669,M,2.0,0031*00", false)]
    public void TestValidateChecksum(string nmea, bool isValid)
    {
        Assert.That(() => NmeaSentence.Parse(nmea), isValid ? Throws.Nothing : Throws.TypeOf<ChecksumException>());
    }

    [Test, TestOf(nameof(RMC))]
    public void TestRMC()
    {
        const string nmea = "$GPRMC,175539.00,A,4032.09886864,N,10502.48855827,W,0.04,105.20,210725,7.5,E,A,U*5E";
        RMC rmc = new(nmea);
        Assert.Multiple(() =>
        {
            Assert.That(rmc.IsValid, Is.True);
            Assert.That(rmc.SpeedKnots, Is.EqualTo(0.04).Within(0.001));
            Assert.That(rmc.Course, Is.EqualTo(105.20).Within(0.01));
            Assert.That(rmc.Date, Is.EqualTo(new DateOnly(2025, 7, 21)));
        });
    }

    [Test, TestOf(nameof(VTG))]
    public void TestVTG()
    {
        const string nmea = "$GPVTG,105.20,T,97.68,M,0.04,N,0.08,K,A*19";
        VTG vtg = new(nmea);
        Assert.Multiple(() =>
        {
            Assert.That(vtg.TrueHeading, Is.EqualTo(105.20).Within(0.01));
            Assert.That(vtg.SpeedKnots, Is.EqualTo(0.04).Within(0.001));
            Assert.That(vtg.SpeedKmh, Is.EqualTo(0.08).Within(0.001));
        });
    }

    [Test, TestOf(nameof(GST))]
    public void TestGST()
    {
        const string nmea = "$GPGST,175541.00,2.000,1.600,1.213,324.585,1.481,1.355,2.578*5B";
        GST gst = new(nmea);
        GPSData data = new();
        gst.UpdateGPSData(data);
        Assert.That(data.EstimatedAccuracy, Is.EqualTo(gst.HorizontalAccuracy).Within(0.001));
    }
}
