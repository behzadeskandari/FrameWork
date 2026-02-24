using BankingGateway.Core.Interfaces;

namespace BankingGateway.Infrastructure.Logging;

public sealed class CorrelationIdProvider : ICorrelationIdProvider
{
    private readonly AsyncLocal<string?> _correlationId = new();

    public string GetCorrelationId()
    {
        return _correlationId.Value ??= Guid.NewGuid().ToString("N");
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationId.Value = correlationId;
    }
}
