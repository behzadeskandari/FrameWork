namespace BankingGateway.Core.Interfaces;

public interface ICorrelationIdProvider
{
    string GetCorrelationId();
    void SetCorrelationId(string correlationId);
}
