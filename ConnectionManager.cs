// MIT License

// Copyright (c) 2022 EggRP

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Net.Sockets;

namespace hashashin.FxCommands;

internal sealed class ConnectionManager
{
    public async Task SendMessageAsync(
        string message,
        FxConnectionOptions options,
        CancellationToken cancellationToken)
    {
        var data = FxCommandPacket.Build(message);
        using var timeout = CreateTimeoutTokenSource(options.Timeout, cancellationToken);

        using var tcpClient = new TcpClient
        {
            NoDelay = true
        };

        try
        {
            await tcpClient.ConnectAsync(options.Host, options.Port, timeout.Token).ConfigureAwait(false);
            await using var tcpStream = tcpClient.GetStream();
            await tcpStream.WriteAsync(data, timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
    }

    public async Task TestConnectionAsync(
        FxConnectionOptions options,
        CancellationToken cancellationToken)
    {
        using var timeout = CreateTimeoutTokenSource(options.Timeout, cancellationToken);
        using var tcpClient = new TcpClient
        {
            NoDelay = true
        };

        try
        {
            await tcpClient.ConnectAsync(options.Host, options.Port, timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException();
        }
    }

    private static CancellationTokenSource CreateTimeoutTokenSource(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        source.CancelAfter(timeout);
        return source;
    }
}
