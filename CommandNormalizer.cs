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

        // FiveM treats ';' as a command-chain separator. Accept the natural
        // "command ; next-command" spelling as well as "command;next-command".
        normalized = System.Text.RegularExpressions.Regex.Replace(
            normalized,
            @"\s*;\s*",
            ";");

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
