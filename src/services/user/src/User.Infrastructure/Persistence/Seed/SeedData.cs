using User.Domain.Entities;

namespace User.Infrastructure.Persistence.Seed;

/// <summary>Deterministic seed data: 10 user profiles with assorted roles.</summary>
public static class SeedData
{
    public static readonly DateTime SeededUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public sealed record SeedUser(
        Guid UserId,
        string Email,
        string FirstName,
        string LastName,
        string Department,
        string[] Roles);

    public static readonly SeedUser[] Users =
    [
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "alice@dashboard.local",   "Alice",   "Walker",   "Engineering", [Roles.Dev]),
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "bob@dashboard.local",     "Bob",     "Stone",    "Engineering", [Roles.Dev]),
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "carol@dashboard.local",   "Carol",   "Nguyen",   "QA",          [Roles.QA]),
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "dave@dashboard.local",    "Dave",    "Patel",    "QA",          [Roles.QA, Roles.Dev]),
        new(Guid.Parse("55555555-5555-5555-5555-555555555555"), "erin@dashboard.local",    "Erin",    "Lopez",    "Product",     [Roles.PO]),
        new(Guid.Parse("66666666-6666-6666-6666-666666666666"), "frank@dashboard.local",   "Frank",   "Murphy",   "Product",     [Roles.PO]),
        new(Guid.Parse("77777777-7777-7777-7777-777777777777"), "grace@dashboard.local",   "Grace",   "Kim",      "Engineering", [Roles.Lead]),
        new(Guid.Parse("88888888-8888-8888-8888-888888888888"), "henry@dashboard.local",   "Henry",   "Adams",    "Engineering", [Roles.Dev, Roles.Lead]),
        new(Guid.Parse("99999999-9999-9999-9999-999999999999"), "irene@dashboard.local",   "Irene",   "Costa",    "Engineering", [Roles.Dev]),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "jack@dashboard.local",    "Jack",    "Owens",    "QA",          [Roles.QA])
    ];
}
