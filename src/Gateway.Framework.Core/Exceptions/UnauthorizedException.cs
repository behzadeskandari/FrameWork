using Gateway.Framework.Core.Models;

namespace Gateway.Framework.Core.Exceptions;

/// <summary>
/// Exception for authentication failures.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base(message, BankingErrorCodes.AuthenticationFailed, 401) { }
}
