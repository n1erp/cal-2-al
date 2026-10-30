using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Cal2AL.Converter;
using Cal2AL.Converter.Comparison;
using Cal2AL.Converter.Engine;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Cal2AL.CLI;

/// <summary>Defines the commands and runs them.</summary>
public sealed partial class CommandHandlers(IServiceProvider services, CliOptions options)
{
    private IConversionEngine Engine => services.GetRequiredService<IConversionEngine>();

    // ---- convert -----------------------------------------------------------------------------------

    public Command ConvertCommand()
    {
        var command = new Command("convert", "Convert C/AL text exports into an AL project folder.")
        {
            options.Source, options.Target, options.Report, options.FailOn, options.ObjectTypes, options.Ids,
            options.Structure, options.ExtensionStartId, options.Runtime, options.Encoding, options.DateOrder,
            options.ScopeAttributes, options.NoMetadata, options.NoTranslations, options.NoVerify,
            options.FilePattern, options.ExtensionFilePattern,
        };
        command.SetAction((result, token) => Task.FromResult(RunEngine(result, RunMode.Convert, token)));
        return command;
    }

    // ---- analyze -----------------------------------------------------------------------------------

    public Command AnalyzeCommand()
    {
        var command = new Command("analyze", "Convert in memory and report diagnostics and statistics without writing AL files.")
        {
            options.Source, options.Report, options.FailOn, options.ObjectTypes, options.Ids, options.Runtime,
            options.Encoding, options.DateOrder, options.ScopeAttributes, options.NoVerify,
        };
        command.SetAction((result, token) => Task.FromResult(RunEngine(result, RunMode.Analyze, token)));
        return command;
    }

    // ---- validate ----------------------------------------------------------------------------------

    public Command ValidateCommand()
    {
        var command = new Command("validate", "Check that the input files can be read and parsed (object structure and C/AL code syntax).")
        {
            options.Source, options.Report, options.FailOn, options.Encoding,
        };
        command.SetAction((result, token) => Task.FromResult(RunEngine(result, RunMode.Validate, token)));
        return command;
    }

    private int RunEngine(ParseResult result, RunMode mode, CancellationToken cancellationToken)
    {
        var reporter = CreateReporter(result);
        var (conversionOptions, error) = options.BuildConversionOptions(result);
        if (conversionOptions is null)
        {
            reporter.Error(error!);
            return ExitCodes.InvalidArguments;
        }

        var source = result.GetValue(options.Source)!;
        if (!File.Exists(source) && !Directory.Exists(source))
        {
            reporter.Error($"The source '{source}' does not exist.");
            return ExitCodes.NoInput;
        }

        var target = mode == RunMode.Convert ? Path.GetFullPath(result.GetValue(options.Target)!) : null;
        if (target is not null && IsInside(target, Path.GetFullPath(source)) && Directory.Exists(source))
        {
            reporter.Error("The target folder must not be the source folder or inside it.");
            return ExitCodes.InvalidArguments;
        }

        var request = new ConversionRequest
        {
            Source = Path.GetFullPath(source),
            Target = target,
            Mode = mode,
            Options = conversionOptions,
            ReportedOptions = CliOptions.Echo(result),
        };

        var stopwatch = Stopwatch.StartNew();
        var sink = target is null ? null : new FileSystemOutputSink(target);
        var run = reporter.WithProgress(mode.ToString(), progress => Engine.Run(request, sink, progress, cancellationToken));
        stopwatch.Stop();

        var reportPath = result.GetValue(options.Report)
            ?? (target is null ? null : Path.Combine(target, conversionOptions.ArtifactFolder, "report.json"));
        if (reportPath is not null)
        {
            try
            {
                run.Report.WriteJson(reportPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                reporter.Error($"Cannot write the report '{reportPath}': {ex.Message}");
            }
        }

        reporter.Diagnostics(run.Report);
        reporter.Summary(run.Report);
        reporter.ResultLine(run.Report, stopwatch.Elapsed, reportPath is null ? null : Path.GetFullPath(reportPath));

        if (run.ParsedObjects.Count == 0 && run.Report.Objects.Count == 0)
        {
            return ExitCodes.NoInput;
        }

        return CliOptions.ReachesFailLevel(run.Report, result.GetValue(options.FailOn))
            ? ExitCodes.DiagnosticsAtFailLevel
            : ExitCodes.Success;
    }

    private static bool IsInside(string path, string folder)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var f = folder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return string.Equals(path.TrimEnd(Path.DirectorySeparatorChar), folder.TrimEnd(Path.DirectorySeparatorChar), comparison)
            || path.StartsWith(f, comparison);
    }

