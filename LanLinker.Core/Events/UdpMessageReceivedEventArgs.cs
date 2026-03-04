using System.Net;

namespace LanLinker.Core.Events;

public class UdpMessageReceivedEventArgs(byte[] buffer, IPEndPoint ipEndPoint) : EventArgs
{
    public byte[] Buffer { get; } = buffer;
    public IPEndPoint IpEndPoint { get; } = ipEndPoint;
}