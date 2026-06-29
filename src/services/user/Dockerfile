# ── Stage 1: restore & build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /user

# Copy project files first for layer-cached restore
COPY /User.Domain/User.Domain.csproj                 /User.Domain/
COPY /User.Application/User.Application.csproj        /User.Application/
COPY /User.Infrastructure/User.Infrastructure.csproj /User.Infrastructure/
COPY /User.Api/User.Api.csproj                        /User.Api/

RUN dotnet restore /User.Api/User.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish /User.Api/User.Api.csproj \
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