    // ---- inspect -----------------------------------------------------------------------------------

    public Command InspectCommand()
    {
        var source = new Option<string?>("--source", "-s") { Description = "A C/AL .txt/.delta file or folder to inspect." };
        var objectFilter = new Option<string?>("--object", "-o") { Description = "Show one object in detail, e.g. Table:50000 or \"Page 21\"." };
        var showAl = new Option<bool>("--al") { Description = "With --object: print the converted AL and its diagnostics." };
        var listDiagnostics = new Option<bool>("--diagnostics") { Description = "List every diagnostic code Cal2AL can report." };
        var command = new Command("inspect", "Show the objects in C/AL files, the parsed structure of one object, its AL conversion, or the diagnostic catalogue.")
        {
            source, objectFilter, showAl, listDiagnostics, options.Encoding,
        };
        command.SetAction((result, token) => Task.FromResult(Inspect(result, source, objectFilter, showAl, listDiagnostics, token)));
        return command;
    }

    private int Inspect(ParseResult result, Option<string?> sourceOption, Option<string?> objectOption, Option<bool> alOption, Option<bool> diagnosticsOption, CancellationToken cancellationToken)
    {
        var reporter = CreateReporter(result);
        var console = reporter.Console;
        if (result.GetValue(diagnosticsOption))
        {
            var table = new Table().Border(TableBorder.Rounded).AddColumns("Code", "Severity", "Category", "Title", "Suggested resolution");
            foreach (var d in DiagnosticDescriptors.All.OrderBy(d => d.Code, StringComparer.Ordinal))
            {
                table.AddRow(d.Code, d.DefaultSeverity.ToString(), d.Category.ToString(), Markup.Escape(d.Title), Markup.Escape(d.DefaultResolution ?? string.Empty));
            }

            console.Write(table);
            return ExitCodes.Success;
        }

        var source = result.GetValue(sourceOption);
        if (source is null)
        {
            reporter.Error("Specify --source, or --diagnostics to list the diagnostic codes.");
            return ExitCodes.InvalidArguments;
        }

        if (!File.Exists(source) && !Directory.Exists(source))
        {
            reporter.Error($"The source '{source}' does not exist.");
            return ExitCodes.NoInput;
        }

        var (conversionOptions, error) = options.BuildConversionOptions(result);
        if (conversionOptions is null)
        {
            reporter.Error(error!);
            return ExitCodes.InvalidArguments;
        }

        var filterText = result.GetValue(objectOption);
        var request = new ConversionRequest { Source = Path.GetFullPath(source), Mode = RunMode.Analyze, Options = conversionOptions };
        var run = Engine.Run(request, null, null, cancellationToken);

        if (filterText is null)
        {
            var table = new Table().Border(TableBorder.Rounded)
                .AddColumns("Type", "ID", "Name", "Dialect", "Status", "Fields", "Controls", "Procedures", "Triggers", "Code lines", "Source");
            foreach (var o in run.Objects)
            {
                var m = o.Result.Metrics;
                table.AddRow(o.Source.Type.ToString(), o.Source.Id.ToString(CultureInfo.InvariantCulture), Markup.Escape(o.Source.Name),
                    o.Source.Dialect.ToString(), o.Result.Status.ToString(), m.Fields.ToString(), m.Controls.ToString(),
                    m.Procedures.ToString(), m.Triggers.ToString(), m.CodeLines.ToString(),
                    Markup.Escape($"{Path.GetFileName(o.Source.SourceFile)}:{o.Source.Position.Line}"));
            }

            foreach (var failed in run.Report.Objects.Where(r => r.Status == ObjectConversionStatus.Failed && run.Objects.All(o => o.Result != r)))
            {
                table.AddRow(failed.ObjectType, failed.ObjectId.ToString(CultureInfo.InvariantCulture), Markup.Escape(failed.ObjectName),
                    "-", "[red]Not parsed[/]", "", "", "", "", "", Markup.Escape($"{Path.GetFileName(failed.SourceFile)}:{failed.SourceLine}"));
            }

            console.Write(table);
            reporter.Diagnostics(new ConversionReport { Command = "inspect", Source = source, RunDiagnostics = run.Report.RunDiagnostics });
            return ExitCodes.Success;
        }

        var filter = ObjectFilterRegex().Match(filterText.Trim());
        if (!filter.Success)
        {
            reporter.Error($"Invalid --object '{filterText}'. Use Type:Id, e.g. Table:50000.");
            return ExitCodes.InvalidArguments;
        }

        var type = CalObjectTypeExtensions.ParseObjectType(filter.Groups["type"].Value);
        var id = int.Parse(filter.Groups["id"].Value, CultureInfo.InvariantCulture);
        var match = run.Objects.FirstOrDefault(o => o.Source.Type == type && o.Source.Id == id);
        if (match is null)
        {
            reporter.Error($"{type} {id} was not found in '{source}'.");
            return ExitCodes.NoInput;
        }

        if (result.GetValue(alOption))
        {
            if (match.AlText is { } text)
            {
                console.WriteLine(text);
            }
            else
            {
                reporter.Error("No AL was produced for this object.");
            }

            reporter.Diagnostics(new ConversionReport { Command = "inspect", Source = source, Objects = [match.Result] });
            return ExitCodes.Success;
        }

        console.Write(StructureTree(match.Source));
        return ExitCodes.Success;
    }

