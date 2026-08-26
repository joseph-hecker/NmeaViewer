namespace Nmea;

public static class ConstellationHelper
{
    public static Constellation FromTalkerId(string talkerId) => talkerId switch
    {
        "GP" => Constellation.GPS,
        "GL" => Constellation.GLONASS,
        "GA" => Constellation.Galileo,
        "GB" => Constellation.BeiDou,
        _ => Constellation.Unknown,
    };

    public static Constellation FromSentenceHeader(string header)
    {
        if (header.Length >= 2)
        {
            return FromTalkerId(header[..2]);
        }

        return Constellation.Unknown;
    }
}
