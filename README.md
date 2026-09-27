# FxCommands

Macro Deck 3 plugin for sending commands to the FiveM/RedM client console.

The plugin uses the FiveM console protocol from
[EggRP/fxcommands](https://github.com/EggRP/fxcommands). Every command action
uses `127.0.0.1:29200` by default. Enable `Use custom endpoint` only when the
FiveM client is on another host or port; the Host and Port fields then appear.
Commands may be written with or without the leading `/`.

## Actions

- `FxCommand` sends one command. Examples: `e wave`, `/e wave`, or `me waves`.
- `Test FiveM connection` checks the configured host and port without sending a
  command.

The protocol packet is built from UTF-8 bytes, so commands containing accents
or other non-ASCII characters are framed with the correct length.

Existing Macro Deck 2 FxCommands buttons are migrated to `FxCommand`; their
host and port use the new defaults unless changed in the action settings.

## Build and test

This plugin targets .NET 10 and uses the Macro Deck 3 beta 14 SDK:

```powershell
dotnet build
dotnet test
```

To run it against the disposable Macro Deck 3 stub host, install the prerelease
CLI and run:

```powershell
dotnet tool install --global MacroDeck.Plugin.Cli --prerelease
macrodeck-plugin run --project hashashin.FxCommands.csproj --stub-host
```
