using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;

namespace hashashin.FxCommands.Tests;

[TestFixture]
public sealed class CommandNormalizerTests
{
    [TestCase("e wave", "e wave")]
    [TestCase(" /e wave ", "e wave")]
    [TestCase("/ e wave", "e wave")]
    [TestCase("me a long action ; e smoke", "me a long action;e smoke")]
    public void Normalize_accepts_commands_with_or_without_a_leading_slash(
        string input,
        string expected)
    {
        Assert.That(CommandNormalizer.Normalize(input), Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("/")]
    public void Normalize_rejects_blank_commands(string? input)
    {
        Assert.That(CommandNormalizer.Normalize(input), Is.Null);
    }
}

[TestFixture]
public sealed class FxCommandPacketTests
{
    [Test]
    public void Build_uses_utf8_byte_length_and_big_endian_protocol_header()
    {
        const string message = "e café";
        var command = Encoding.UTF8.GetBytes(message + "\n");
        var packet = FxCommandPacket.Build(message);

        Assert.That(packet[..6], Is.EqualTo(new byte[] { 0x43, 0x4d, 0x4e, 0x44, 0x00, 0xd3 }));
        Assert.That(BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(6, 4)), Is.EqualTo(command.Length + 1));
        Assert.That(packet[12..^1], Is.EqualTo(command));
        Assert.That(packet[^1], Is.EqualTo((byte)0));
    }

    [Test]
    public void Build_preserves_a_long_utf8_command_without_truncation()
    {
        var message = "me " + new string('x', 4000) + " áéíóú";
        var expectedCommand = Encoding.UTF8.GetBytes(message + "\n");
        var packet = FxCommandPacket.Build(message);

        Assert.That(packet[12..^1], Is.EqualTo(expectedCommand));
        Assert.That(BinaryPrimitives.ReadInt32BigEndian(packet.AsSpan(6, 4)), Is.EqualTo(expectedCommand.Length + 1));
    }
}

[TestFixture]
public sealed class ConnectionManagerTests
{
    [Test]
    public async Task SendMessageAsync_writes_a_complete_packet_to_the_configured_endpoint()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var acceptTask = listener.AcceptTcpClientAsync();
        var options = new FxConnectionOptions("127.0.0.1", port, TimeSpan.FromSeconds(2));

        await new ConnectionManager().SendMessageAsync("/e café", options, CancellationToken.None);

        using var client = await acceptTask.WaitAsync(TimeSpan.FromSeconds(2));
        await using var stream = client.GetStream();
        var expected = FxCommandPacket.Build("/e café");
        var received = new byte[expected.Length];
        await stream.ReadExactlyAsync(received);

        Assert.That(received, Is.EqualTo(expected));
    }

    [Test]
    public async Task TestConnectionAsync_succeeds_when_the_endpoint_accepts_connections()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var acceptTask = listener.AcceptTcpClientAsync();
        var options = new FxConnectionOptions("127.0.0.1", port, TimeSpan.FromSeconds(2));

        await new ConnectionManager().TestConnectionAsync(options, CancellationToken.None);

        using var accepted = await acceptTask.WaitAsync(TimeSpan.FromSeconds(2));
    }
}
