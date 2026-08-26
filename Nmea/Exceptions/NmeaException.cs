namespace Nmea.Exceptions;

public class NmeaException : Exception
{
    public NmeaException() : base() { }
    public NmeaException(string message) : base(message) { }
    public NmeaException(string message, Exception ex) : base(message, ex) { }
}