    private static Tree StructureTree(CalObject obj)
    {
        var tree = new Tree($"[bold]{Markup.Escape(obj.DisplayName)}[/] [grey]({obj.Dialect}, {Markup.Escape(Path.GetFileName(obj.SourceFile))}:{obj.Position.Line}-{obj.EndLine})[/]");
        if (obj.UnsupportedReason is { } reason)
        {
            tree.AddNode($"[red]Unsupported:[/] {Markup.Escape(reason)}");
        }

        AddNode(tree.AddNode("[blue]OBJECT-PROPERTIES[/]"), obj.ObjectProperties.Select(p => $"{p.Key}={p.Value}"));
        AddCalNode(tree, obj);
        var code = tree.AddNode("[blue]CODE[/]");
        AddNode(code.AddNode("Variables"), obj.Code.Variables.Select(v => $"{v.Name}: {v.Type.Name}"));
        foreach (var p in obj.Code.Procedures)
        {
            code.AddNode(Markup.Escape($"{(p.IsEvent ? "EVENT" : "PROCEDURE")} {p.Name} ({p.Parameters.Count} parameter(s), {p.Body.Lines.Count} line(s)) @{p.Position.Line}"));
        }

        return tree;
    }

    private static void AddCalNode(IHasTreeNodes parent, CalNode node)
    {
        if (node.Properties.Any())
        {
            AddNode(parent.AddNode("Properties"), node.Properties.Select(p => $"{p.Name}={Shorten(p.Value)}"));
        }

        foreach (var trigger in node.Triggers)
        {
            parent.AddNode(Markup.Escape($"trigger {trigger.Name} ({trigger.Body.Lines.Count} line(s)) @{trigger.Position.Line}"));
        }

        foreach (var section in node.Sections)
        {
            var sectionNode = parent.AddNode($"[blue]{Markup.Escape(section.Name)}[/] ({section.Items.Count} item(s))");
            foreach (var item in section.Items)
            {
                var itemNode = sectionNode.AddNode(Markup.Escape(string.Join(" ; ", item.Positional.Select(p => p.Trim()))) + $" [grey]@{item.Position.Line}[/]");
                AddCalNode(itemNode, item);
            }

            AddNode(sectionNode, section.Variables.Select(v => $"VAR {v.Name}: {v.Type.Name}"));
            AddNode(sectionNode, section.Procedures.Select(p => $"PROCEDURE {p.Name}"));
        }
    }

