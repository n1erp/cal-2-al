using Cal2AL.Converter.Engine;
using Cal2AL.Diagnostics;
using Spectre.Console;

namespace Cal2AL.CLI;

/// <summary>Renders progress, diagnostics and summaries with Spectre.Console.</summary>
public sealed class ConsoleReporter(IAnsiConsole console, Verbosity verbosity)
{
    public IAnsiConsole Console { get; } = console;
    public Verbosity Verbosity { get; } = verbosity;

    /// <summary>Runs <paramref name="work"/> with a progress bar when the console is interactive.</summary>
    public T WithProgress<T>(string title, Func<IProgress<ConversionProgress>?, T> work)
    {
        if (Verbosity == Verbosity.Quiet || !Console.Profile.Capabilities.Interactive)
        {
            return work(Verbosity == Verbosity.Verbose ? new LineProgress(Console) : null);
        }

        T result = default!;
        Console.Progress()
            .AutoClear(true)
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new RemainingTimeColumn())
            .Start(ctx =>
            {
                var tasks = new Dictionary<string, ProgressTask>(StringComparer.Ordinal);
                var gate = new Lock();
                var progress = new SyncProgress(p =>
                {
                    lock (gate)
                    {
                        if (!tasks.TryGetValue(p.Stage, out var task))
                        {
                            task = ctx.AddTask($"{title}: {p.Stage}", maxValue: Math.Max(1, p.Total));
                            tasks[p.Stage] = task;
                        }

                        task.MaxValue = Math.Max(1, p.Total);
                        task.Value = p.Current;
                    }
                });
                result = work(progress);
            });
        return result;
    }

    public void Error(string message) => Console.MarkupLine($"[red]error:[/] {Markup.Escape(message)}");

    public void Info(string message)
    {
        if (Verbosity != Verbosity.Quiet)
        {
            Console.MarkupLine(Markup.Escape(message));
        }
    }

    /// <summary>Prints diagnostics: errors always, warnings unless quiet, infos only when verbose.</summary>
    public void Diagnostics(ConversionReport report)
    {
        var minimum = Verbosity switch
        {
            Verbosity.Quiet => DiagnosticSeverity.Error,
            Verbosity.Normal => DiagnosticSeverity.Warning,
            _ => DiagnosticSeverity.Info,
        };

        foreach (var diagnostic in report.AllDiagnostics.Where(d => d.Severity >= minimum))
        {
            var color = diagnostic.Severity switch
            {
                DiagnosticSeverity.Error => "red",
                DiagnosticSeverity.Warning => "yellow",
                _ => "grey",
            };
            Console.MarkupLine($"[{color}]{diagnostic.Severity.ToString().ToLowerInvariant()} {diagnostic.Code}[/]: {Markup.Escape(diagnostic.Message)}");
            Console.MarkupLine($"    [grey]{Markup.Escape(diagnostic.Location.ToString())}[/]");
            if (Verbosity == Verbosity.Verbose && diagnostic.SuggestedResolution is { Length: > 0 } resolution)
            {
                Console.MarkupLine($"    [grey]→ {Markup.Escape(resolution)}[/]");
            }
        }
    }

    /// <summary>Per-type summary table, plus a per-object table in verbose mode.</summary>
    public void Summary(ConversionReport report)
    {
        if (Verbosity == Verbosity.Quiet)
        {
            return;
        }

        if (report.Objects.Count > 0)
        {
            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Objects[/]");
            table.AddColumns("Type", "Total", "Converted", "With warnings", "Manual work", "Failed", "Skipped");
            foreach (var s in report.Summary)
            {
                table.AddRow(s.ObjectType, s.Total.ToString(), s.Converted.ToString(), s.ConvertedWithWarnings.ToString(),
                    Count(s.RequiresManualWork, "yellow"), Count(s.Failed, "red"), s.Skipped.ToString());
            }

            Console.Write(table);
        }

        if (Verbosity == Verbosity.Verbose && report.Objects.Count > 0)
        {
            var table = new Table().Border(TableBorder.Simple).Title("[bold]Per object[/]");
            table.AddColumns("Object", "Status", "Err", "Warn", "Fields", "Controls", "Procs", "Triggers", "Code lines", "Statements in→out", "DB ops in→out");
            foreach (var o in report.Objects)
            {
                var m = o.Metrics;
                table.AddRow(
                    Markup.Escape($"{o.ObjectType} {o.ObjectId} {o.ObjectName}"),
                    Status(o.Status),
                    o.ErrorCount.ToString(), o.WarningCount.ToString(),
                    m.Fields.ToString(), m.Controls.ToString(), m.Procedures.ToString(), m.Triggers.ToString(), m.CodeLines.ToString(),
                    Arrow(m.Statements, m.OutputStatements), Arrow(m.DatabaseOperations, m.OutputDatabaseOperations));
            }

            Console.Write(table);
        }
    }

    public void ResultLine(ConversionReport report, TimeSpan elapsed, string? reportPath)
    {
        var color = report.ErrorCount > 0 ? "red" : report.WarningCount > 0 ? "yellow" : "green";
        Console.MarkupLine(
            $"[{color}]{report.Objects.Count} object(s)[/] from {report.FilesRead} file(s): " +
            $"[red]{report.ErrorCount} error(s)[/], [yellow]{report.WarningCount} warning(s)[/], {report.InfoCount} info, " +
            $"{report.GeneratedFiles.Count} file(s) written in {elapsed.TotalSeconds:0.0}s.");
        if (reportPath is not null && Verbosity != Verbosity.Quiet)
        {
            Console.MarkupLine($"Report: [blue]{Markup.Escape(reportPath)}[/]");
        }
    }

    private static string Count(int value, string color) => value == 0 ? "0" : $"[{color}]{value}[/]";

    private static string Arrow(int input, int output) =>
        input == output ? $"{input}→{output}" : $"[red]{input}→{output}[/]";

    private static string Status(ObjectConversionStatus status) => status switch
    {
        ObjectConversionStatus.Converted => "[green]Converted[/]",
        ObjectConversionStatus.ConvertedWithWarnings => "[yellow]Warnings[/]",
        ObjectConversionStatus.RequiresManualWork => "[darkorange]Manual work[/]",
        ObjectConversionStatus.Failed => "[red]Failed[/]",
        _ => "[grey]Skipped[/]",
    };

    /// <summary>Invokes the callback on the reporting thread (unlike Progress&lt;T&gt;, which posts asynchronously).</summary>
    private sealed class SyncProgress(Action<ConversionProgress> report) : IProgress<ConversionProgress>
    {
        public void Report(ConversionProgress value) => report(value);
    }

    /// <summary>Verbose, non-interactive output: one line per object.</summary>
    private sealed class LineProgress(IAnsiConsole console) : IProgress<ConversionProgress>
    {
        private readonly Lock gate = new();

        public void Report(ConversionProgress value)
        {
            if (value.Stage == "Reading")
            {
                return;
            }

            lock (gate)
            {
                console.MarkupLine($"[grey][[{value.Current}/{value.Total}]][/] {Markup.Escape(value.Item)}");
            }
        }
    }
}
