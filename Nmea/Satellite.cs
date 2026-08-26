namespace Nmea;

public class Satellite(int prn, int elevation, int azimuth, int snr, Constellation constellation = Constellation.Unknown) : IEquatable<Satellite>
{
    public int PRN { get; } = prn;
    public int Elevation { get; } = elevation;
    public int Azimuth { get; } = azimuth;
    public int SNR { get; } = snr;
    public Constellation Constellation { get; } = constellation;

    public bool HasSignal => SNR > 0;

    public bool Equals(Satellite? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return PRN == other.PRN &&
               Elevation == other.Elevation &&
               Azimuth == other.Azimuth &&
               SNR == other.SNR &&
               Constellation == other.Constellation;
    }

    public override bool Equals(object? obj) => Equals(obj as Satellite);

    public override int GetHashCode() => HashCode.Combine(PRN, Elevation, Azimuth, SNR, Constellation);

    public override string ToString() =>
        $"(PRN: {PRN}, Elevation: {Elevation}, Azimuth: {Azimuth}, SNR: {SNR}, Constellation: {Constellation})";
}
