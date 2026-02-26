using LanLinker.Core.Models;

namespace LanLinker.Network.Test;

internal abstract class Program
{
    private static readonly string DeviceId = Guid.NewGuid().ToString();
    private static readonly string DeviceName = Environment.MachineName;
    private static string _userName = "ahmadov.dev";

    public static async Task Main(string[] args)
    {
        if (args.Length > 0)
        {
            DeserializeArguments(args);
        }

        LocalPeerConfig peerConfig = new LocalPeerConfig(DeviceId, DeviceName, _userName);

        using NetworkManager networkManager = new NetworkManager(peerConfig);

        CancellationTokenSource cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("\n\n[System] Stopping requested...");

            e.Cancel = true;
            cts.Cancel();
        };

        networkManager.PeerConnected += (_, eventArgs) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] New Peer Discovered!");
            Console.WriteLine($"User:    {eventArgs.Peer.UserName}");
            Console.WriteLine($"Device:  {eventArgs.Peer.DeviceName}");
            Console.ResetColor();
        };

        networkManager.PeerDisconnected += (_, eventArgs) =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Peer Disconnected!");
            Console.WriteLine($"User:    {eventArgs.Peer.UserName}");
            Console.WriteLine($"Device:  {eventArgs.Peer.DeviceName}");
            Console.ResetColor();
        };

        networkManager.NetworkError += (_, eventArgs) =>
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\n[CRITICAL ERROR] {eventArgs.Context}");
            Console.ResetColor();

            cts.Cancel();
        };

        try
        {
            await networkManager.StartAsync(cts.Token);
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
            await networkManager.StopAsync();
        }
    }

    private static void DeserializeArguments(string[] args)
    {
        _userName = args[0];
    }
}