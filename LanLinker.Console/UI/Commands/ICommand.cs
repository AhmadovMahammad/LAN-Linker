using Spectre.Console.Rendering;

namespace LanLinker.Console.UI.Commands;

internal interface ICommand
{
    string Name { get; }
    string Description { get; }
    IRenderable? Execute(string[] args);
}