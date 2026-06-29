namespace User.Domain.Entities;

/// <summary>The canonical set of assignable roles (source of truth for authorization).</summary>
public static class Roles
{
    public const string Dev = "Dev";
    public const string QA = "QA";
    public const string PO = "PO";
    public const string Lead = "Lead";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Dev, QA, PO, Lead };

    public static bool IsValid(string role) => All.Contains(role);
}
