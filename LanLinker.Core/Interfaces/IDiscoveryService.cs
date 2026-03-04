using LanLinker.Core.Events;

namespace LanLinker.Core.Interfaces;

public interface IDiscoveryService
{
    Task StartAsync(CancellationToken cancellationToken);

    event EventHandler<PeerEventArgs> PeerAnnounced;
}