using Gateway.Framework.Core.Models;

namespace Gateway.Framework.Core.Exceptions;

/// <summary>
/// Exception for authorization failures.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, BankingErrorCodes.AuthorizationFailed, 403) { }
}
