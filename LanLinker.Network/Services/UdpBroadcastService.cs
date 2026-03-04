using System.Net;
using System.Net.Sockets;
using LanLinker.Core.Events;
using LanLinker.Core.Interfaces;

namespace LanLinker.Network.Services;

public class UdpBroadcastService : IUdpBroadcastService
{
    private readonly IPEndPoint _broadcastEndpoint = new(IPAddress.Broadcast, 5000);

    private readonly IPEndPoint _listerEndpoint = new(IPAddress.Any, 5000);

    private UdpClient? _udpBroadcaster;

    private UdpClient? _udpListener;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _udpListener = new UdpClient();

        _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        _udpListener.Client.Bind(_listerEndpoint);

        _udpBroadcaster = new UdpClient();

        _udpBroadcaster.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        _ = Task.Run(() => StartListeningLoop(cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _udpListener?.Dispose();

        _udpBroadcaster?.Dispose();

        return Task.CompletedTask;
    }

    public async Task SendAsync(byte[] data)
    {
        try
        {
            if (_udpBroadcaster != null)
            {
                await _udpBroadcaster.SendAsync(data, _broadcastEndpoint);
            }
        }
        catch (Exception exception)
        {
            NetworkError?.Invoke
            (
                this,
                new NetworkErrorEventArgs(exception, "[UDP SEND ERROR] An error occurred while sending data.")
            );
        }
    }

    public event EventHandler<NetworkErrorEventArgs>? NetworkError;

    public event EventHandler<UdpMessageReceivedEventArgs>? MessageReceived;

    private async Task StartListeningLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_udpListener == null)
                {
                    continue;
                }

                UdpReceiveResult udpReceiveResult = await _udpListener.ReceiveAsync(cancellationToken);

                MessageReceived?.Invoke
                (
                    this,
                    new UdpMessageReceivedEventArgs
                    (
                        udpReceiveResult.Buffer, udpReceiveResult.RemoteEndPoint
                    )
                );
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            NetworkError?.Invoke
            (
                this,
                new NetworkErrorEventArgs(exception, "[UDP RECEIVE ERROR] An error occurred while listening for data.")
            );
        }
    }
}