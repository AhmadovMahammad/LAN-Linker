// See https://aka.ms/new-console-template for more information

using LanLinker.Core.Interfaces;
using LanLinker.Network.Services;

namespace LanLinker.Network.Test;

internal abstract class Program
{
    private static readonly string DeviceId = Guid.NewGuid().ToString();
    private static readonly string DeviceName = Environment.MachineName;
    private static readonly string UserName = "ahmadov.dev";

    public static async Task Main(string[] args)
    {
        IUdpDiscoveryService discoveryService = new UdpDiscoveryService(DeviceId, DeviceName, UserName);

        discoveryService.OnPeerDiscovered += peer => { Console.WriteLine($"New peer found.\n{peer}\n\n"); };

        CancellationTokenSource cts = new CancellationTokenSource();

        await discoveryService.StartAsync(cts.Token);

        await Task.Delay(-1, cts.Token);
    }
}