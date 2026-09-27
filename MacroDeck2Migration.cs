using System.Text.Json;
using MacroDeck.Sdk.Migration;

namespace hashashin.FxCommands;

internal sealed class MacroDeck2Migration : IIntegrationMigration
{
    private const string IntegrationId = "com.hashashin.fxcommands";

    public MigrationSource Source => MigrationSource.MacroDeck2;

    public IReadOnlyList<string> ClaimedActionSources { get; } = ["hashashin.FxCommands"];

    // Macro Deck 2 stored no plugin-level settings. Host and port are now
    // action parameters, so there is no settings source to claim here.
    public IReadOnlyList<string> ClaimedSettingsSources { get; } = [];

    public Task<ActionMigrationResult?> MigrateActionAsync(
        ForeignAction action,
        CancellationToken cancellationToken)
    {
        if (!IsFxCommandAction(action.TypeName))
        {
            return Task.FromResult<ActionMigrationResult?>(null);
        }

        var command = ReadString(action.Configuration, "command");
        if (string.IsNullOrWhiteSpace(command))
        {
            return Task.FromResult<ActionMigrationResult?>(null);
        }

        var parameters = new Dictionary<string, JsonElement>
        {
            ["command"] = JsonSerializer.SerializeToElement(command)
        };

        return Task.FromResult<ActionMigrationResult?>(new ActionMigrationResult(
            IntegrationId,
            "fx-command",
            action.DisplayName ?? "FxCommand",
            parameters));
    }

    public Task<IReadOnlyList<MigratedConfiguration>> MigrateConfigurationAsync(
        ForeignPluginSettings settings,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<MigratedConfiguration>>([]);
    }

    private static bool IsFxCommandAction(string typeName)
    {
        return typeName.Equals(
                   "hashashin.FxCommands.Main+FxCommands",
                   StringComparison.Ordinal) ||
               typeName.EndsWith(".Main+FxCommands", StringComparison.Ordinal);
    }

    private static string? ReadString(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.TryGetProperty(propertyName, out var value) &&
                   value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
