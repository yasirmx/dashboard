using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using User.Api;
using User.Application;
using User.Infrastructure;
using User.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Application & Infrastructure ──────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Controllers ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ── OpenAPI (built-in .NET 10) + Scalar UI ────────────────────────────────────
builder.Services.AddOpenApi();

// ── Problem Details + global exception mapping ────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// ── Auto-migrate on startup ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersContext>();
    await db.Database.MigrateAsync();
}

// ── Middleware Pipeline ────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                      // /openapi/v1.json
    app.MapScalarApiReference();           // /scalar/v1  (Swagger UI replacement)
}

app.UseExceptionHandler();
app.MapControllers();

app.Run();
