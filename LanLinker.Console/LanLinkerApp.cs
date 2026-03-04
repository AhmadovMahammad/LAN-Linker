using LanLinker.Console.UI;
using LanLinker.Core.Models;
using LanLinker.Network;

namespace LanLinker.Console;

internal static class LanLinkerApp
{
    public static async Task RunAsync()
    {
        Identity identity = IdentityPrompt.Collect();

        NetworkManager networkManager = new NetworkManager(identity);

        CancellationTokenSource cts = new CancellationTokenSource();

        AppLayout layout = new AppLayout(networkManager.Peers);

        layout.QuitRequested += () => cts.Cancel();

        layout.MessageSubmitted += message => networkManager.GlobalChatService.SendGlobalMessageAsync(message);

        System.Console.CancelKeyPress += (_, e) =>
        {
            cts.Cancel();
        };

        networkManager.NetworkError += (_, e) =>
        {
            layout.ReportError(e.Context);
            cts.Cancel();
        };

        networkManager.PeerConnected += (_, e) => layout.AddPeerConnected(e.Peer);

        networkManager.PeerDisconnected += (_, e) => layout.AddPeerDisconnected(e.Peer);

        networkManager.GlobalMessageReceived += (_, e) =>
        {
            string senderName = networkManager.Peers.TryGetValue(e.SenderDeviceId, out Peer? peer)
                ? peer.UserName
                : e.SenderDeviceId;

            layout.AddGlobalMessage(senderName, e.Message.UserMessage);
        };

        await networkManager.StartAsync(cts.Token);

        await layout.RunAsync(cts.Token);

        await networkManager.StopAsync();

        networkManager.Dispose();
    }
}