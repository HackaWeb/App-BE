namespace App.Domain.Exceptions;

public class DomainException : Exception
{
    public int? InternalStatusCode { get; }

    public IReadOnlyDictionary<string, string> AdditionalInfo { get; }

    public DomainException(string message, int? internalStatusCode = null, IDictionary<string, string> additionalInfo = null)
        : base(message)
    {
        InternalStatusCode = internalStatusCode;
        AdditionalInfo = ((additionalInfo != null) ? additionalInfo.ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value) : new Dictionary<string, string>());
    }

    public DomainException(string message, Exception exception, int? internalStatusCode = null, IDictionary<string, string> additionalInfo = null)
        : base(message, exception)
    {
        InternalStatusCode = internalStatusCode;
        AdditionalInfo = ((additionalInfo != null) ? additionalInfo.ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value) : new Dictionary<string, string>());
    }
}
