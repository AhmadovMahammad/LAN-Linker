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

        AppLayout layout = new AppLayout();

        CancellationTokenSource cts = new CancellationTokenSource();

        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        networkManager.PeerConnected += (_, e) => layout.AddPeer(e.Peer);
        networkManager.PeerDisconnected += (_, e) => layout.RemovePeer(e.Peer);
        networkManager.NetworkError += (_, e) => layout.ReportError(e.Context);

        // layout.MessageSubmitted += message => networkManager.SendMessageAsync(message, cts.Token);

        await networkManager.StartAsync(cts.Token);

        await layout.RunAsync(cts.Token);

        await networkManager.StopAsync();
    }
}