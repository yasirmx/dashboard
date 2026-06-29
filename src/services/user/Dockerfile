# ── Stage 1: restore & build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer-cached restore
COPY src/User.Domain/User.Domain.csproj                 src/User.Domain/
COPY src/User.Application/User.Application.csproj        src/User.Application/
COPY src/User.Infrastructure/User.Infrastructure.csproj src/User.Infrastructure/
COPY src/User.Api/User.Api.csproj                        src/User.Api/

RUN dotnet restore src/User.Api/User.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish src/User.Api/User.Api.csproj \
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

ENTRYPOINT ["dotnet", "User.Api.dll"]
