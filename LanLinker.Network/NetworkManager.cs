using LanLinker.Core.Events;
using LanLinker.Core.Interfaces;
using LanLinker.Core.Models;
using LanLinker.Network.Services;

namespace LanLinker.Network;

public class NetworkManager : IDisposable
{
    private readonly PeerManager _peerManager;
    private readonly IUdpDiscoveryService _udpService;

    public NetworkManager(LocalPeerConfig config)
    {
        _udpService = new UdpDiscoveryService(config);
        _peerManager = new PeerManager(config.DeviceId);

        _udpService.PeerAnnounced += (_, args) => _peerManager.HandlePeerDiscovery(args.Peer);

        _peerManager.PeerConnected += (_, e) => PeerConnected?.Invoke(this, e);
        _peerManager.PeerDisconnected += (_, e) => PeerDisconnected?.Invoke(this, e);
        _udpService.NetworkError += (_, e) => NetworkError?.Invoke(this, e);
    }

    public IReadOnlyList<Peer> GetActivePeers()
    {
        return _peerManager.Peers();
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

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _udpService.StopAsync();
    }
}