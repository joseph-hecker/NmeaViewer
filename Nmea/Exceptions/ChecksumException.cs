namespace Nmea.Exceptions;

public class ChecksumException : NmeaException
{
    public ChecksumException() : base() { }
    public ChecksumException(string message) : base(message) { }
    public ChecksumException(string message, Exception ex) : base(message, ex) { }
}
