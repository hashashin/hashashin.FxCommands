using System.Globalization;
using System.Text.Json;

namespace hashashin.FxCommands;

internal sealed record FxConnectionOptions(string Host, int Port, TimeSpan Timeout)
{
    public const string DefaultHost = "127.0.0.1";
    public const int DefaultPort = 29200;
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

    public string Endpoint => $"{Host}:{Port}";

    public static FxConnectionOptions Default { get; } =
        new(DefaultHost, DefaultPort, DefaultTimeout);

    public static bool IsEnabled(object? value)
    {
        if (value is bool boolean)
        {
            return boolean;
        }

        if (value is string text && bool.TryParse(text, out var parsed))
        {
            return parsed;
        }

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(element.GetString(), out var parsedElement) => parsedElement,
                _ => false
            };
        }

        return false;
    }

    public static bool TryCreate(
        bool useCustomEndpoint,
        object? hostValue,
        object? portValue,
        out FxConnectionOptions options,
        out string error)
    {
        if (!useCustomEndpoint)
        {
            options = Default;
            error = string.Empty;
            return true;
        }

        return TryCreate(hostValue, portValue, out options, out error);
    }

    public static bool TryCreate(
        object? hostValue,
        object? portValue,
        out FxConnectionOptions options,
        out string error)
    {
        var host = ReadString(hostValue)?.Trim();
        if (string.IsNullOrWhiteSpace(host))
        {
            host = DefaultHost;
        }

        if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
        {
            options = Default;
            error = "The host is not valid.";
            return false;
        }

        if (!TryReadPort(portValue, out var port))
        {
            options = Default;
            error = "The port must be a number between 1 and 65535.";
            return false;
        }

        options = new FxConnectionOptions(host, port, DefaultTimeout);
        error = string.Empty;
        return true;
    }

    private static bool TryReadPort(object? value, out int port)
    {
        if (value is null)
        {
            port = DefaultPort;
            return true;
        }

        if (value is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out port))
            {
                return IsValidPort(port);
            }

            value = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        }

        if (value is string text &&
            int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
        {
            return IsValidPort(port);
        }

        try
        {
            port = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return IsValidPort(port);
        }
        catch (FormatException)
        {
            port = 0;
            return false;
        }
        catch (InvalidCastException)
        {
            port = 0;
            return false;
        }
        catch (OverflowException)
        {
            port = 0;
            return false;
        }
    }

    private static string? ReadString(object? value)
    {
        if (value is string text)
        {
            return text;
        }

        if (value is JsonElement element && element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        return value?.ToString();
    }

    private static bool IsValidPort(int port) => port is >= 1 and <= 65535;
}
