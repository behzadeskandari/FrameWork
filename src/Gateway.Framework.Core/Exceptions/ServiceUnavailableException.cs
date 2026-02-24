using Gateway.Framework.Core.Models;

namespace Gateway.Framework.Core.Exceptions;

/// <summary>
/// Exception for service unavailability.
/// </summary>
public class ServiceUnavailableException : DomainException
{
    public ServiceUnavailableException(string message = "The service is temporarily unavailable.")
        : base(message, BankingErrorCodes.ServiceUnavailable, 503) { }
}
