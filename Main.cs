using System.Net.Sockets;
using MacroDeck.Localization;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.Migration;

namespace hashashin.FxCommands;

internal sealed class PluginIntegration : IPluginIntegration, IMigrationProvider
{
    public IReadOnlyList<IActionDefinition> Actions { get; } = [new FxCommandAction()];

    public IReadOnlyList<IIntegrationMigration> Migrations { get; } = [new MacroDeck2Migration()];

    public Task InitializeAsync(IIntegrationContext context) => Task.CompletedTask;

    public Task ShutdownAsync() => Task.CompletedTask;
}

internal sealed class FxCommandAction : IActionDefinition
{
    public string Id => "fx-command";

    public LocalizedText Name => "FxCommand";

    public LocalizedText Description =>
        "Send a command to the FiveM/RedM client console without the leading slash.";

    public IReadOnlyList<ActionParameter> Parameters { get; } =
    [
        ActionParameter.Text("command", label: "Command", required: true)
    ];

    public IActionExecutor CreateExecutor() => new Executor();

    private sealed class Executor : IActionExecutor
    {
        public async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            if (!context.Parameters.TryGetValue("command", out var value) ||
                value is not string command ||
                string.IsNullOrWhiteSpace(command))
            {
                return ActionResult.Failed(
                    ActionErrorCodes.InvalidParameter,
                    "A command is required.");
            }

            try
            {
                await new ConnectionManager().SendMessageAsync(command, context.CancellationToken);
                return ActionResult.Success();
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (SocketException)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.NotConnected,
                    "The FiveM/RedM client console is not reachable on 127.0.0.1:29200.");
            }
            catch (Exception)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.ProviderError,
                    "The command could not be sent to the FiveM/RedM client console.");
            }
        }
    }
}
