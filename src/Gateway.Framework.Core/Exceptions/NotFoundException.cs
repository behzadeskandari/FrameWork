using Gateway.Framework.Core.Models;

namespace Gateway.Framework.Core.Exceptions;

/// <summary>
/// Exception for resource not found errors.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message, BankingErrorCodes.ResourceNotFound, 404) { }

    public NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.",
            BankingErrorCodes.ResourceNotFound, 404) { }
}
