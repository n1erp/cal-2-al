using System.CommandLine;
using System.Text;
using Cal2AL.CLI;
using Cal2AL.Converter;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;

var services = new ServiceCollection()
    .AddCal2AL()
    .BuildServiceProvider();

var options = new CliOptions();
var commands = new CommandHandlers(services, options);

var root = new RootCommand(
    "Cal2AL converts Dynamics NAV C/AL objects exported as text (.txt, .delta) into AL for Business Central. " +
    "Code is carried over token for token and checked; anything that cannot be converted is reported with a reason and saved as a migration artifact.");
root.Options.Add(options.Verbose);
root.Options.Add(options.Quiet);
root.Options.Add(options.NoColor);

root.Subcommands.Add(commands.ConvertCommand());
root.Subcommands.Add(commands.AnalyzeCommand());
root.Subcommands.Add(commands.ValidateCommand());
root.Subcommands.Add(commands.InspectCommand());
root.Subcommands.Add(commands.CompareCommand());

var parseResult = root.Parse(args);
if (parseResult.Errors.Count > 0)
{
    foreach (var error in parseResult.Errors)
    {
        Console.Error.WriteLine($"error: {error.Message}");
    }

    Console.Error.WriteLine("Run 'cal2al --help' for usage.");
    return ExitCodes.InvalidArguments;
}

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

try
{
    return await parseResult.InvokeAsync(cancellationToken: cancellation.Token);
}
catch (OperationCanceledException)
{
    AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
    return ExitCodes.Cancelled;
}
