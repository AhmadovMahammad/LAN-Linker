using LanLinker.Core.Models;

namespace LanLinker.Core.Interfaces;

public interface IUdpDiscoveryService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);

    event Action<Peer> OnPeerDiscovered;
    event Action<Exception> OnCriticalError;
}