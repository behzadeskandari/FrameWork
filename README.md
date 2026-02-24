# Banking Gateway — .NET 8 Microservices Framework

A **production-grade, banking-focused** microservices framework built with .NET 8 and ASP.NET Core. This solution provides a secure, scalable, and resilient API Gateway with centralized identity management, advanced caching, comprehensive monitoring, and banking-grade security controls.

---

## ?? Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Core Features](#core-features)
4. [Project Structure](#project-structure)
5. [Installation & Setup](#installation--setup)
6. [Configuration](#configuration)
7. [Running Locally](#running-locally)
8. [API Documentation](#api-documentation)
9. [Security Considerations](#security-considerations)
10. [Logging & Monitoring](#logging--monitoring)
11. [Caching Strategy](#caching-strategy)
12. [Rate Limiting & Resilience](#rate-limiting--resilience)
13. [Database & Migrations](#database--migrations)
14. [Testing](#testing)
15. [Docker Deployment](#docker-deployment)
16. [Contributing](#contributing)
17. [License](#license)
18. [Support](#support)

---

## Project Overview

**Banking Gateway** is a comprehensive, enterprise-grade solution designed for financial institutions and payment platforms. It combines:

- **?? Centralized Identity Management** via OpenIddict (OpenID Connect / OAuth 2.0)
- **?? API Gateway & Reverse Proxy** powered by YARP (Yet Another Reverse Proxy)
- **?? Distributed Caching** with Redis and in-memory fallback
- **??? Security-First Architecture** with banking-grade password policies, lockout mechanisms, and security headers
- **?? Observability & Monitoring** using Serilog, OpenTelemetry, and Prometheus metrics
- **? Rate Limiting & Resilience** with circuit breakers and retry policies
- **? Request Validation** via FluentValidation
- **?? Correlation ID Tracking** for distributed tracing
- **??? Entity Framework Core** with SQL Server and comprehensive migrations

---

## Architecture

```
???????????????????????????????????????????????????????????????????
?                    CLIENT APPLICATIONS                          ?
?              (Web, Mobile, SPA, Microservices)                 ?
???????????????????????????????????????????????????????????????????
                             ?
                             ?
???????????????????????????????????????????????????????????????????
?                   API GATEWAY (YARP)                            ?
?  ???????????????????????????????????????????????????????????   ?
?  ? • Request Routing & Load Balancing                      ?   ?
?  ? • Rate Limiting (100 req/60s default)                  ?   ?
?  ? • Correlation ID Injection                              ?   ?
?  ? • Security Headers & IP Filtering                       ?   ?
?  ? • Request Size Validation                               ?   ?
?  ? • Exception Handling & Logging                          ?   ?
?  ???????????????????????????????????????????????????????????   ?
?                    BankingGateway.Api                           ?
???????????????????????????????????????????????????????????????????
             ?                                        ?
      ???????????????                        ???????????????????
      ?              ?                        ?                 ?
      ?              ?                        ?                 ?
???????????????? ????????????????    ???????????????? ????????????????
?   Identity   ? ?   Accounts   ?    ?  Payments    ? ?   Services   ?
?   Server     ? ?   Service    ?    ?   Service    ? ?  Microsvcs   ?
?              ? ?              ?    ?              ? ?              ?
? • OAuth2.0   ? ? • User Info  ?    ? • Processing ? ? (Internal    ?
? • OpenID     ? ? • Account    ?    ? • History    ? ?  Services)   ?
? • JWT Tokens ? ?   Management ?    ? • Status     ? ?              ?
? • Roles &    ? ?              ?    ?              ? ?              ?
?   Claims     ? ?              ?    ?              ? ?              ?
???????????????? ????????????????    ???????????????? ????????????????
       ?              ?                      ?              ?
       ??????????????????????????????????????????????????????
                      ?
        ?????????????????????????????
        ?             ?             ?
        ?             ?             ?
   ??????????  ?????????????  ???????????
   ?  SQL   ?  ?  Redis    ?  ? Serilog ?
   ? Server ?  ?  Cache    ?  ?  Logs   ?
   ??????????  ?????????????  ???????????
        ?
        ?
   ??????????????????????
   ? Prometheus Metrics ?
   ? OpenTelemetry      ?
   ? Distributed Trace  ?
   ??????????????????????
```

---

## Core Features

### ?? **Authentication & Authorization**

- **OpenIddict Server** with OpenID Connect support
- **OAuth 2.0 Authorization Code Flow** with PKCE (Proof Key for Code Exchange) — mandatory
- **Client Credentials Flow** for service-to-service authentication
- **JWT Token Introspection** for API Gateway validation
- **Reference Tokens** (persisted in DB) for server-side revocation capability
- **Scope-based Authorization** (`openid`, `email`, `profile`, `roles`, `banking_api`)
- **Role-based Access Control (RBAC)** — Admin, Writer, Auditor, User roles
- **Policy-based Authorization** — AdminOnly, ReadAccess, WriteAccess, AuditAccess

### ?? **API Gateway & Routing**

- **YARP Reverse Proxy** for intelligent request routing
- **Load Balancing** across multiple destination clusters
- **Correlation ID Propagation** via `X-Correlation-ID` header
- **Route transformation** for token injection and header manipulation
- **Swagger / OpenAPI** integration for API discovery

### ??? **Security Controls**

- **Banking-grade Password Policy**
  - Minimum 12 characters
  - Required: uppercase, lowercase, digits, special characters
  - Minimum 6 unique characters
- **Account Lockout** — 15 minutes after 5 failed attempts
- **Security Headers**
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `X-XSS-Protection: 1; mode=block`
  - `Content-Security-Policy: default-src 'self'`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- **HTTPS Enforcement** (HSTS) in production
- **Request Size Limit** — configurable (default 10 MB)
- **IP Filtering** — optional whitelisting for sensitive endpoints
- **Certificate-based Signing & Encryption** for OpenIddict tokens

### ?? **Caching Strategy**

- **Dual-mode Caching**:
  - **Redis** for distributed caching (production)
  - **In-Memory Cache** for local fallback (development)
- **TTL-based Expiration** (default 5 minutes)
- **JSON Serialization** for complex types
- **Cache Key Management** with instance naming

### ?? **Observability & Monitoring**

- **Serilog** structured logging to console and files
- **Daily rolling log files** (`logs/gateway-.log`, `logs/identityserver-.log`)
- **OpenTelemetry** for distributed tracing
- **Prometheus Metrics** at `/metrics` endpoint
- **Request Logging** with Serilog middleware
- **Health Checks**
  - Liveness: `/health/live` (always healthy if running)
  - Readiness: `/health/ready` (includes Redis, database checks)
- **Correlation IDs** for request tracing across services

### ? **Rate Limiting & Resilience**

- **Fixed Window Rate Limiter**
  - Default: 100 requests per 60 seconds
  - Configurable queue processing order
- **Resilience Policies** (Polly)
  - Retry with exponential backoff
  - Circuit breaker pattern
  - Timeout protection
- **Database Resilience** — SQL Server retry on transient failures

---

## Project Structure

```
FrameWork/
??? src/
?   ??? BankingGateway.Core/
?   ?   ??? Configuration/
?   ?   ?   ??? JwtSettings.cs              # JWT authority, audience config
?   ?   ?   ??? RedisSettings.cs            # Redis connection & instance settings
?   ?   ?   ??? RateLimitSettings.cs        # Rate limit permits, window, queue
?   ?   ?   ??? ResilienceSettings.cs       # Retry, circuit breaker, timeout config
?   ?   ?   ??? SecurityHeadersSettings.cs  # HSTS, CSP, max body size config
?   ?   ??? Exceptions/
?   ?   ?   ??? DomainException.cs          # Base exception with HTTP status codes
?   ?   ?   ??? NotFoundException.cs        # 404 exceptions
?   ?   ?   ??? UnauthorizedException.cs    # 401 exceptions
?   ?   ?   ??? ForbiddenException.cs       # 403 exceptions
?   ?   ?   ??? ConflictException.cs        # 409 exceptions
?   ?   ??? Interfaces/
?   ?       ??? ICacheService.cs            # Get/Set/Remove/Exists cache operations
?   ?       ??? IAuditLogger.cs             # Audit event logging contract
?   ?       ??? ICorrelationIdProvider.cs   # Correlation ID retrieval
?   ?
?   ??? BankingGateway.Infrastructure/
?   ?   ??? Caching/
?   ?   ?   ??? RedisCacheService.cs        # Distributed Redis cache implementation
?   ?   ?   ??? MemoryCacheService.cs       # In-memory fallback cache
?   ?   ??? Logging/
?   ?   ?   ??? AuditLogger.cs              # Structured audit logging
?   ?   ?   ??? CorrelationIdProvider.cs    # HTTP context-based ID provider
?   ?   ??? DependencyInjection.cs          # Service registration extension
?   ?
?   ??? BankingGateway.Api/
?   ?   ??? Program.cs                      # API Gateway startup (YARP, Auth, Monitoring)
?   ?   ??? Middleware/
?   ?   ?   ??? ExceptionHandlingMiddleware.cs   # Global exception -> JSON response
?   ?   ?   ??? SecurityHeadersMiddleware.cs     # Security header injection
?   ?   ?   ??? CorrelationIdMiddleware.cs       # Correlation ID generation/tracking
?   ?   ?   ??? IpFilteringMiddleware.cs         # IP whitelist enforcement
?   ?   ?   ??? RequestSizeLimitMiddleware.cs    # Request body size validation
?   ?   ??? Auth/
?   ?   ?   ??? ClaimsTransformer.cs        # Claims transformation pipeline
?   ?   ??? Validation/
?   ?   ?   ??? ValidationFilter.cs         # FluentValidation integration filter
?   ?   ??? appsettings.json                # Gateway config (JWT, Redis, YARP routes)
?   ?   ??? appsettings.Development.json    # Dev-specific overrides
?   ?
?   ??? BankingGateway.IdentityServer/
?       ??? Program.cs                      # Identity server startup (OpenIddict setup)
?       ??? Controllers/
?       ?   ??? AuthorizationController.cs  # OpenID endpoints (/connect/authorize, token, etc)
?       ?   ??? AccountController.cs        # Login/Logout UI endpoints
?       ??? Data/
?       ?   ??? ApplicationDbContext.cs      # EF Core DbContext for Identity & OpenIddict
?       ?   ??? DatabaseSeeder.cs           # Initial data seeding (admin user, clients)
?       ?   ??? Migrations/                 # EF Core migrations
?       ??? Domain/
?       ?   ??? ApplicationUser.cs           # Custom user class with banking fields
?       ?   ??? ApplicationRole.cs           # Custom role class
?       ??? Helpers/
?       ?   ??? ClaimsDestinationHelper.cs   # Claims routing for ID/Access tokens
?       ??? Views/
?       ?   ??? Account/Login.cshtml         # Login form UI
?       ?   ??? Home/Index.cshtml            # Home page
?       ?   ??? Shared/_Layout.cshtml        # Base layout
?       ??? appsettings.json                 # Identity server config
?       ??? appsettings.Development.json     # Dev config
?
??? tests/
    ??? BankingGateway.Tests/
        ??? BankingGateway.Tests.csproj      # Integration & unit tests (xUnit, Moq, FluentAssertions)
        ??? [test fixtures]                  # Uses WebApplicationFactory for API testing
```

### Key Files Explained

#### **BankingGateway.Core** (Shared, dependency-light)
- **Purpose**: Core abstractions, configurations, and exceptions used by all projects
- **Key Classes**:
  - `DomainException`: Base exception with HTTP status codes
  - `ICacheService`: Cache abstraction (Redis/Memory)
  - `ICorrelationIdProvider`: Request tracing
  - `IAuditLogger`: Structured audit logging
- **Configuration Classes**: `JwtSettings`, `RedisSettings`, `RateLimitSettings`, `ResilienceSettings`, `SecurityHeadersSettings`

#### **BankingGateway.Infrastructure** (Cross-cutting concerns)
- **Purpose**: Concrete implementations of Core interfaces and shared services
- **Key Classes**:
  - `RedisCacheService`: Production distributed caching
  - `MemoryCacheService`: Development/fallback caching
  - `AuditLogger`: Serilog-based audit trail
  - `CorrelationIdProvider`: HTTP context tracking
  - `DependencyInjection.cs`: Service registration

#### **BankingGateway.Api** (API Gateway)
- **Purpose**: Entry point for all client requests, routes to backend services
- **Key Components**:
  - **Program.cs**: Configures Serilog, OpenIddict validation, YARP routes, rate limiting, health checks, OpenTelemetry, security headers
  - **Middleware**: Exception handling, security headers, correlation IDs, IP filtering, request size limits
  - **Authentication**: OpenIddict introspection + JWT validation
  - **Authorization**: Role & policy-based (AdminOnly, ReadAccess, WriteAccess, AuditAccess)
  - **Rate Limiting**: Fixed-window limiter (100/60s default)
  - **YARP Configuration**: Routes for `accounts-route`, `payments-route` with load balancing

#### **BankingGateway.IdentityServer** (OpenID Connect / OAuth 2.0 Server)
- **Purpose**: Centralized identity provider for all services
- **Key Components**:
  - **Program.cs**: OpenIddict server setup with:
    - Authorization Code + PKCE (mandatory)
    - Client Credentials flow
    - Refresh Token flow
    - Token endpoints, userinfo endpoint, logout
    - Reference tokens (persisted, revocable)
    - Development signing/encryption certs or production certificate store
  - **AuthorizationController.cs**: OAuth2 endpoints
    - `/connect/authorize` — authorization code request
    - `/connect/token` — token exchange
    - `/connect/userinfo` — user info endpoint
    - `/connect/logout` — logout endpoint
  - **AccountController.cs**: Login/Logout UI
  - **ApplicationUser**: Extended user with banking fields (NationalId, Department, LastLoginAt, LockedUntil, etc.)
  - **DatabaseSeeder**: Creates admin user, roles, and OAuth clients
  - **Razor Pages**: Login page, home page, shared layout

#### **BankingGateway.Tests** (Integration & Unit Tests)
- **Framework**: xUnit + Moq + FluentAssertions
- **Key Packages**:
  - `Microsoft.AspNetCore.Mvc.Testing` for `WebApplicationFactory`
  - `coverlet.collector` for code coverage
- **Test Coverage**: API endpoints, middleware, authorization, exception handling

---

## Installation & Setup

### Prerequisites

- **.NET 9.0.304 SDK** ([Download](https://dotnet.microsoft.com/download))
  - Or configure in `global.json` as needed
- **SQL Server 2019+** (local or remote)
  - Or edit connection strings for PostgreSQL/MySQL
- **Redis** (optional, for distributed caching)
  - Or use in-memory cache in development
- **Git**
- **Visual Studio 2022** or **VS Code** with C# extensions

### Step 1: Clone Repository

```bash
git clone https://github.com/behzadeskandari/FrameWork.git
cd FrameWork
```

### Step 2: Restore NuGet Dependencies

```bash
dotnet restore
```

This restores all packages across all projects.

### Step 3: Configure Connection Strings

#### **For Identity Server** (`src/BankingGateway.IdentityServer/appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "IdentityServer": "Server=localhost;Database=BankingGateway_Identity;User Id=sa;Password=YourSecurePassword;TrustServerCertificate=True;"
  },
  "OpenIddict": {
    "Issuer": "https://localhost:5010",
    "Certificates": {
      "Signing": { "Thumbprint": "" },
      "Encryption": { "Thumbprint": "" }
    },
    "GatewayClientSecret": "your-gateway-client-secret-here"
  },
  "Seed": {
    "Admin": {
      "Email": "admin@bankinggateway.local",
      "Password": "SecureAdminPassword123!",
      "FirstName": "System",
      "LastName": "Administrator"
    }
  }
}
```

#### **For API Gateway** (`src/BankingGateway.Api/appsettings.Development.json`):

```json
{
  "Jwt": {
    "Authority": "https://localhost:5010",
    "Audience": "banking-gateway",
    "RequireHttpsMetadata": false
  },
  "OpenIddict": {
    "GatewayClientSecret": "your-gateway-client-secret-here"
  },
  "Redis": {
    "Enabled": false,
    "ConnectionString": "localhost:6379",
    "InstanceName": "BankingGateway_"
  }
}
```

### Step 4: Create & Migrate Database

From the solution root:

```bash
# Create Identity Server database
dotnet ef database update \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer
```

This creates:
- `IdentityUsers`, `IdentityRoles` tables
- OpenIddict application, authorization, scope, and token tables
- Seeds initial admin user and clients

### Step 5: Build Solution

```bash
dotnet build
```

Verify no compilation errors occur.

---

## Configuration

### Identity Server Configuration

**File**: `src/BankingGateway.IdentityServer/appsettings.json`

| Setting | Default | Purpose |
|---------|---------|---------|
| `ConnectionStrings:IdentityServer` | Local SQL Server | Database for users, roles, OAuth clients |
| `OpenIddict:Issuer` | `https://localhost:5010` | Token issuer URI |
| `OpenIddict:GatewayClientSecret` | (Required) | Shared secret for API Gateway |
| `Seed:Admin:*` | Values | Initial admin user credentials & fields |

**Key OpenIddict Features**:
- **Flows Enabled**:
  - Authorization Code + PKCE (user login)
  - Client Credentials (service-to-service)
  - Refresh Token (offline access)
- **Token Lifetimes**:
  - Access Token: 15 minutes
  - Refresh Token: 1 day
  - Authorization Code: 5 minutes
- **Scopes**: `openid`, `email`, `profile`, `roles`, `offline_access`, `banking_api`
- **Signing**: Development auto-certs or production certificate store thumbprints

### API Gateway Configuration

**File**: `src/BankingGateway.Api/appsettings.json`

#### JWT Settings
```json
{
  "Jwt": {
    "Authority": "https://localhost:5010",
    "Audience": "banking-gateway",
    "RequireHttpsMetadata": true
  }
}
```

#### Rate Limiting
```json
{
  "RateLimit": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "QueueLimit": 0
  }
}
```

#### Redis Caching
```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "localhost:6379,ssl=false",
    "InstanceName": "BankingGateway_"
  }
}
```

#### Security Headers
```json
{
  "SecurityHeaders": {
    "EnableHsts": true,
    "HstsMaxAgeSeconds": 31536000,
    "MaxRequestBodySize": 10485760,
    "EnableIpFiltering": false,
    "AllowedIpRanges": []
  }
}
```

#### Resilience Policies
```json
{
  "Resilience": {
    "RetryCount": 3,
    "RetryBaseDelayMs": 200,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30,
    "TimeoutSeconds": 30
  }
}
```

#### YARP Reverse Proxy Routes
```json
{
  "ReverseProxy": {
    "Routes": {
      "accounts-route": {
        "ClusterId": "accounts-cluster",
        "Match": { "Path": "/api/accounts/{**catch-all}" },
        "Transforms": [
          { "RequestHeader": "X-Correlation-ID", "Set": "{Header:X-Correlation-ID}" }
        ]
      }
    },
    "Clusters": {
      "accounts-cluster": {
        "LoadBalancingPolicy": "RoundRobin",
        "Destinations": {
          "accounts-primary": { "Address": "https://accounts-service.internal:5001/" }
        }
      }
    }
  }
}
```

---

## Running Locally

### Using .NET CLI

**Terminal 1 — Start Identity Server** (port 5010):

```bash
cd src/BankingGateway.IdentityServer
dotnet run --launch-profile BankingGateway.IdentityServer
```

Expected output:
```
[2024-01-15 10:30:45 INF] Now listening on: https://localhost:5010
```

**Terminal 2 — Start API Gateway** (port 5000):

```bash
cd src/BankingGateway.Api
dotnet run
```

Expected output:
```
[2024-01-15 10:30:50 INF] Now listening on: https://localhost:5000 and http://localhost:5001
```

### Using Visual Studio

1. Set **Startup Projects** to multiple:
   - `BankingGateway.IdentityServer`
   - `BankingGateway.Api`
2. Press `F5` to start debugging
3. Both services launch with configured ports and profiles

### Verify Services are Running

```bash
# Health check — API Gateway liveness
curl -k https://localhost:5000/health/live

# Health check — API Gateway readiness
curl -k https://localhost:5000/health/ready

# Swagger — API Gateway
curl -k https://localhost:5000/swagger/index.html
```

---

## API Documentation

### Identity Server Endpoints

#### **Authorization** (`POST /connect/authorize`)
Initiates OAuth 2.0 Authorization Code flow (user login).

**Request**:
```
GET /connect/authorize?
  client_id=banking-gateway&
  response_type=code&
  scope=openid%20email%20profile%20banking_api&
  redirect_uri=https://localhost:5000/signin-oidc&
  state=random-state&
  code_challenge=challenge&
  code_challenge_method=S256
```

**Response**: User is redirected to login page if not authenticated, then back to `redirect_uri` with authorization code.

---

#### **Token Exchange** (`POST /connect/token`)
Exchanges authorization code for access/refresh tokens.

**Request**:
```bash
curl -X POST https://localhost:5010/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=authorization_code&code=AUTH_CODE&client_id=banking-gateway&client_secret=SECRET&redirect_uri=https://localhost:5000/signin-oidc&code_verifier=VERIFIER"
```

**Response**:
```json
{
  "access_token": "eyJhbGc...",
  "refresh_token": "eyJhbGc...",
  "token_type": "Bearer",
  "expires_in": 900
}
```

---

#### **Refresh Token** (`POST /connect/token`)
Exchanges refresh token for new access token.

**Request**:
```bash
curl -X POST https://localhost:5010/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=refresh_token&refresh_token=REFRESH_TOKEN&client_id=banking-gateway&client_secret=SECRET"
```

---

#### **User Info** (`GET /connect/userinfo`)
Returns authenticated user's claims.

**Request**:
```bash
curl -X GET https://localhost:5010/connect/userinfo \
  -H "Authorization: Bearer ACCESS_TOKEN"
```

**Response**:
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "admin@bankinggateway.local",
  "email_verified": true,
  "name": "admin@bankinggateway.local",
  "given_name": "System",
  "family_name": "Administrator",
  "role": ["Admin"]
}
```

---

#### **Logout** (`GET/POST /connect/logout`)
Invalidates user session and access/refresh tokens.

**Request**:
```bash
curl -X POST https://localhost:5010/connect/logout \
  -H "Content-Type: application/x-www-form-urlencoded" \
  --data-raw ""
```

---

### API Gateway Endpoints

#### **Health Checks**

**Liveness** (process running?):
```bash
curl -k https://localhost:5000/health/live
# Response: 200 OK
```

**Readiness** (ready to serve?):
```bash
curl -k https://localhost:5000/health/ready
# Response: 200 OK (or 503 if dependencies unavailable)
```

---

#### **Metrics** (Prometheus)

```bash
curl -k https://localhost:5000/metrics
```

Returns Prometheus-format metrics:
```
# HELP dotnet_gc_collection_count_total Total number of garbage collections
# TYPE dotnet_gc_collection_count_total counter
dotnet_gc_collection_count_total{generation="0"} 5
...
```

---

#### **Swagger / OpenAPI**

```bash
curl -k https://localhost:5000/swagger/index.html
```

Interactive Swagger UI at `https://localhost:5000/swagger/ui`.

---

### Rate Limiting Behavior

**Default**: 100 requests per 60 seconds

When exceeded:
```
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{
  "error": "Rate limit exceeded",
  "statusCode": 429,
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

### Example: Protected API Call

1. **Login at Identity Server**:
```bash
# Assumption: user already authenticated via browser
# Retrieve access token from session/storage
ACCESS_TOKEN=$(cat token.txt)
```

2. **Call Protected Endpoint**:
```bash
curl -X GET https://localhost:5000/api/accounts/profile \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "X-Correlation-ID: 550e8400-e29b-41d4-a716-446655440000"
```

3. **API Gateway**:
   - Validates JWT via OpenIddict introspection
   - Checks authorization policies
   - Routes to backend `accounts-service`
   - Injects correlation ID
   - Logs request with Serilog

---

## Security Considerations

### ?? Password Policy (Banking-Grade)

- **Minimum length**: 12 characters
- **Requires uppercase**: At least one `A-Z`
- **Requires lowercase**: At least one `a-z`
- **Requires digits**: At least one `0-9`
- **Requires special characters**: At least one `!@#$%^&*`
- **Unique characters**: Minimum 6 different characters

**Example valid password**: `Secure@Pass123`

---

### ?? Account Lockout

- **Lockout duration**: 15 minutes
- **Failed attempts threshold**: 5 attempts
- **Explicit tracking**: `LockedUntil` timestamp on `ApplicationUser`

After 5 failed login attempts within the lockout duration, the account is locked until the time expires.

---

### ??? HTTPS & TLS

- **Production**: HSTS enabled (31536000 seconds = 1 year)
- **Development**: HTTP allowed for testing
- **Header**: `Strict-Transport-Security: max-age=31536000; includeSubDomains`

**Recommendation**: Use certificates from trusted CAs in production.

---

### ?? Security Headers

**Applied by `SecurityHeadersMiddleware.cs`**:

| Header | Value | Purpose |
|--------|-------|---------|
| `X-Content-Type-Options` | `nosniff` | Prevent MIME sniffing |
| `X-Frame-Options` | `DENY` | Disable framing (clickjacking) |
| `X-XSS-Protection` | `1; mode=block` | XSS protection (legacy) |
| `Referrer-Policy` | `strict-origin-when-cross-origin` | Referrer leakage prevention |
| `Content-Security-Policy` | `default-src 'self'` | Only load resources from same origin |
| `Permissions-Policy` | `camera=(), microphone=(), geolocation=()` | Disable dangerous APIs |

**Removed Headers** (information hiding):
- `X-Powered-By` — hides ASP.NET Core
- `Server` — hides server technology

---

### ?? Token Security

**Access Tokens**:
- **Type**: Reference tokens (persisted in database)
- **Lifetime**: 15 minutes
- **Revocation**: Server-side revocation possible (tokens in DB)
- **Storage**: Secure HTTP-only cookies recommended

**Refresh Tokens**:
- **Type**: Reference tokens (persisted in database)
- **Lifetime**: 1 day
- **Revocation**: Server-side revocation possible
- **Rotation**: Recommended to rotate on each use

**Signing & Encryption**:
- **Development**: Auto-generated ephemeral certificates
- **Production**: X.509 certificates from Windows Certificate Store (by thumbprint)

---

### ?? OpenID Connect PKCE

**Mandatory** for Authorization Code flow. Prevents authorization code interception attacks:

```
client_secret NOT sent over front-channel
code_challenge = BASE64URL(SHA256(code_verifier))
code_challenge_method = S256 (SHA-256)
```

---

### ?? Request Size Limits

**Max request body size**: 10 MB (configurable)

Requests exceeding limit receive `413 Payload Too Large`:

```json
{
  "error": "Request body too large",
  "statusCode": 413,
  "correlationId": "..."
}
```

---

### ?? IP Filtering (Optional)

Enable in `appsettings.json`:

```json
{
  "SecurityHeaders": {
    "EnableIpFiltering": true,
    "AllowedIpRanges": ["192.168.1.0/24", "10.0.0.1"]
  }
}
```

Requests from unlisted IPs receive `403 Forbidden`.

---

### ?? Audit Logging

All authentication events are logged via `AuditLogger`:

```
[2024-01-15 10:35:22 INF] User 'admin@bankinggateway.local' logged in. CorrelationId=550e8400-e29b-41d4-a716-446655440000
[2024-01-15 10:35:45 WRN] User 'testuser@bank.local' account locked out. CorrelationId=...
```

---

## Logging & Monitoring

### Serilog Structured Logging

**Console Output** (with timestamp, level, message, properties):

```
[2024-01-15 10:35:22 INF] User '{Email}' logged in. {"Email":"admin@bankinggateway.local","UserId":"550e8400-e29b-41d4-a716-446655440000","Application":"BankingGateway.IdentityServer"}
```

**File Output** (rolling daily):

```
logs/
  gateway-20240115.log
  gateway-20240116.log
  identityserver-20240115.log
  identityserver-20240116.log
```

**Configuration** (`appsettings.json`):

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### OpenTelemetry Tracing & Metrics

**Configured in `BankingGateway.Api/Program.cs`**:

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("BankingGateway"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddPrometheusExporter());
```

**Metrics Endpoint**: `https://localhost:5000/metrics`

**Exportable to**:
- Prometheus (scrape `/metrics`)
- Jaeger (tracing)
- Datadog
- New Relic

---

### Correlation ID Tracking

**Injected by `CorrelationIdMiddleware.cs`**:

1. **Request arrives** ? Middleware generates or retrieves `X-Correlation-ID` header
2. **Throughout request** ? Same ID used in all logs
3. **Response sent** ? `X-Correlation-ID` included in response headers
4. **Distributed tracing** ? ID propagated to downstream services

**Usage in code**:

```csharp
// Inject ICorrelationIdProvider
public MyService(ICorrelationIdProvider provider)
{
    var correlationId = provider.GetCorrelationId();
    _logger.LogInformation("Processing request {CorrelationId}", correlationId);
}
```

---

### Health Checks

**Liveness** (`/health/live`): Process is running (always 200 OK)

```bash
curl -k https://localhost:5000/health/live
# 200 OK
```

**Readiness** (`/health/ready`): All dependencies available

```bash
curl -k https://localhost:5000/health/ready
# 200 OK (Database + Redis OK)
# OR
# 503 Service Unavailable (Redis down, etc.)
```

**Configuration**:

```csharp
var healthChecksBuilder = builder.Services.AddHealthChecks();
if (redisConfig.Enabled)
{
    healthChecksBuilder.AddRedis(redisConfig.ConnectionString, name: "redis");
}
```

---

## Caching Strategy

### Dual-Mode Architecture

**Redis (Production)**:
- **Type**: Distributed cache
- **Connection**: `localhost:6379` (configurable)
- **Instance Name**: `BankingGateway_` (prefix for keys)
- **Serialization**: JSON
- **TTL**: 5 minutes default

**In-Memory (Development)**:
- **Type**: Local process memory
- **Performance**: Ultra-fast (~1-5 µs)
- **Limitation**: Not shared across processes
- **Eviction**: LRU policy

### Usage Example

```csharp
// Inject ICacheService
public MyService(ICacheService cache)
{
    _cache = cache;
}

// Get from cache
var user = await _cache.GetAsync<ApplicationUser>("user:123");

// Set in cache (expires in 30 minutes)
await _cache.SetAsync("user:123", user, TimeSpan.FromMinutes(30));

// Remove from cache
await _cache.RemoveAsync("user:123");

// Check existence
bool exists = await _cache.ExistsAsync("user:123");
```

### Configuration

```json
{
  "Redis": {
    "Enabled": true,
    "ConnectionString": "localhost:6379,ssl=false",
    "InstanceName": "BankingGateway_"
  }
}
```

---

## Rate Limiting & Resilience

### Rate Limiting

**Fixed-Window Limiter** in `BankingGateway.Api/Program.cs`:

- **Permit Limit**: 100 requests
- **Window**: 60 seconds
- **Queue Limit**: 0 (no queuing)
- **Order**: FIFO (oldest first)

**Behavior**:
- Requests 1-100 ? `200 OK`
- Request 101+ ? `429 Too Many Requests`

**Configuration**:

```json
{
  "RateLimit": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "QueueLimit": 0
  }
}
```

---

### Resilience Policies (Polly)

**Configured in `BankingGateway.Infrastructure`**:

| Policy | Default | Purpose |
|--------|---------|---------|
| **Retry** | 3 attempts | Transient failures (network timeouts, 5xx) |
| **Retry Delay** | 200 ms (exponential) | Backoff to avoid overwhelming service |
| **Circuit Breaker** | 5 failures ? 30s open | Prevent cascading failures |
| **Timeout** | 30 seconds | Abort hung requests |

**Implementation**:

```csharp
var resilience = options.GetSection(ResilienceSettings.SectionName).Get<ResilienceSettings>();

var policy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(
        retryCount: resilience.RetryCount,
        sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(
            resilience.RetryBaseDelayMs * Math.Pow(2, attempt - 1)))
    .WrapAsync(Policy
        .Handle<Exception>()
        .CircuitBreakerAsync<HttpResponseMessage>(
            handledEventsAllowedBeforeBreaking: resilience.CircuitBreakerThreshold,
            durationOfBreak: TimeSpan.FromSeconds(resilience.CircuitBreakerDurationSeconds)));
```

---

## Database & Migrations

### Entity Framework Core Setup

**Provider**: SQL Server (configurable for PostgreSQL, MySQL)

**Connection String** (`appsettings.json`):

```json
{
  "ConnectionStrings": {
    "IdentityServer": "Server=localhost;Database=BankingGateway_Identity;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

**Resilience**:
- **Retry on Failure**: 5 attempts
- **Retry Delay**: 30 seconds between attempts
- **Command Timeout**: 60 seconds

---

### Creating Migrations

From solution root:

**Add migration**:

```bash
dotnet ef migrations add MigrationName \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer \
    --output-dir Data/Migrations
```

**Apply migration**:

```bash
dotnet ef database update \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer
```

**Check pending migrations**:

```bash
dotnet ef migrations list \
    --project src/BankingGateway.IdentityServer \
    --startup-project src/BankingGateway.IdentityServer
```

---

### Database Schema

**Core Tables**:

1. **IdentityUsers** (ApplicationUser)
   - `Id` (Guid)
   - `UserName`, `Email`, `NormalizedEmail`
   - `PasswordHash`, `SecurityStamp`
   - `FirstName`, `LastName`, `NationalId`
   - `IsActive`, `Department`
   - `CreatedAt`, `LastLoginAt`, `LockedUntil`
   - `FailedLoginCount`, `MustChangePassword`

2. **IdentityRoles** (ApplicationRole)
   - `Id` (Guid)
   - `Name`, `NormalizedName`

3. **IdentityUserRoles**
   - Maps users to roles

4. **OpenIddictApplications**
   - Registered OAuth clients (e.g., `banking-gateway`)

5. **OpenIddictScopes**
   - Scopes (e.g., `openid`, `email`, `banking_api`)

6. **OpenIddictTokens** (Reference Tokens)
   - Persisted access/refresh tokens (revocable)

---

## Testing

### Test Framework

- **Framework**: xUnit
- **Mocking**: Moq
- **Assertions**: FluentAssertions
- **Integration**: WebApplicationFactory

### Running Tests

**All tests**:

```bash
dotnet test
```

**Specific project**:

```bash
dotnet test tests/BankingGateway.Tests/BankingGateway.Tests.csproj
```

**With coverage**:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutput=coverage/ /p:CoverletOutputFormat=opencover
```

**Specific test class**:

```bash
dotnet test --filter "TestClass=BankingGateway.Tests.AuthenticationTests"
```

---

### Example Integration Test

```csharp
[Fact]
public async Task Login_ValidCredentials_Returns200()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    var loginModel = new { email = "admin@test.local", password = "ValidPassword123!" };

    // Act
    var response = await client.PostAsJsonAsync("/account/login", loginModel);

    // Assert
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
}
```

---

### Test Project Structure

```
tests/
  BankingGateway.Tests/
    ??? AuthenticationTests.cs           # OpenIddict, JWT validation
    ??? AuthorizationTests.cs            # RBAC, policy-based
    ??? ApiGatewayTests.cs               # YARP routing, rate limiting
    ??? CachingTests.cs                  # Redis/Memory cache
    ??? LoggingTests.cs                  # Serilog, correlation IDs
    ??? SecurityTests.cs                 # Password policy, lockout
```

---

## Docker Deployment

### Dockerfile (API Gateway)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files
COPY ["src/BankingGateway.Core/BankingGateway.Core.csproj", "src/BankingGateway.Core/"]
COPY ["src/BankingGateway.Infrastructure/BankingGateway.Infrastructure.csproj", "src/BankingGateway.Infrastructure/"]
COPY ["src/BankingGateway.Api/BankingGateway.Api.csproj", "src/BankingGateway.Api/"]

# Restore and build
RUN dotnet restore "src/BankingGateway.Api/BankingGateway.Api.csproj"
COPY . .
RUN dotnet build "src/BankingGateway.Api/BankingGateway.Api.csproj" -c Release -o /app/build

# Publish
FROM builder AS publish
RUN dotnet publish "src/BankingGateway.Api/BankingGateway.Api.csproj" -c Release -o /app/publish

# Runtime
FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "BankingGateway.Api.dll"]
```

---

### Docker Compose (Complete Stack)

```yaml
version: '3.9'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword123!" -Q "SELECT 1"
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: redis-cli ping
      interval: 10s
      timeout: 5s
      retries: 5

  # Identity Server
  identity-server:
    build:
      context: .
      dockerfile: src/BankingGateway.IdentityServer/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: "Production"
      ASPNETCORE_URLS: "https://+:5010"
      ASPNETCORE_Kestrel__Certificates__Default__Path: "/app/certs/tls.crt"
      ASPNETCORE_Kestrel__Certificates__Default__Password: ""
      ConnectionStrings__IdentityServer: "Server=sqlserver;Database=BankingGateway_Identity;User Id=sa;Password=YourPassword123!;"
      OpenIddict__Issuer: "https://identity-server:5010"
      OpenIddict__GatewayClientSecret: "gateway-secret-key"
    depends_on:
      sqlserver:
        condition: service_healthy
    ports:
      - "5010:5010"
    volumes:
      - ./certs:/app/certs:ro
    networks:
      - banking-network

  # API Gateway
  api-gateway:
    build:
      context: .
      dockerfile: src/BankingGateway.Api/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: "Production"
      ASPNETCORE_URLS: "https://+:5000"
      ASPNETCORE_Kestrel__Certificates__Default__Path: "/app/certs/tls.crt"
      Jwt__Authority: "https://identity-server:5010"
      Jwt__Audience: "banking-gateway"
      Redis__Enabled: "true"
      Redis__ConnectionString: "redis:6379"
    depends_on:
      - identity-server
      - redis
    ports:
      - "5000:5000"
    volumes:
      - ./certs:/app/certs:ro
    networks:
      - banking-network

volumes:
  sqlserver_data:
  redis_data:

networks:
  banking-network:
    driver: bridge
```

**Run stack**:

```bash
docker-compose up -d
```

**Check logs**:

```bash
docker-compose logs -f api-gateway
docker-compose logs -f identity-server
```

**Stop stack**:

```bash
docker-compose down
```

---

## Contributing

We welcome contributions! Please follow these guidelines:

### 1. Fork & Clone

```bash
git clone https://github.com/behzadeskandari/FrameWork.git
cd FrameWork
```

### 2. Create Feature Branch

```bash
git checkout -b feature/my-feature
```

### 3. Follow Code Standards

- **Naming**: PascalCase for classes, camelCase for variables
- **Method length**: Max 30 lines (single responsibility)
- **Comments**: Document complex logic and public APIs
- **Null safety**: Use `#nullable enable` and handle nulls
- **Async**: Use `async/await` for I/O operations
- **Logging**: Log important events at appropriate levels

### 4. Test Changes

```bash
dotnet test
```

Ensure 80%+ code coverage for new code.

### 5. Commit & Push

```bash
git add .
git commit -m "feat: Add new feature description"
git push origin feature/my-feature
```

### 6. Create Pull Request

- Clear description of changes
- Link to related issues
- Screenshots for UI changes
- Test instructions

---

## License

This project is licensed under the **MIT License**. See [LICENSE](LICENSE) for details.

---

## Support

### Getting Help

- **Issues**: [GitHub Issues](https://github.com/behzadeskandari/FrameWork/issues)
- **Discussions**: [GitHub Discussions](https://github.com/behzadeskandari/FrameWork/discussions)
- **Email**: [contact@example.com] (to be configured)

### Documentation

- **Migrations**: [Data/Migrations/README.md](src/BankingGateway.IdentityServer/Data/Migrations/README.md)
- **Security Policy**: [SECURITY.md](SECURITY.md) (to be created)
- **Architecture Decision Records**: [ADR/](docs/adr/) (to be created)

---

## Troubleshooting

### Issue: "Connection string not configured"

**Solution**: Ensure `appsettings.Development.json` has valid `ConnectionStrings:IdentityServer` and `ConnectionStrings:DefaultConnection`.

```bash
# Test connection
sqlcmd -S localhost -U sa -P YourPassword -Q "SELECT 1"
```

---

### Issue: "Port already in use"

**Solution**: Change port in `launchSettings.json` or kill process:

```bash
# Windows
netstat -ano | findstr :5010
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :5010
kill -9 <PID>
```

---

### Issue: "Redis connection refused"

**Solution**: Either disable Redis (use in-memory cache) or start Redis:

```bash
# Docker
docker run -d -p 6379:6379 redis:7-alpine

# Or disable in appsettings.json
{
  "Redis": {
    "Enabled": false
  }
}
```

---

### Issue: "Certificate not found in store"

**Solution** (Production only): Import X.509 certificate to certificate store:

```powershell
# Windows
$cert = Import-PfxCertificate -FilePath "cert.pfx" -CertStoreLocation "Cert:\LocalMachine\My" -Password (ConvertTo-SecureString "password" -AsPlainText -Force)
Write-Host "Thumbprint: $($cert.Thumbprint)"
```

Then update `appsettings.json` with thumbprint.

---

### Issue: "HTTP 401 Unauthorized on API calls"

**Solution**: 
1. Verify access token is valid: `curl https://localhost:5010/connect/userinfo -H "Authorization: Bearer $TOKEN"`
2. Check token scopes include `banking_api`
3. Ensure API Gateway is configured with correct JWT Authority and Audience
4. Check logs: `tail -f logs/gateway-*.log`

---

## Performance Tuning

### Database Connection Pooling

```json
{
  "ConnectionStrings": {
    "IdentityServer": "Server=localhost;Database=BankingGateway_Identity;Min Pool Size=5;Max Pool Size=100;"
  }
}
```

### Redis Connection Pooling

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379,minIoThreads=10,maxIoThreads=50"
  }
}
```

### Kestrel Threading

```csharp
builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10_485_760;
    options.Limits.Min = new KestrelServerLimits
    {
        MaxRequestLineSize = 16_384,
        MaxRequestHeadersTotalSize = 32_768
    };
});
```

---

## Security Checklist

- [ ] Change default admin password in `appsettings.json`
- [ ] Generate production X.509 certificates for signing/encryption
- [ ] Set `OpenIddict:GatewayClientSecret` to strong random value
- [ ] Enable HTTPS everywhere (`RequireHttpsMetadata: true`)
- [ ] Enable HSTS in production (`EnableHsts: true`, `HstsMaxAgeSeconds: 31536000`)
- [ ] Configure IP filtering if needed (`EnableIpFiltering: true`)
- [ ] Rotate refresh tokens regularly (implement token rotation policy)
- [ ] Monitor logs for suspicious activity
- [ ] Keep .NET and NuGet packages updated
- [ ] Use secrets management (Azure Key Vault, AWS Secrets Manager) for sensitive configs
- [ ] Implement rate limiting per user/IP
- [ ] Enable audit logging for all authentication events
- [ ] Regularly review and audit database access patterns

---

## Roadmap

- [ ] Multi-tenancy support
- [ ] SMS/Email 2FA
- [ ] FIDO2 / WebAuthn support
- [ ] GraphQL API support
- [ ] Kubernetes Helm charts
- [ ] Distributed tracing with Jaeger
- [ ] Advanced threat detection
- [ ] API versioning strategy

---

## Changelog

### Version 1.0.0 (2024-01-15)
- Initial release
- OpenIddict OAuth 2.0 / OpenID Connect server
- API Gateway with YARP
- Serilog structured logging
- OpenTelemetry observability
- Redis/Memory caching
- Rate limiting & resilience policies
- Banking-grade security controls

---

## Contributing Authors

- **Behzad Eskandari** — Project Lead & Architecture

---

**Last Updated**: 2024-01-15  
**Version**: 1.0.0

---

**Happy coding! ?? For production deployments, follow the [Security Checklist](#security-checklist) and [Configuration](#configuration) guides.**
