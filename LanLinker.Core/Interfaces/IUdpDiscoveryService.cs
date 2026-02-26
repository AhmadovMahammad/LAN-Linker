using LanLinker.Core.Events;

namespace LanLinker.Core.Interfaces;

public interface IUdpDiscoveryService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();

    event EventHandler<PeerEventArgs> PeerAnnounced;
    event EventHandler<NetworkErrorEventArgs> NetworkError;
}