using System.Buffers.Binary;
using System.Text;

namespace hashashin.FxCommands;

internal static class FxCommandPacket
{
    private static readonly byte[] MagicAndProtocol = [0x43, 0x4d, 0x4e, 0x44, 0x00, 0xd3];

    public static byte[] Build(string message)
    {
        var command = Encoding.UTF8.GetBytes(message + "\n");
        var packet = new byte[MagicAndProtocol.Length + sizeof(int) + 2 + command.Length + 1];

        MagicAndProtocol.CopyTo(packet, 0);

        // DevCon's CMND length is the UTF-8 command payload plus its trailing NUL.
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(MagicAndProtocol.Length, sizeof(int)), command.Length + 1);

        command.CopyTo(packet, MagicAndProtocol.Length + sizeof(int) + 2);
        return packet;
    }
}
