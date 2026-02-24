namespace BankingGateway.Core.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, 404) { }
}
