using LanLinker.Core.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LanLinker.Console.UI.Commands;

internal sealed class UsersCommand(IReadOnlyDictionary<string, Peer> peers) : ICommand
{
    public string Name => "/users";
    public string Description => "list online peers";

    public IRenderable Execute(string[] args)
    {
        List<Peer> peers1 = peers.Values.OrderBy(p => p.ConnectedAt).ToList();

        if (peers1.Count == 0)
        {
            return new Markup($"[{Theme.Dim}]  no peers online[/]");
        }

        Table table = new Table()
            .Border(TableBorder.None)
            .HideHeaders()
            .AddColumn(new TableColumn("").NoWrap().PadLeft(2).PadRight(2))
            .AddColumn(new TableColumn("").NoWrap().PadRight(4))
            .AddColumn(new TableColumn(""));

        foreach (Peer peer in peers1)
        {
            table.AddRow(
                new Markup($"[{Theme.Dim}]●[/]"),
                new Markup($"[{Theme.Primary}]{Markup.Escape(peer.UserName)}[/]"),
                new Markup($"[{Theme.Dim}]{Markup.Escape(peer.DeviceName)}[/]"));
        }

        return table;
    }
}