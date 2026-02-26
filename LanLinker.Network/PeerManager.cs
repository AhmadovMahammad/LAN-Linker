using System.Collections.Concurrent;
using LanLinker.Core;
using LanLinker.Core.Events;
using LanLinker.Core.Models;

namespace LanLinker.Network;

public class PeerManager : IDisposable
{
    private readonly Timer _cleanupTimer;
    private readonly string _localDeviceId;
    private readonly ConcurrentDictionary<string, Peer> _peers = new();

    public PeerManager(string localDeviceId)
    {
        _localDeviceId = localDeviceId;
        _cleanupTimer = new Timer(CleanupStalePeers, null, AppSettings.CleanupDueTime, AppSettings.CleanupPeriodTime);
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }

    public event EventHandler<PeerEventArgs>? PeerConnected;
    public event EventHandler<PeerEventArgs>? PeerDisconnected;

    public IReadOnlyList<Peer> Peers()
    {
        return _peers.Values.ToList();
    }

    private void CleanupStalePeers(object? state)
    {
        List<Peer> stalePeers = _peers.Values.Where(p => !p.IsAlive()).ToList();

        foreach (var peer in stalePeers)
        {
            if (_peers.TryRemove(peer.DeviceId, out var removedPeer))
            {
                PeerDisconnected?.Invoke(this, new PeerEventArgs(removedPeer));
            }
        }
    }

    public void HandlePeerDiscovery(Peer peer)
    {
        if (peer.DeviceId == _localDeviceId)
        {
            return;
        }

        bool newPeer = !_peers.ContainsKey(peer.DeviceId);

        _peers.AddOrUpdate(
            peer.DeviceId,
            peer,
            (_, existingPeer) =>
            {
                existingPeer.UpdateLastSeenAt();
                existingPeer.IpAddress = peer.IpAddress;
                return existingPeer;
            });

        if (newPeer)
        {
            PeerConnected?.Invoke(this, new PeerEventArgs(peer));
        }
    }
}