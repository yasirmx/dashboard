# ── Stage 1: restore & build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer-cached restore
COPY src/Authorization.Domain/Authorization.Domain.csproj           src/Authorization.Domain/
COPY src/Authorization.Application/Authorization.Application.csproj src/Authorization.Application/
COPY src/Authorization.Infrastructure/Authorization.Infrastructure.csproj src/Authorization.Infrastructure/
COPY src/Authorization.Api/Authorization.Api.csproj                  src/Authorization.Api/

RUN dotnet restore src/Authorization.Api/Authorization.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish src/Authorization.Api/Authorization.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:7000

EXPOSE 7000

ENTRYPOINT ["dotnet", "Authorization.Api.dll"]
