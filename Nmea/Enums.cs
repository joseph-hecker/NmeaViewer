namespace Nmea;

[Flags]
public enum NmeaType : uint
{
    Unknown = 0x00000000,
    GGA = 0x00000001,
    GPGGA = GGA,
    GSV = 0x00000002,
    GAGSV = GSV,
    GBGSV = GSV,
    GLGSV = GSV,
    GPGSV = GSV,
    RMC = 0x00000004,
    GPRMC = RMC,
    VTG = 0x00000008,
    GPVTG = VTG,
    ZDA = 0x00000010,
    GPZDA = ZDA,
    GSA = 0x00000020,
    GPGSA = GSA,
    GNGSA = GSA,
    GST = 0x00000040,
    GPGST = GST,
}

public enum Fix : int
{
    Invalid = 0,
    GPS = 1,
    DGPS = 2,
    RTKFix = 4,
    RTKFloat = 5,
}

public enum FixMode : int
{
    None = 1,
    TwoD = 2,
    ThreeD = 3,
}

public enum Constellation
{
    Unknown,
    GPS,
    GLONASS,
    Galileo,
    BeiDou,
}

public enum SpeedUnit
{
    Knots,
    FeetPerSecond,
    MilesPerHour,
    MetersPerSecond,
    KilometersPerHour,
}
