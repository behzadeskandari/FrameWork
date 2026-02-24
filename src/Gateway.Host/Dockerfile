FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files
COPY BankingGateway.sln .
COPY global.json .
COPY src/Gateway.Framework.Core/Gateway.Framework.Core.csproj src/Gateway.Framework.Core/
COPY src/Gateway.Framework.Shared/Gateway.Framework.Shared.csproj src/Gateway.Framework.Shared/
COPY src/Gateway.Framework.Security/Gateway.Framework.Security.csproj src/Gateway.Framework.Security/
COPY src/Gateway.Framework.Logging/Gateway.Framework.Logging.csproj src/Gateway.Framework.Logging/
COPY src/Gateway.Framework.Monitoring/Gateway.Framework.Monitoring.csproj src/Gateway.Framework.Monitoring/
COPY src/Gateway.Framework.Resilience/Gateway.Framework.Resilience.csproj src/Gateway.Framework.Resilience/
COPY src/Gateway.Framework.Infrastructure/Gateway.Framework.Infrastructure.csproj src/Gateway.Framework.Infrastructure/
COPY src/Gateway.Framework.Gateway/Gateway.Framework.Gateway.csproj src/Gateway.Framework.Gateway/
COPY src/Gateway.Host/Gateway.Host.csproj src/Gateway.Host/

# Restore
RUN dotnet restore src/Gateway.Host/Gateway.Host.csproj

# Copy everything and build
COPY src/ src/
RUN dotnet build src/Gateway.Host/Gateway.Host.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish src/Gateway.Host/Gateway.Host.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Security: run as non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Gateway.Host.dll"]
