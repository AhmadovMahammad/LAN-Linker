using System.Collections.Concurrent;
using LanLinker.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LanLinker.Console.UI;

internal sealed class AppLayout()
{
    private readonly ConcurrentDictionary<string, Peer> _peers = new();
    private readonly InputHandler _inputHandler = new();
    private string? _errorMessage;

    public event Action<string>? MessageSubmitted;

    public void AddPeer(Peer peer)
    {
        _peers[peer.DeviceId] = peer;
    }

    public void RemovePeer(Peer peer)
    {
        _peers.TryRemove(peer.DeviceId, out _);
    }

    public void ReportError(string message)
    {
        _errorMessage = message;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using (_inputHandler)
        {
            Task inputTask = Task.Run(() => _inputHandler.Run(cancellationToken), CancellationToken.None);

            await AnsiConsole.Live(Render()).StartAsync(async ctx =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ctx.UpdateTarget(Render());

                    ctx.Refresh();

                    try
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            });

            try
            {
                await inputTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        AnsiConsole.MarkupLine($"[{Theme.Dim}]stopped.[/]");
        AnsiConsole.WriteLine();
    }

    private IRenderable Render()
    {
        string separator = new string('-', System.Console.WindowWidth);

        List<Peer> peers = _peers.Values.OrderBy(p => p.ConnectedAt).ToList();

        List<IRenderable> rows =
        [
            new Markup($"[{Theme.Dim}]peers {peers.Count} online[/]"),
            new Text("")
        ];

        if (peers.Count == 0)
        {
            rows.Add(new Markup($"  [{Theme.Dim}]waiting for peers[/]"));
        }
        else
        {
            foreach (Peer peer in peers)
            {
                string name = Markup.Escape(peer.UserName);

                string peerDevice = Markup.Escape(peer.DeviceName);

                rows.Add(new Markup($"  [{Theme.Primary}]{name}[/]  [{Theme.Dim}]{peerDevice}[/]"));
            }
        }

        if (!string.IsNullOrWhiteSpace(_errorMessage))
        {
            rows.Add(new Text(""));
            rows.Add(new Markup($"[{Theme.Dim}]! {Markup.Escape(_errorMessage)}[/]"));
        }

        rows.Add(new Text(""));

        rows.Add(new Markup($"[{Theme.Dim}]{separator}[/]"));

        rows.Add(new Markup($"[{Theme.Dim}]>[/] [{Theme.Primary}]{Markup.Escape(_inputHandler.CurrentBuffer)}[/]"));

        return new Rows(rows);
    }
}