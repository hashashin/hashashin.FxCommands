namespace hashashin.FxCommands;

internal static class CommandNormalizer
{
    public static string? Normalize(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        var normalized = command.Trim();
        if (normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = normalized[1..].TrimStart();
        }

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
