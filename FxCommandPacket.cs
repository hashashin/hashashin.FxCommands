using System.Buffers.Binary;
using System.Text;

namespace hashashin.FxCommands;

internal static class FxCommandPacket
{
    private static readonly byte[] Header = [0x43, 0x4d, 0x4e, 0x44, 0x00, 0xd2, 0x00, 0x00];

    public static byte[] Build(string message)
    {
        var command = Encoding.UTF8.GetBytes(message + "\n");
        var packet = new byte[Header.Length + sizeof(int) + 2 + command.Length + 1];

        Header.CopyTo(packet, 0);

        // The protocol length is the old ASCII length formula (message + 13),
        // but it must use the number of UTF-8 bytes for non-ASCII commands.
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(Header.Length, sizeof(int)), command.Length + 12);

        command.CopyTo(packet, Header.Length + sizeof(int) + 2);
        return packet;
    }
}
