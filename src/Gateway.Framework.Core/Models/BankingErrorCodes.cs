namespace Gateway.Framework.Core.Models;

/// <summary>
/// Banking-specific error codes for standardized error handling.
/// </summary>
public static class BankingErrorCodes
{
    public const string GeneralError = "BANK_0001";
    public const string ValidationError = "BANK_0002";
    public const string AuthenticationFailed = "BANK_0003";
    public const string AuthorizationFailed = "BANK_0004";
    public const string ResourceNotFound = "BANK_0005";
    public const string RateLimitExceeded = "BANK_0006";
    public const string ServiceUnavailable = "BANK_0007";
    public const string TransactionFailed = "BANK_0008";
    public const string InsufficientFunds = "BANK_0009";
    public const string AccountLocked = "BANK_0010";
    public const string InvalidAccountNumber = "BANK_0011";
    public const string DuplicateTransaction = "BANK_0012";
    public const string ExternalServiceError = "BANK_0013";
    public const string DataIntegrityError = "BANK_0014";
    public const string ConfigurationError = "BANK_0015";
}
