# ── Stage 1: restore & build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /authorization

# Copy project files first for layer-cached restore
COPY /Authorization.Domain/Authorization.Domain.csproj           /Authorization.Domain/
COPY /Authorization.Application/Authorization.Application.csproj /Authorization.Application/
COPY /Authorization.Infrastructure/Authorization.Infrastructure.csproj /Authorization.Infrastructure/
COPY /Authorization.Api/Authorization.Api.csproj                  /Authorization.Api/

RUN dotnet restore /Authorization.Api/Authorization.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish /Authorization.Api/Authorization.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:7000

EXPOSE 7000

ENTRYPOINT ["dotnet", "Authorization.Api.dll"]
