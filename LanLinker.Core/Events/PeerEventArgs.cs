using LanLinker.Core.Models;

namespace LanLinker.Core.Events;

public class PeerEventArgs(Peer peer) : EventArgs
{
    public Peer Peer { get; } = peer;
}