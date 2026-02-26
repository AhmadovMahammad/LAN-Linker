using LanLinker.Core.Interfaces;
using LanLinker.Core.Models;
using LanLinker.Network.Services;

namespace LanLinker.Network.Test;

internal abstract class Program
{
    private static readonly string DeviceId = Guid.NewGuid().ToString();
    private static readonly string DeviceName = Environment.MachineName;
    private static string _userName = "ahmadov.dev";

    private static readonly HashSet<string> DiscoveredDevices = [];

    public static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            DeserializeArguments(args);
        }

        LocalPeerConfig peerConfig = new LocalPeerConfig(DeviceId, DeviceName, _userName);

        IUdpDiscoveryService discoveryService = new UdpDiscoveryService(peerConfig);

        CancellationTokenSource cts = new CancellationTokenSource();

        discoveryService.PeerAnnounced += (_, eventArgs) =>
        {
            Peer peer = eventArgs.Peer;

            lock (DiscoveredDevices)
            {
                if (!DiscoveredDevices.Add(peer.DeviceId))
                {
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] New Peer Discovered!");
                Console.WriteLine($"User:    {peer.UserName}");
                Console.WriteLine($"Device:  {peer.DeviceName}");
                Console.ResetColor();
            }
        };

        discoveryService.NetworkError += (_, eventArgs) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\n[CRITICAL ERROR] {eventArgs.Context}");
            Console.ResetColor();

            cts.Cancel();
        };

        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("\n\n[System] Stopping requested...");

            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            await discoveryService.StartAsync(cts.Token);
            await Task.Delay(-1, cts.Token);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Unexpected Error] {exception.Message}");
            Console.ResetColor();
        }
        finally
        {
            Console.WriteLine("[System] Cleaning up resources...");
            await discoveryService.StopAsync();
        }

        Console.WriteLine("[System] Exited.");
        Console.ResetColor();
    }

    private static void DeserializeArguments(string[] args)
    {
        _userName = args[0];
    }
}