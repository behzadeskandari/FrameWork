# Migrations

Run from the solution root:

```bash
dotnet ef migrations add InitialIdentitySchema \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer \
    --output-dir Data/Migrations

dotnet ef database update \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer
```
