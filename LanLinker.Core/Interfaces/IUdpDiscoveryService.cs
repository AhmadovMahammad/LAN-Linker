using LanLinker.Core.Models;

namespace LanLinker.Core.Interfaces;

public interface IUdpDiscoveryService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    Task DiscoverPeersAsync();
    Task AnnouncePresenceAsync();

    event Action<Peer> OnPeerDiscovered;
}