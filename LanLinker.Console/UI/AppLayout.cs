using System.Threading.Channels;
using LanLinker.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LanLinker.Console.UI;

internal sealed class AppLayout
{
    private readonly CommandsManager _commandsManager;
    private readonly InputHandler _inputHandler = new();
    private readonly List<IRenderable> _messages =
    [
        new Markup($"[{Theme.Dim}]  type /help for commands[/]")
    ];

    private readonly object _messagesLock = new();
    private readonly Channel<bool> _refreshChannel = Channel.CreateBounded<bool>
    (
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite
        }
    );

    public AppLayout(IReadOnlyDictionary<string, Peer> peers)
    {
        _commandsManager = new CommandsManager(peers, AppendMessage, () => QuitRequested?.Invoke());
        _inputHandler.BufferChanged += SignalRefresh;
        _inputHandler.Submitted += HandleInput;
    }

    public event Action<string>? MessageSubmitted;
    public event Action? QuitRequested;

    public void AddPeerConnected(Peer peer)
    {
        AppendMessage(new Markup($"[{Theme.Dim}]● {Markup.Escape(peer.UserName)} joined[/]"));
    }

    public void AddPeerDisconnected(Peer peer)
    {
        AppendMessage(new Markup($"[{Theme.Dim}]○ {Markup.Escape(peer.UserName)} left[/]"));
    }

    public void ReportError(string message)
    {
        AppendMessage(new Markup($"[red]! {Markup.Escape(message)}[/]"));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using (_inputHandler)
        {
            Task inputTask = Task.Run(() => _inputHandler.Run(cancellationToken), CancellationToken.None);

            await AnsiConsole.Live(Render()).StartAsync(async ctx =>
            {
                SignalRefresh();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await _refreshChannel.Reader.ReadAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    ctx.UpdateTarget(Render());
                    ctx.Refresh();
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

    private void HandleInput(string input)
    {
        if (input.StartsWith('/'))
        {
            _commandsManager.Execute(input);
            return;
        }

        MessageSubmitted?.Invoke(input);

        AppendMessage(new Markup($"[{Theme.Primary}]  you: {Markup.Escape(input)}[/]"));
    }

    private void AppendMessage(IRenderable renderable)
    {
        lock (_messagesLock)
        {
            _messages.Add(renderable);
        }

        SignalRefresh();
    }

    private void SignalRefresh()
    {
        _refreshChannel.Writer.TryWrite(true);
    }

    private IRenderable Render()
    {
        IRenderable[] snapshot;

        lock (_messagesLock)
        {
            snapshot = _messages.TakeLast(30).ToArray();
        }

        Panel messagePanel = new Panel(new Rows(snapshot))
            .Header($"[{Theme.Dim}] lan-linker [/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Grey35))
            .Expand();

        Panel inputPanel = new Panel(
                new Markup($"[{Theme.Dim}]>[/] [{Theme.Primary}]{Markup.Escape(_inputHandler.CurrentBuffer)}[/]"))
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Color.Grey35))
            .Expand();

        return new Rows(messagePanel, inputPanel);
    }
}