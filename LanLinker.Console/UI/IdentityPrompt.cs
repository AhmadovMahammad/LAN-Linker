using LanLinker.Core.Models;
using Spectre.Console;

namespace LanLinker.Console.UI;

internal static class IdentityPrompt
{
    public static Identity Collect()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Theme.Muted}]LAN Linker[/]");
        AnsiConsole.WriteLine();

        string userName = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{Theme.Dim}]>[/] call sign")
                .DefaultValue(Environment.UserName)
                .ValidationErrorMessage($"[{Theme.Dim}]invalid[/]")
                .Validate(name => name.Length switch
                {
                    < 3 => ValidationResult.Error("at least 3 characters"),
                    > 15 => ValidationResult.Error("no more than 15 characters"),
                    _ => ValidationResult.Success()
                }));

        Identity identity = new Identity(userName, Environment.MachineName, Guid.NewGuid().ToString());

        AnsiConsole.WriteLine();

        Grid grid = new Grid()
            .AddColumn(new GridColumn().NoWrap().PadRight(2))
            .AddColumn(new GridColumn().NoWrap());

        grid.AddRow($"[{Theme.Dim}]user[/]", $"[{Theme.Primary}]{Markup.Escape(identity.UserName)}[/]");
        grid.AddRow($"[{Theme.Dim}]machine[/]", $"[{Theme.Muted}]{Markup.Escape(identity.DeviceName)}[/]");
        grid.AddRow($"[{Theme.Dim}]id[/]", $"[{Theme.Dim}]{Markup.Escape(identity.DeviceId)}[/]");

        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        return identity;
    }
}