using LanLinker.Core.Events;
using LanLinker.Core.Interfaces;
using LanLinker.Core.Models;
using LanLinker.Network.Services;

namespace LanLinker.Network;

public class NetworkManager : IDisposable
{
    private readonly IDiscoveryService _discoveryService;

    private readonly IGlobalChatService _globalChatService;

    private readonly PeerManager _peerManager;

    private readonly IUdpBroadcastService _udpBroadcastService;

    public NetworkManager(Identity identity)
    {
        _udpBroadcastService = new UdpBroadcastService();
        _udpBroadcastService.NetworkError += (_, e) => NetworkError?.Invoke(this, e);

        _peerManager = new PeerManager(identity.DeviceId);
        _peerManager.PeerConnected += (_, e) => PeerConnected?.Invoke(this, e);
        _peerManager.PeerDisconnected += (_, e) => PeerDisconnected?.Invoke(this, e);

        _discoveryService = new DiscoveryService(identity, _udpBroadcastService);
        _discoveryService.PeerAnnounced += (_, args) => _peerManager.HandlePeerDiscovery(args.Peer);

        _globalChatService = new GlobalChatService(identity, _udpBroadcastService);
        _globalChatService.GlobalMessageReceived += (_, e) => GlobalMessageReceived?.Invoke(this, e);
    }

    public IReadOnlyDictionary<string, Peer> Peers => _peerManager.Peers;

    public void Dispose()
    {
        _peerManager.Dispose();
    }

    public event EventHandler<PeerEventArgs>? PeerConnected;

    public event EventHandler<PeerEventArgs>? PeerDisconnected;

    public event EventHandler<NetworkErrorEventArgs>? NetworkError;

    public event EventHandler<GlobalChatMessageEventArgs>? GlobalMessageReceived;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _udpBroadcastService.StartAsync(cancellationToken);

        await _discoveryService.StartAsync(cancellationToken);
    }

    public async Task StopAsync()
    {
        await _udpBroadcastService.StopAsync();
    }
}