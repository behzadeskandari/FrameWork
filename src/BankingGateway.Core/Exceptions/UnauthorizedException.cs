namespace BankingGateway.Core.Exceptions;

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message, 401) { }
}
