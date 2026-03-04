using LanLinker.Core.Events;

namespace LanLinker.Core.Interfaces;

public interface IUdpBroadcastService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
    Task SendAsync(byte[] data);

    event EventHandler<NetworkErrorEventArgs>? NetworkError;
    event EventHandler<UdpMessageReceivedEventArgs> MessageReceived;
}