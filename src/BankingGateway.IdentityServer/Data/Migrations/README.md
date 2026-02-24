# Migrations

Run the following commands from the solution root to generate and apply the initial EF Core migration:

```bash
dotnet ef migrations add InitialIdentitySchema \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer \
    --output-dir Data/Migrations

dotnet ef database update \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer
```

The migration will create the following tables:
- `IdentityUsers` — ASP.NET Core Identity users with banking audit fields
- `IdentityRoles` — ASP.NET Core Identity roles
- OpenIddict tables for applications, authorizations, scopes, and tokens
