using Spectre.Console;

namespace LanLinker.Console;

internal abstract class Program
{
    private static readonly string DeviceId = Guid.NewGuid().ToString();
    private static readonly string DeviceName = Environment.MachineName;

    public static void Main(string[] args)
    {
        string defaultName = Environment.UserName;

        string userName =
            AnsiConsole.Prompt(new TextPrompt<string>("[grey]>[/] Please identify yourself ([green]Call sign[/]):")
                .DefaultValue(defaultName)
                .ValidationErrorMessage("[red]That is not a valid call sign![/]")
                .Validate(name =>
                {
                    return name.Length switch
                    {
                        < 3 => ValidationResult.Error("[red]Call sign must be at least 3 characters[/]"),
                        > 15 => ValidationResult.Error("[red]Call sign must be less than 15 characters[/]"),
                        _ => ValidationResult.Success()
                    };
                }));

        Grid grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(4));
        grid.AddColumn(new GridColumn().NoWrap());

        grid.AddRow("[gray]User Identity:[/]", $"[dim]{userName}[/]");
        grid.AddRow("[gray]Machine Name:[/]", $"[dim]{DeviceName}[/]");
        grid.AddRow("[gray]Device ID:[/]", $"[dim]{DeviceId}[/]");
        grid.AddRow("[gray]Status:[/]", "[dim]ONLINE (Broadcasting)[/]");

        Panel panel = new Panel(grid)
            .Header("[green bold] ACCESS GRANTED [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green)
            .Expand();

        AnsiConsole.Write(panel);
    }
}