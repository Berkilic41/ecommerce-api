# syntax=docker/dockerfile:1.7

# ─── Build stage ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for layer caching of NuGet restore
COPY ECommerceAPI/ECommerceAPI.csproj ECommerceAPI/
COPY tests/ECommerceAPI.Tests/ECommerceAPI.Tests.csproj tests/ECommerceAPI.Tests/
COPY ECommerceAPI.sln .
RUN dotnet restore ECommerceAPI.sln

# Copy the rest and publish
COPY ECommerceAPI/ ECommerceAPI/
COPY tests/ tests/
RUN dotnet publish ECommerceAPI/ECommerceAPI.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ─── Runtime stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Run as non-root for security
RUN groupadd -r app && useradd -r -g app app
COPY --from=build /app/publish .
RUN chown -R app:app /app
USER app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

# Health check (assumes /swagger is reachable; replace with /health when added)
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/swagger/v1/swagger.json || exit 1

ENTRYPOINT ["dotnet", "ECommerceAPI.dll"]
