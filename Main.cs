using System.Net.Sockets;
using MacroDeck.Localization;
using MacroDeck.Sdk;
using MacroDeck.Sdk.Actions;
using MacroDeck.Sdk.Migration;

namespace hashashin.FxCommands;

internal sealed class PluginIntegration : IPluginIntegration, IMigrationProvider
{
    public IReadOnlyList<IActionDefinition> Actions { get; } =
    [
        new FxCommandAction(),
        new TestConnectionAction()
    ];

    public IReadOnlyList<IIntegrationMigration> Migrations { get; } = [new MacroDeck2Migration()];

    public Task InitializeAsync(IIntegrationContext context) => Task.CompletedTask;

    public Task ShutdownAsync() => Task.CompletedTask;
}

internal sealed class FxCommandAction : IActionDefinition
{
    public string Id => "fx-command";

    public LocalizedText Name => "FxCommand";

    public LocalizedText Description =>
        "Send a command to the FiveM/RedM client console. A leading slash is optional.";

    public IReadOnlyList<ActionParameter> Parameters { get; } =
    [
        ActionParameter.Text(
            "command",
            label: "Command",
            placeholder: "e wave or /e wave",
            description: "The command to send to the FiveM/RedM console.",
            required: true),
        ActionParameter.Toggle(
            "customEndpoint",
            label: "Use custom endpoint",
            description: "Enable this to override the default 127.0.0.1:29200 endpoint.",
            defaultValue: false),
        ActionParameter.Text(
            "host",
            label: "Host",
            placeholder: FxConnectionOptions.DefaultHost,
            description: "FiveM/RedM console host. Default: 127.0.0.1.",
            defaultValue: FxConnectionOptions.DefaultHost)
            .OnlyWhen("customEndpoint", "true"),
        ActionParameter.Number(
            "port",
            min: 1,
            max: 65535,
            label: "Port",
            description: "FiveM/RedM console port. Default: 29200.",
            defaultValue: FxConnectionOptions.DefaultPort)
            .OnlyWhen("customEndpoint", "true")
    ];

    public IActionExecutor CreateExecutor() => new Executor();

    private sealed class Executor : IActionExecutor
    {
        public async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            var command = CommandNormalizer.Normalize(
                context.Parameters.GetValueOrDefault("command") as string);
            if (command is null)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.InvalidParameter,
                    "A command is required. Example: e wave.");
            }

            var useCustomEndpoint = !context.Parameters.ContainsKey("customEndpoint") ||
                                    FxConnectionOptions.IsEnabled(
                                        context.Parameters.GetValueOrDefault("customEndpoint"));

            if (!FxConnectionOptions.TryCreate(
                    useCustomEndpoint,
                    context.Parameters.GetValueOrDefault("host"),
                    context.Parameters.GetValueOrDefault("port"),
                    out var options,
                    out var optionsError))
            {
                return ActionResult.Failed(ActionErrorCodes.InvalidParameter, optionsError);
            }

            try
            {
                await new ConnectionManager()
                    .SendMessageAsync(command, options, context.CancellationToken);
                return ActionResult.Success();
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.Timeout,
                    $"The FiveM/RedM console did not respond at {options.Endpoint}.");
            }
            catch (SocketException)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.NotConnected,
                    $"The FiveM/RedM console is not reachable at {options.Endpoint}.");
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

internal sealed class TestConnectionAction : IActionDefinition
{
    public string Id => "test-connection";

    public LocalizedText Name => "Test FiveM connection";

    public LocalizedText Description =>
        "Check whether a FiveM/RedM client console is reachable at the configured host and port.";

    public IReadOnlyList<ActionParameter> Parameters { get; } =
    [
        ActionParameter.Toggle(
            "customEndpoint",
            label: "Use custom endpoint",
            description: "Enable this to override the default 127.0.0.1:29200 endpoint.",
            defaultValue: false),
        ActionParameter.Text(
            "host",
            label: "Host",
            placeholder: FxConnectionOptions.DefaultHost,
            description: "FiveM/RedM console host. Default: 127.0.0.1.",
            defaultValue: FxConnectionOptions.DefaultHost)
            .OnlyWhen("customEndpoint", "true"),
        ActionParameter.Number(
            "port",
            min: 1,
            max: 65535,
            label: "Port",
            description: "FiveM/RedM console port. Default: 29200.",
            defaultValue: FxConnectionOptions.DefaultPort)
            .OnlyWhen("customEndpoint", "true")
    ];

    public IActionExecutor CreateExecutor() => new Executor();

    private sealed class Executor : IActionExecutor
    {
        public async Task<ActionResult> ExecuteAsync(ActionExecutionContext context)
        {
            var useCustomEndpoint = !context.Parameters.ContainsKey("customEndpoint") ||
                                    FxConnectionOptions.IsEnabled(
                                        context.Parameters.GetValueOrDefault("customEndpoint"));

            if (!FxConnectionOptions.TryCreate(
                    useCustomEndpoint,
                    context.Parameters.GetValueOrDefault("host"),
                    context.Parameters.GetValueOrDefault("port"),
                    out var options,
                    out var optionsError))
            {
                return ActionResult.Failed(ActionErrorCodes.InvalidParameter, optionsError);
            }

            try
            {
                await new ConnectionManager()
                    .TestConnectionAsync(options, context.CancellationToken);
                return ActionResult.Success($"Connected to {options.Endpoint}.");
            }
            catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (TimeoutException)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.Timeout,
                    $"The FiveM/RedM console did not respond at {options.Endpoint}.");
            }
            catch (SocketException)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.NotConnected,
                    $"The FiveM/RedM console is not reachable at {options.Endpoint}.");
            }
            catch (Exception)
            {
                return ActionResult.Failed(
                    ActionErrorCodes.ProviderError,
                    "The FiveM/RedM connection could not be tested.");
            }
        }
    }
}
