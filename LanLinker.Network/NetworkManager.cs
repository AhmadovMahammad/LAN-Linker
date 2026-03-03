using LanLinker.Core.Events;
using LanLinker.Core.Models;
using LanLinker.Network.Services;

namespace LanLinker.Network;

public class NetworkManager : IDisposable
{
    private readonly PeerManager _peerManager;
    private readonly UdpDiscoveryService _udpService;

    public NetworkManager(Identity identity)
    {
        _udpService = new UdpDiscoveryService(identity);
        _peerManager = new PeerManager(identity.DeviceId);

        _udpService.PeerAnnounced += (_, args) => _peerManager.HandlePeerDiscovery(args.Peer);

        _peerManager.PeerConnected += (_, e) => PeerConnected?.Invoke(this, e);
        _peerManager.PeerDisconnected += (_, e) => PeerDisconnected?.Invoke(this, e);
        _udpService.NetworkError += (_, e) => NetworkError?.Invoke(this, e);
    }

    public void Dispose()
    {
        _peerManager.Dispose();
    }

    public event EventHandler<PeerEventArgs>? PeerConnected;
    public event EventHandler<PeerEventArgs>? PeerDisconnected;
    public event EventHandler<NetworkErrorEventArgs>? NetworkError;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _udpService.StartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        await _udpService.StopAsync();
    }
}