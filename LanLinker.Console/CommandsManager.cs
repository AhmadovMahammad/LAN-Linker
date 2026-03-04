using LanLinker.Console.UI;
using LanLinker.Console.UI.Commands;
using LanLinker.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LanLinker.Console;

internal sealed class CommandsManager
{
    private readonly Action<IRenderable> _appendMessage;
    
    private readonly List<ICommand> _commands;

    public CommandsManager(
        IReadOnlyDictionary<string, Peer> peers,
        Action<IRenderable> appendMessage,
        Action onQuit)
    {
        _appendMessage = appendMessage;

        _commands =
        [
            new HelpCommand(new List<ICommand>()),
            new UsersCommand(peers),
            new QuitCommand(onQuit)
        ];

        _commands[0] = new HelpCommand(_commands);
    }

    public void Execute(string input)
    {
        string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string name = parts[0].ToLowerInvariant();

        if (_commands.FirstOrDefault(c => c.Name == name) is not { } command)
        {
            _appendMessage(new Markup($"[{Theme.Dim}]  unknown: {Markup.Escape(name)} — type /help[/]"));
            return;
        }

        IRenderable? renderable = command.Execute(parts);

        if (renderable != null)
        {
            _appendMessage.Invoke(renderable);
        }
    }
}