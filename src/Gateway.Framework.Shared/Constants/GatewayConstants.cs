namespace Gateway.Framework.Shared.Constants;

/// <summary>
/// Framework-wide constants.
/// </summary>
public static class GatewayConstants
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string ApiKeyHeader = "X-API-Key";
    public const string TenantIdHeader = "X-Tenant-ID";
    public const string RequestIdHeader = "X-Request-ID";
    
    public static class Policies
    {
        public const string AdminPolicy = "AdminPolicy";
        public const string ReadPolicy = "ReadPolicy";
        public const string WritePolicy = "WritePolicy";
        public const string TransactionPolicy = "TransactionPolicy";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Service = "Service";
        public const string Auditor = "Auditor";
    }

    public static class ClaimTypes
    {
        public const string TenantId = "tenant_id";
        public const string Permission = "permission";
        public const string ClientId = "client_id";
    }
}
