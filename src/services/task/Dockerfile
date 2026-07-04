# ── Stage 1: restore & build ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /task

# Copy project files first for layer-cached restore
COPY /Tasks.Domain/Tasks.Domain.csproj             /Tasks.Domain/
COPY /Tasks.Application/Tasks.Application.csproj   /Tasks.Application/
COPY /Tasks.Infrastructure/Tasks.Infrastructure.csproj /Tasks.Infrastructure/
COPY /Tasks.Api/Tasks.Api.csproj                   /Tasks.Api/

RUN dotnet restore /Tasks.Api/Tasks.Api.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish /Tasks.Api/Tasks.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ── Stage 2: runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:7002

EXPOSE 7002

ENTRYPOINT ["dotnet", "Tasks.Api.dll"]
