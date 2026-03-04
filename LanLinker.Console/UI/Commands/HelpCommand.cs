using Spectre.Console;
using Spectre.Console.Rendering;

namespace LanLinker.Console.UI.Commands;

internal sealed class HelpCommand(IReadOnlyList<ICommand> commands) : ICommand
{
    public string Name => "/help";
    public string Description => "show this message";

    public IRenderable Execute(string[] args)
    {
        Table table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("").NoWrap().PadLeft(2).PadRight(4))
            .AddColumn(new TableColumn(""));

        foreach (ICommand command in commands)
        {
            table.AddRow(
                new Markup($"[{Theme.Primary}]{Markup.Escape(command.Name)}[/]"),
                new Markup($"[{Theme.Dim}]{Markup.Escape(command.Description)}[/]"));
        }

        return table;
    }
}