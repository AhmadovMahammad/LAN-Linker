using LanLinker.Console.UI;
using LanLinker.Core.Models;
using LanLinker.Network;

namespace LanLinker.Console;

internal static class LanLinkerApp
{
    public static async Task RunAsync()
    {
        Identity identity = IdentityPrompt.Collect();

        using NetworkManager networkManager = new NetworkManager(identity);

        CancellationTokenSource cts = new CancellationTokenSource();

        AppLayout layout = new AppLayout(networkManager.Peers);

        layout.QuitRequested += () => cts.Cancel();

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        networkManager.NetworkError += (_, e) => layout.ReportError(e.Context);

        networkManager.PeerConnected += (_, e) => layout.AddPeerConnected(e.Peer);

        networkManager.PeerDisconnected += (_, e) => layout.AddPeerDisconnected(e.Peer);

        await networkManager.StartAsync(cts.Token);

        await layout.RunAsync(cts.Token);

        await networkManager.StopAsync();
    }
}