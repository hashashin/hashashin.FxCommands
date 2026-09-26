// MIT License

// Copyright (c) 2022 EggRP

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Net.Sockets;
using System.Text;

namespace hashashin.FxCommands;

internal sealed class ConnectionManager
{
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        const string ipAddress = "127.0.0.1";
        const int port = 29200; // FiveM/RedM console

        byte[] header = "43:4d:4e:44:00:d2:00:00"
            .Split(':')
            .Select(s => byte.Parse(s, System.Globalization.NumberStyles.HexNumber))
            .ToArray(); // CMND 0x00d20000
        byte[] command = Encoding.UTF8.GetBytes(message + "\n");
        byte[] padding = [0, 0];
        byte[] length = BitConverter.GetBytes(message.Length + 13);
        byte[] terminator = [0];

        Array.Reverse(length); // flip flop

        byte[] data = header
            .Concat(length)
            .Concat(padding)
            .Concat(command)
            .Concat(terminator)
            .ToArray();

        using var tcpClient = new TcpClient
        {
            NoDelay = true
        };

        await tcpClient.ConnectAsync(ipAddress, port, cancellationToken).ConfigureAwait(false);
        await using var tcpStream = tcpClient.GetStream();
        await tcpStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }
}
