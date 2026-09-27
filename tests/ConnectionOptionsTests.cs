using NUnit.Framework;

namespace hashashin.FxCommands.Tests;

[TestFixture]
public sealed class ConnectionOptionsTests
{
    [Test]
    public void Missing_values_use_the_default_endpoint()
    {
        var valid = FxConnectionOptions.TryCreate(null, null, out var options, out var error);

        Assert.That(valid, Is.True);
        Assert.That(error, Is.Empty);
        Assert.That(options.Host, Is.EqualTo(FxConnectionOptions.DefaultHost));
        Assert.That(options.Port, Is.EqualTo(FxConnectionOptions.DefaultPort));
    }

    [Test]
    public void Custom_host_and_port_are_preserved()
    {
        var valid = FxConnectionOptions.TryCreate("fivem.local", 29300, out var options, out _);

        Assert.That(valid, Is.True);
        Assert.That(options.Endpoint, Is.EqualTo("fivem.local:29300"));
    }

    [Test]
    public void Disabled_custom_endpoint_ignores_saved_host_and_port()
    {
        var valid = FxConnectionOptions.TryCreate(
            false,
            "fivem.local",
            29300,
            out var options,
            out var error);

        Assert.That(valid, Is.True);
        Assert.That(error, Is.Empty);
        Assert.That(options.Endpoint, Is.EqualTo("127.0.0.1:29200"));
    }

    [TestCase("not a host", 29200)]
    [TestCase("127.0.0.1", 0)]
    [TestCase("127.0.0.1", 65536)]
    public void Invalid_endpoint_values_are_rejected(string host, int port)
    {
        var valid = FxConnectionOptions.TryCreate(host, port, out _, out var error);

        Assert.That(valid, Is.False);
        Assert.That(error, Is.Not.Empty);
    }
}
