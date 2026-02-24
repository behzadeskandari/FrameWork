namespace Gateway.Framework.Core.Exceptions;

/// <summary>
/// Base exception for domain-level errors.
/// </summary>
public class DomainException : Exception
{
    public string ErrorCode { get; }
    public int HttpStatusCode { get; }

    public DomainException(string message, string errorCode, int httpStatusCode = 400)
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }

    public DomainException(string message, string errorCode, int httpStatusCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }
}
