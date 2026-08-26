namespace Nmea;

public class Epoch
{
    public int Index { get; init; }
    public required GPSData Data { get; init; }
    public required IReadOnlyList<string> RawSentences { get; init; }

    public bool HasPosition =>
        !string.IsNullOrWhiteSpace(LatField) &&
        !string.IsNullOrWhiteSpace(LonField);

    public bool HasValidFix => Data.Fix != Fix.Invalid;

    internal string? LatField { get; init; }
    internal string? LonField { get; init; }
}
