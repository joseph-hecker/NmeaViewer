using Nmea;

namespace NmeaViewer.Helpers;

public static class FixDisplayHelper
{
    public static string GetFixLabel(Fix fix) => fix switch
    {
        Fix.Invalid => "Invalid (0)",
        Fix.GPS => "GPS (1)",
        Fix.DGPS => "DGPS (2)",
        Fix.RTKFix => "RTK Fix (4)",
        Fix.RTKFloat => "RTK Float (5)",
        _ => fix.ToString(),
    };
}
