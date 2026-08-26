namespace Nmea;

public static class NmeaLineExtractor
{
    public static string? ExtractSentence(string line)
    {
        int start = line.IndexOf('$');
        if (start < 0)
        {
            return null;
        }

        int checksumStart = line.IndexOf('*', start);
        if (checksumStart < 0 || checksumStart + 2 >= line.Length)
        {
            return null;
        }

        return line[start..(checksumStart + 3)].Trim();
    }
}
