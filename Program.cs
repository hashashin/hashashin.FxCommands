using MacroDeck.Plugin.Hosting;
using MacroDeck.Plugin.Serilog;

var plugin = MacroDeckPlugin.CreatePlugin(args)
    .UseMacroDeckLogging()
    .RegisterIntegration<hashashin.FxCommands.PluginIntegration>()
    .Build();

await plugin.RunAsync();