    private static void AddNode(IHasTreeNodes parent, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            parent.AddNode(Markup.Escape(line));
        }
    }

    private static string Shorten(string value)
    {
        var flat = value.ReplaceLineEndings(" ");
        return flat.Length <= 100 ? flat : flat[..97] + "...";
    }

    [GeneratedRegex(@"^(?<type>[A-Za-z]+)\s*[: ]\s*(?<id>\d+)$")]
    private static partial Regex ObjectFilterRegex();

    // ---- compare -----------------------------------------------------------------------------------

    public Command CompareCommand()
    {
        var cal2al = new Option<string>("--cal2al") { Description = "Folder with AL files produced by Cal2AL.", Required = true };
        var reference = new Option<string>("--reference") { Description = "Folder with AL files produced by the reference converter (Txt2Al) from the same input.", Required = true };
        var showEquivalent = new Option<bool>("--show-equivalent") { Description = "Also list entries that are identical." };
        var command = new Command("compare", "Compare Cal2AL output with reference (Txt2Al) output, entry by entry, and classify the differences.")
        {
            cal2al, reference, options.Report, showEquivalent,
        };
        command.SetAction((result, _) => Task.FromResult(Compare(result, cal2al, reference, showEquivalent)));
        return command;
    }

    private int Compare(ParseResult result, Option<string> cal2alOption, Option<string> referenceOption, Option<bool> showEquivalentOption)
    {
        var reporter = CreateReporter(result);
        ComparisonReport comparison;
        try
        {
            comparison = AlComparer.CompareFolders(result.GetValue(cal2alOption)!, result.GetValue(referenceOption)!);
        }
        catch (DirectoryNotFoundException ex)
        {
            reporter.Error(ex.Message);
            return ExitCodes.NoInput;
        }

        if (result.GetValue(options.Report) is { } reportPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath))!);
            File.WriteAllText(reportPath, comparison.ToJson());
        }

        var console = reporter.Console;
        var showEquivalent = result.GetValue(showEquivalentOption);
        if (reporter.Verbosity != Verbosity.Quiet)
        {
            foreach (var obj in comparison.Objects)
            {
                var differences = obj.Entries.Where(e => showEquivalent || e.Category != DifferenceCategory.Equivalent).ToList();
                if (differences.Count == 0 && obj.Cal2ALFile is not null && obj.ReferenceFile is not null)
                {
                    continue;
                }

                console.MarkupLine($"[bold]{Markup.Escape(obj.Key)}[/] [grey]{Markup.Escape(obj.Cal2ALFile ?? "(no Cal2AL file)")} ↔ {Markup.Escape(obj.ReferenceFile ?? "(no reference file)")}[/]");
                foreach (var d in differences)
                {
                    var color = d.Category switch
                    {
                        DifferenceCategory.Behavioral => "red",
                        DifferenceCategory.MissingInCal2AL => "red",
                        DifferenceCategory.MissingInReference => "blue",
                        DifferenceCategory.Formatting => "grey",
                        _ => "green",
                    };
                    console.MarkupLine($"  [{color}]{d.Category,-18}[/] {Markup.Escape(d.Path)}");
                    if (reporter.Verbosity == Verbosity.Verbose && d.Category != DifferenceCategory.Equivalent)
                    {
                        console.MarkupLine($"      [grey]cal2al:[/]    {Markup.Escape(d.Cal2AL ?? "-")}");
                        console.MarkupLine($"      [grey]reference:[/] {Markup.Escape(d.Reference ?? "-")}");
                    }
                }
            }
        }

        var totals = new Table().Border(TableBorder.Rounded).AddColumns("Category", "Entries");
        foreach (var (category, count) in comparison.Totals)
        {
            totals.AddRow(category.ToString(), count.ToString(CultureInfo.InvariantCulture));
        }

        console.Write(totals);
        return comparison.Totals[DifferenceCategory.MissingInCal2AL] + comparison.Totals[DifferenceCategory.Behavioral] > 0
            ? ExitCodes.DiagnosticsAtFailLevel
            : ExitCodes.Success;
    }

    private ConsoleReporter CreateReporter(ParseResult result)
    {
        var noColor = result.GetValue(options.NoColor) || Environment.GetEnvironmentVariable("NO_COLOR") is { Length: > 0 };
        var console = noColor
            ? AnsiConsole.Create(new AnsiConsoleSettings { Ansi = AnsiSupport.No, ColorSystem = ColorSystemSupport.NoColors, Interactive = InteractionSupport.No })
            : AnsiConsole.Console;
        if (System.Console.IsOutputRedirected)
        {
            // Without a terminal Spectre cannot detect a width and would truncate every line.
            console.Profile.Width = int.TryParse(Environment.GetEnvironmentVariable("COLUMNS"), out var columns) && columns > 40 ? columns : 160;
        }

        return new ConsoleReporter(console, options.GetVerbosity(result));
    }
}
