namespace LanLinker.Core.Models;

public class Peer
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; }

    public DateTime LastSeenAt { get; set; }

    public bool IsAlive()
    {
        return LastSeenAt.AddSeconds(AppSettings.PeerTimeout.Seconds) > DateTime.UtcNow;
    }

    public void UpdateLastSeenAt()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"{UserName} [{DeviceId}] - [{IpAddress}:{Port}]";
    }
}