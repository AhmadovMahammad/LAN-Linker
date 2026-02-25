using LanLinker.Core.Interfaces;
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
        
        IUdpDiscoveryService discoveryService = new UdpDiscoveryService(DeviceId, DeviceName, _userName);
        
        CancellationTokenSource cts = new CancellationTokenSource();
        
        discoveryService.OnPeerDiscovered += peer =>
        {
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
        
        discoveryService.OnCriticalError += (ex) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\n[CRITICAL ERROR] {ex.Message}");
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
            await discoveryService.StopAsync(CancellationToken.None);
        }
        
        Console.WriteLine("[System] Exited.");
        Console.ResetColor();
    }

    private static void DeserializeArguments(string[] args)
    {
        _userName = args[0];
    }
}