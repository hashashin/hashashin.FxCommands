# FxCommands

Macro Deck 3 plugin for sending commands to the FiveM/RedM client console.

The plugin sends the configured command to `127.0.0.1:29200`, using the
protocol from [EggRP/fxcommands](https://github.com/EggRP/fxcommands). Commands
should not include the leading `/`.

## Build

This plugin targets .NET 10 and uses the Macro Deck 3 hosting SDK:

```powershell
dotnet build
```

To run it against the disposable Macro Deck 3 stub host, install the prerelease
CLI and run:

```powershell
dotnet tool install --global MacroDeck.Plugin.Cli --prerelease
macrodeck-plugin run --project hashashin.FxCommands.csproj --stub-host
```

The `manifest.json` includes a Macro Deck 2 settings migration for existing
FxCommands buttons.
