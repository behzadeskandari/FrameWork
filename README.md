# Banking Gateway Framework

A production-grade, enterprise-level Banking API Gateway Framework built with .NET 8, following Clean Architecture principles.

## Architecture

```
BankingGateway.sln
├── src/
│   ├── Gateway.Framework.Core          # Models, exceptions, interfaces, middleware
│   ├── Gateway.Framework.Shared        # Configuration, constants, extensions
│   ├── Gateway.Framework.Security      # JWT auth, authorization, secure headers, rate limiting
│   ├── Gateway.Framework.Logging       # Serilog, audit logging, sensitive data masking
│   ├── Gateway.Framework.Monitoring    # Health checks, OpenTelemetry, Prometheus
│   ├── Gateway.Framework.Resilience    # Polly retry, circuit breaker, timeout
│   ├── Gateway.Framework.Infrastructure# Caching (Memory/Redis), feature flags
│   ├── Gateway.Framework.Gateway       # YARP reverse proxy configuration
│   └── Gateway.Host                    # Web API host, Program.cs, Dockerfile
└── tests/
    └── Gateway.Framework.Tests         # Unit tests
```

## Features

- **API Gateway** — YARP reverse proxy with path-based routing, load balancing, and request transforms
- **Authentication/Authorization** — JWT Bearer, role-based and policy-based authorization
- **Security** — Secure headers, IP whitelisting, rate limiting, request size limits, HTTPS/HSTS
- **Logging** — Serilog with structured logging, correlation IDs, audit logging, sensitive data masking
- **Monitoring** — Health checks (liveness/readiness), OpenTelemetry tracing, Prometheus metrics
- **Resilience** — Polly retry, circuit breaker, and timeout policies
- **Caching** — In-memory and Redis-ready distributed caching with cache abstraction
- **API Versioning** — URL-based and header-based versioning
- **Swagger/OpenAPI** — With JWT support and XML documentation
- **Standardized Responses** — Unified API response model with banking-specific error codes
- **Feature Flags** — Microsoft.FeatureManagement integration
- **Docker/Kubernetes Ready** — Dockerfile with non-root user, health check endpoints

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Optional) [Docker](https://www.docker.com/)
- (Optional) Redis for distributed caching

## Getting Started

### Build

```bash
dotnet build BankingGateway.sln
```

### Run Tests

```bash
dotnet test
```

### Run the Gateway

```bash
cd src/Gateway.Host
dotnet run
```

The gateway will start at `http://localhost:5000` (or the port configured in `launchSettings.json`).

- **Swagger UI**: `http://localhost:5000/swagger`
- **Health Check**: `http://localhost:5000/health`
- **Liveness Probe**: `http://localhost:5000/health/live`
- **Readiness Probe**: `http://localhost:5000/health/ready`
- **Prometheus Metrics**: `http://localhost:5000/metrics`
- **Ping**: `http://localhost:5000/api/v1/gateway/ping`

### Run with Docker

```bash
# Build from solution root
docker build -f src/Gateway.Host/Dockerfile -t banking-gateway .

# Run
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Production banking-gateway
```

## Configuration

Configuration is loaded from `appsettings.json` and environment variables. Key sections:

| Section | Description |
|---------|-------------|
| `Gateway` | Core gateway settings (name, version) |
| `Gateway:Security` | HTTPS, HSTS, JWT, rate limiting, IP whitelist |
| `Gateway:Cache` | In-memory or Redis cache configuration |
| `Gateway:Resilience` | Retry, circuit breaker, timeout policies |
| `ReverseProxy` | YARP routes and cluster definitions |
| `Serilog` | Log levels and sink configuration |

### Environment Variables

Override any setting with environment variables using `__` as separator:

```bash
export Gateway__Security__Jwt__SecretKey="your-production-secret-key-here"
export Gateway__Cache__RedisConnectionString="localhost:6379"
```

## Downstream Services

Configure downstream services in `appsettings.json` under `ReverseProxy`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "my-service-route": {
        "ClusterId": "my-service-cluster",
        "Match": { "Path": "/api/my-service/{**catch-all}" }
      }
    },
    "Clusters": {
      "my-service-cluster": {
        "Destinations": {
          "destination1": { "Address": "https://my-service:5001/" }
        },
        "LoadBalancingPolicy": "RoundRobin"
      }
    }
  }
}
```

## Security Notes

- In production, always use a strong JWT secret key via environment variables or a secret manager
- Enable HTTPS redirection and HSTS in production
- Configure IP whitelisting as needed
- Rate limiting is enabled by default (100 requests per 60 seconds)
- All responses include security headers (X-Content-Type-Options, X-Frame-Options, CSP, etc.)

## License

This project is proprietary software for banking operations.
