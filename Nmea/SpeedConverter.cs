namespace Nmea;

public static class SpeedConverter
{
    private const double KnotsToMph = 1.15078;
    private const double KnotsToMps = 0.514444;
    private const double KnotsToKmh = 1.852;
    private const double KnotsToFps = 1.68781;

    public static double ConvertFromKnots(double knots, SpeedUnit unit) => unit switch
    {
        SpeedUnit.Knots => knots,
        SpeedUnit.MilesPerHour => knots * KnotsToMph,
        SpeedUnit.MetersPerSecond => knots * KnotsToMps,
        SpeedUnit.KilometersPerHour => knots * KnotsToKmh,
        SpeedUnit.FeetPerSecond => knots * KnotsToFps,
        _ => knots,
    };

    public static string UnitLabel(SpeedUnit unit) => unit switch
    {
        SpeedUnit.Knots => "kn",
        SpeedUnit.MilesPerHour => "mph",
        SpeedUnit.MetersPerSecond => "m/s",
        SpeedUnit.KilometersPerHour => "km/h",
        SpeedUnit.FeetPerSecond => "ft/s",
        _ => "kn",
    };
}
