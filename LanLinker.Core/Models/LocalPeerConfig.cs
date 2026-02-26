namespace LanLinker.Core.Models;

public record LocalPeerConfig(
    string DeviceId,
    string DeviceName,
    string UserName,
    int Port = 5000);