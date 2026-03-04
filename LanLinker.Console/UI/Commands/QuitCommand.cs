using Spectre.Console.Rendering;

namespace LanLinker.Console.UI.Commands;

internal sealed class QuitCommand(Action onQuit) : ICommand
{
    public string Name => "/quit";
    public string Description => "exit";

    public IRenderable? Execute(string[] args)
    {
        onQuit();
        return null;
    }
}