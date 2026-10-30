using System.Globalization;
using System.Text;
using Cal2AL.Converter.Objects;
using Cal2AL.Converter.Symbols;
using Cal2AL.Core.Model;
using Cal2AL.Diagnostics;
using Cal2AL.Generator;
using Cal2AL.Generator.Model;
using Cal2AL.Parser.Code;
using Cal2AL.Parser.Txt;
using Diagnostic = Cal2AL.Diagnostics.Diagnostic;

namespace Cal2AL.Converter.Engine;

/// <summary>What a run does with the parsed objects.</summary>
public enum RunMode
{
    /// <summary>Parse only: report input that cannot be read or parsed.</summary>
    Validate,

    /// <summary>Parse and convert in memory: diagnostics and statistics, nothing written.</summary>
    Analyze,

    /// <summary>Parse, convert and write the AL project.</summary>
    Convert,
}

public sealed record ConversionRequest
{
    public required string Source { get; init; }
    public string? Target { get; init; }
    public RunMode Mode { get; init; } = RunMode.Convert;
    public ConversionOptions Options { get; init; } = new();

    /// <summary>Values echoed into the report (command-line options as typed).</summary>
    public IReadOnlyDictionary<string, string> ReportedOptions { get; init; } = new Dictionary<string, string>();
}

/// <summary>Progress notification: <paramref name="Stage"/> is "Reading", "Converting" or "Writing".</summary>
public sealed record ConversionProgress(string Stage, int Current, int Total, string Item);

/// <summary>A parsed object and what converting it produced (Analyze/Convert).</summary>
public sealed record ConvertedObject(CalObject Source, ObjectConversionResult Result, AlObject? AlObject, string? AlText);

public sealed class ConversionRun
{
    public required ConversionReport Report { get; init; }
    public List<CalObject> ParsedObjects { get; } = [];
    public List<ConvertedObject> Objects { get; } = [];
}

public interface IConversionEngine
{
    ConversionRun Run(ConversionRequest request, IOutputSink? sink = null, IProgress<ConversionProgress>? progress = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Orchestrates a run: discover files → parse (in parallel) → build the symbol table → convert each object in a
/// deterministic order → write AL files, layouts, enums, the .NET package, translations and migration artifacts.
/// </summary>
public sealed class ConversionEngine(IEnumerable<IObjectConverter> converters) : IConversionEngine
{
    private static readonly string[] InputExtensions = [".txt", ".delta"];
    private readonly IReadOnlyList<IObjectConverter> converters = converters.ToArray();

    public ConversionRun Run(ConversionRequest request, IOutputSink? sink = null, IProgress<ConversionProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (request.Mode == RunMode.Convert && sink is null)
        {
            throw new ArgumentException("A Convert run needs an output sink.", nameof(sink));
        }

        var report = new ConversionReport
        {
            Command = request.Mode.ToString().ToLowerInvariant(),
            Source = request.Source,
            Target = request.Target,
            Options = new Dictionary<string, string>(request.ReportedOptions, StringComparer.Ordinal),
        };
        var run = new ConversionRun { Report = report };

        var files = DiscoverFiles(request.Source, report.RunDiagnostics);
        report.FilesRead = files.Count;
        var parsed = ParseFiles(files, request.Options, progress, cancellationToken);
        var sourceRoot = Directory.Exists(request.Source) ? Path.GetFullPath(request.Source) : Path.GetDirectoryName(Path.GetFullPath(request.Source)) ?? ".";
        var layout = new OutputLayout(request.Options, sourceRoot);

        foreach (var file in parsed)
        {
            report.RunDiagnostics.AddRange(file.Diagnostics);
            run.ParsedObjects.AddRange(file.Objects);
            foreach (var failure in file.Failures)
            {
                report.Objects.Add(FailedParse(failure, request, layout, sink, report));
            }
        }

        var objects = run.ParsedObjects
            .OrderBy(o => TypeOrder(o.Type)).ThenBy(o => o.Id).ThenBy(o => o.SourceFile, StringComparer.Ordinal)
            .ToList();
        var runContext = new RunContext(request.Options, SymbolTable.Build(objects));
        var dateOrders = ResolveDateOrders(objects, request.Options, report);

        for (var i = 0; i < objects.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var obj = objects[i];
            progress?.Report(new ConversionProgress(request.Mode == RunMode.Validate ? "Validating" : "Converting", i + 1, objects.Count, obj.DisplayName));
            var converted = request.Mode == RunMode.Validate
                ? new ConvertedObject(obj, ValidateOnly(obj), null, null)
                : ConvertOne(obj, runContext, dateOrders, request, layout, sink, report);
            run.Objects.Add(converted);
            report.Objects.Add(converted.Result);
        }

        if (request.Mode == RunMode.Convert && sink is not null)
        {
            WriteRunFiles(runContext, layout, sink, report);
        }

        if (files.Count > 0 && run.ParsedObjects.Count == 0 && report.Objects.Count == 0)
        {
            report.RunDiagnostics.Add(DiagnosticDescriptors.NoInputFiles.Create(
                $"No C/AL objects were found in {files.Count} file(s) under '{request.Source}'.",
                new DiagnosticLocation { File = request.Source }));
        }

        report.FinishedAt = DateTimeOffset.UtcNow;
        return run;
    }

    /// <summary>All .txt/.delta files of a file or folder (recursively), in a stable order.</summary>
    public static List<string> DiscoverFiles(string source, List<Diagnostic> diagnostics)
    {
        if (File.Exists(source))
        {
            return [Path.GetFullPath(source)];
        }

        if (!Directory.Exists(source))
        {
            diagnostics.Add(DiagnosticDescriptors.FileReadError.Create(
                $"The source '{source}' does not exist.", new DiagnosticLocation { File = source }));
            return [];
        }

        var files = Directory.EnumerateFiles(source, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, MatchCasing = MatchCasing.CaseInsensitive })
            .Where(f => InputExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .Select(Path.GetFullPath)
            .Order(StringComparer.Ordinal)
            .ToList();

        if (files.Count == 0)
        {
            diagnostics.Add(DiagnosticDescriptors.NoInputFiles.Create(
                $"No .txt or .delta files were found in '{source}'.", new DiagnosticLocation { File = source }));
        }

        return files;
    }

    private static List<TxtFileParseResult> ParseFiles(List<string> files, ConversionOptions options, IProgress<ConversionProgress>? progress, CancellationToken cancellationToken)
    {
        var parser = new TxtFileParser(TextDecoder.ResolveEncoding(options.FallbackEncoding));
        var results = new TxtFileParseResult[files.Count];
        var done = 0;
        Parallel.For(0, files.Count, new ParallelOptions { CancellationToken = cancellationToken }, i =>
        {
            results[i] = parser.ParseFile(files[i]);
            progress?.Report(new ConversionProgress("Reading", Interlocked.Increment(ref done), files.Count, Path.GetFileName(files[i])));
        });
        return [.. results];
    }

    private static int TypeOrder(CalObjectType type) => type switch
    {
        CalObjectType.Table => 0,
        CalObjectType.Page => 1,
        CalObjectType.Report => 2,
        CalObjectType.Codeunit => 3,
        CalObjectType.XmlPort => 4,
        CalObjectType.Query => 5,
        CalObjectType.Modification => 6,
        _ => 7,
    };

    private static ObjectConversionResult NewResult(CalObject obj) => new()
    {
        ObjectType = obj.Type.ToString(),
        ObjectId = obj.Id,
        ObjectName = obj.Name,
        SourceFile = obj.SourceFile,
        SourceLine = obj.Position.Line,
        Dialect = obj.Dialect.ToString(),
        Metrics = ObjectStatistics.Compute(obj),
    };

    private static ObjectConversionResult ValidateOnly(CalObject obj)
    {
        var result = NewResult(obj);
        if (obj.UnsupportedReason is { } reason)
        {
            result.Diagnostics.Add(DiagnosticDescriptors.UnsupportedObjectType.Create(reason, Location(obj)));
        }

        // Parse every body so code syntax errors are reported without converting.
        foreach (var (name, body, line) in Bodies(obj))
        {
            try
            {
                Parser.Code.CodeParser.ParseBody(body.Text);
            }
            catch (Parser.Code.CodeParseException ex)
            {
                result.Diagnostics.Add(DiagnosticDescriptors.CodeSyntaxError.Create(
                    $"Syntax error in {name}: {ex.Message}", Location(obj, line, name), construct: name, originalSource: body.Text));
            }
        }

        result.UpdateStatusFromDiagnostics();
        return result;
    }

    private static IEnumerable<(string Name, CalCodeBody Body, int Line)> Bodies(CalObject obj)
    {
        foreach (var procedure in obj.Code.Procedures)
        {
            yield return (procedure.Name, procedure.Body, procedure.Position.Line);
        }

        foreach (var item in Walk(obj))
        {
            foreach (var trigger in item.Triggers)
            {
                yield return (trigger.Name, trigger.Body, trigger.Position.Line);
            }

            foreach (var section in item.Sections)
            {
                foreach (var procedure in section.Procedures)
                {
                    yield return (procedure.Name, procedure.Body, procedure.Position.Line);
                }
            }
        }
    }

    private static IEnumerable<CalNode> Walk(CalNode node)
    {
        yield return node;
        foreach (var child in node.Sections.SelectMany(s => s.Items).SelectMany(Walk))
        {
            yield return child;
        }
    }

    /// <summary>Resolves <see cref="CalDateOrder.Auto"/> per source file (exports follow the exporting machine's locale).</summary>
    private static Dictionary<string, CalDateOrder> ResolveDateOrders(List<CalObject> objects, ConversionOptions options, ConversionReport report)
    {
        var result = new Dictionary<string, CalDateOrder>(StringComparer.Ordinal);
        if (options.CalDateOrder != CalDateOrder.Auto)
        {
            return result;
        }

        foreach (var file in objects.GroupBy(o => o.SourceFile, StringComparer.Ordinal))
        {
            var detected = DateOrderDetector.Detect(file);
            result[file.Key] = detected.Order;
            report.RunDiagnostics.Add(DiagnosticDescriptors.DateOrderDetected.Create(
                $"Date constants in '{Path.GetFileName(file.Key)}' are read as {detected.Order}: {detected.Evidence}.",
                new DiagnosticLocation { File = file.Key },
                severity: detected.Conflicting ? DiagnosticSeverity.Warning : null));
        }

        return result;
    }

    private ConvertedObject ConvertOne(CalObject obj, RunContext runContext, Dictionary<string, CalDateOrder> dateOrders, ConversionRequest request, OutputLayout layout, IOutputSink? sink, ConversionReport report)
    {
        var result = NewResult(obj);
        if (!request.Options.ShouldConvert(obj.Type, obj.Id))
        {
            result.Status = ObjectConversionStatus.Skipped;
            return new ConvertedObject(obj, result, null, null);
        }

        var converter = obj.UnsupportedReason is null ? converters.FirstOrDefault(c => c.CanConvert(obj)) : null;
        if (converter is null)
        {
            var reason = obj.UnsupportedReason ?? $"Cal2AL has no converter for {obj.Type} objects.";
            result.Diagnostics.Add(DiagnosticDescriptors.UnsupportedObjectType.Create(reason, Location(obj), construct: obj.Type.ToString(), reason: reason));
            result.Status = ObjectConversionStatus.Failed;
            WriteArtifact(obj.RawText, ArtifactName(obj), result, layout, sink, report, request.Mode);
            return new ConvertedObject(obj, result, null, null);
        }

        var context = new ObjectContext(runContext, obj);
        if (dateOrders.TryGetValue(obj.SourceFile, out var dateOrder))
        {
            context.DateOrder = dateOrder;
        }
        ObjectConversionOutput output;
        try
        {
            output = converter.Convert(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result.Diagnostics.AddRange(context.Diagnostics);
            result.Diagnostics.Add(DiagnosticDescriptors.ConverterFailed.Create(
                $"Converting {obj.DisplayName} failed: {ex.GetType().Name}: {ex.Message}",
                Location(obj, context.CurrentElement, context.CurrentMember), reason: ex.ToString()));
            result.Status = ObjectConversionStatus.Failed;
            WriteArtifact(obj.RawText, ArtifactName(obj), result, layout, sink, report, request.Mode);
            return new ConvertedObject(obj, result, null, null);
        }

        result.Diagnostics.AddRange(context.Diagnostics);
        result.Metrics = result.Metrics with
        {
            Statements = context.Statements,
            DatabaseOperations = context.DatabaseOperations,
            OutputStatements = context.OutputStatements,
            OutputDatabaseOperations = context.OutputDatabaseOperations,
        };

        string? text = null;
        if (output.AlObject is { } al)
        {
            if (request.Mode == RunMode.Convert && sink is not null)
            {
                text = WriteObject(obj, al, output, layout, sink, result, report);
            }
            else
            {
                text = new AlWriter().Render(al);
            }
        }
        else
        {
            result.Status = ObjectConversionStatus.Failed;
        }

        foreach (var artifact in output.Files.Where(f => f.Kind == AuxiliaryFileKind.MigrationArtifact))
        {
            WriteArtifactBytes(artifact.Content, artifact.FileName, result, layout, sink, report, request.Mode);
        }

        result.UpdateStatusFromDiagnostics();
        return new ConvertedObject(obj, result, output.AlObject, text);
    }

    private static string WriteObject(CalObject obj, AlObject al, ObjectConversionOutput output, OutputLayout layout, IOutputSink sink, ObjectConversionResult result, ConversionReport report)
    {
        foreach (var file in output.Files.Where(f => f.Kind is AuxiliaryFileKind.RdlcLayout or AuxiliaryFileKind.WordLayout))
        {
            var path = layout.LayoutPath(file.FileName);
            if (!string.Equals(Path.GetFileName(path), file.FileName, StringComparison.OrdinalIgnoreCase))
            {
                // The name had to be made unique: point the layout property at the file actually written.
                var newName = Path.GetFileName(path);
                for (var i = 0; i < al.Properties.Count; i++)
                {
                    if (al.Properties[i].Value.Contains(file.FileName, StringComparison.Ordinal))
                    {
                        al.Properties[i] = al.Properties[i] with { Value = al.Properties[i].Value.Replace(file.FileName, newName, StringComparison.Ordinal) };
                    }
                }

                result.Diagnostics.Add(DiagnosticDescriptors.FileNameCollision.Create(
                    $"Layout file '{file.FileName}' already exists in this run; it was written as '{newName}'.", Location(obj)));
            }

            Write(sink, path, file.Content, result, report);
        }

        var target = al.Extends is null ? null : AlSyntax.Unquote(al.Extends);
        var (alPath, renamed) = layout.ObjectPath(obj, al.Kind, al.Id, al.Name, target, null);
        if (renamed)
        {
            result.Diagnostics.Add(DiagnosticDescriptors.FileNameCollision.Create(
                $"Another object already uses the file name; this object was written to '{alPath}'.", Location(obj)));
        }

        var text = new AlWriter().Render(al);
        try
        {
            sink.WriteText(alPath, text);
            result.OutputFiles.Add(alPath);
            report.GeneratedFiles.Add(alPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            result.Diagnostics.Add(DiagnosticDescriptors.OutputWriteError.Create($"Cannot write '{alPath}': {ex.Message}", Location(obj), reason: ex.Message));
            result.Status = ObjectConversionStatus.Failed;
        }

        return text;
    }

    private static void Write(IOutputSink sink, string path, byte[] content, ObjectConversionResult result, ConversionReport report)
    {
        try
        {
            sink.Write(path, content);
            result.OutputFiles.Add(path);
            report.GeneratedFiles.Add(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            result.Diagnostics.Add(DiagnosticDescriptors.OutputWriteError.Create(
                $"Cannot write '{path}': {ex.Message}", new DiagnosticLocation { File = path }, reason: ex.Message));
        }
    }

    private static string ArtifactName(CalObject obj) =>
        $"{obj.Type}{obj.Id.ToString(CultureInfo.InvariantCulture)}.{AlSyntax.FileNameSafe(obj.Name)}.txt";

    private static void WriteArtifact(string text, string fileName, ObjectConversionResult result, OutputLayout layout, IOutputSink? sink, ConversionReport report, RunMode mode) =>
        WriteArtifactBytes(Encoding.UTF8.GetBytes(text.Replace("\n", "\r\n", StringComparison.Ordinal)), fileName, result, layout, sink, report, mode);

    private static void WriteArtifactBytes(byte[] content, string fileName, ObjectConversionResult result, OutputLayout layout, IOutputSink? sink, ConversionReport report, RunMode mode)
    {
        if (mode != RunMode.Convert || sink is null)
        {
            return;
        }

        var path = layout.ArtifactPath(fileName);
        Write(sink, path, content, result, report);
        result.Diagnostics.Add(DiagnosticDescriptors.ArtifactWritten.Create(
            $"The original text was saved as migration artifact '{path}'.",
            new DiagnosticLocation { File = result.SourceFile, Line = result.SourceLine, ObjectType = result.ObjectType, ObjectId = result.ObjectId, ObjectName = result.ObjectName }));
    }

    private static ObjectConversionResult FailedParse(FailedObject failure, ConversionRequest request, OutputLayout layout, IOutputSink? sink, ConversionReport report)
    {
        var result = new ObjectConversionResult
        {
            ObjectType = failure.Type.ToString(),
            ObjectId = failure.Id,
            ObjectName = failure.Name,
            SourceFile = failure.SourceFile,
            SourceLine = failure.Line,
            Status = ObjectConversionStatus.Failed,
        };
        result.Diagnostics.Add(failure.Diagnostic);
        var name = $"{failure.Type}{failure.Id.ToString(CultureInfo.InvariantCulture)}.{AlSyntax.FileNameSafe(failure.Name)}.txt";
        WriteArtifact(failure.RawText, name, result, layout, sink, report, request.Mode);
        return result;
    }

    private static void WriteRunFiles(RunContext runContext, OutputLayout layout, IOutputSink sink, ConversionReport report)
    {
        var diagnostics = report.RunDiagnostics;

        foreach (var declaration in runContext.Enums.Enums)
        {
            var al = PackageWriters.EnumObject(declaration);
            var path = layout.RunFilePath("enum", declaration.Id, declaration.Name);
            WriteRunText(sink, path, new AlWriter().Render(al), report, diagnostics);
        }

        var dotnet = runContext.DotNet.Types;
        if (dotnet.Count > 0)
        {
            WriteRunText(sink, layout.FixedPath("DotNet.Package.al"), PackageWriters.DotNetPackage(dotnet), report, diagnostics);
        }

        if (runContext.Options.GenerateTranslations)
        {
            var units = runContext.Translations.Units;
            foreach (var language in runContext.Translations.Languages)
            {
                var folder = runContext.Options.TranslationFolder.Trim('/', '\\');
                var fileName = $"{runContext.Options.TranslationFileName}.{language}.xlf";
                var path = layout.FixedPath(folder.Length == 0 ? fileName : $"{folder}/{fileName}");
                WriteRunText(sink, path, PackageWriters.Xliff(units, language, runContext.Options.TranslationFileName), report, diagnostics);
            }
        }
    }

    private static void WriteRunText(IOutputSink sink, string path, string text, ConversionReport report, List<Diagnostic> diagnostics)
    {
        try
        {
            sink.WriteText(path, text);
            report.GeneratedFiles.Add(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            diagnostics.Add(DiagnosticDescriptors.OutputWriteError.Create($"Cannot write '{path}': {ex.Message}", new DiagnosticLocation { File = path }, reason: ex.Message));
        }
    }

    private static DiagnosticLocation Location(CalObject obj, int? line = null, string? member = null) => new()
    {
        File = obj.SourceFile,
        Line = line ?? obj.Position.Line,
        ObjectType = obj.Type.ToString(),
        ObjectId = obj.Id,
        ObjectName = obj.Name,
        Member = member,
    };

    private static DiagnosticLocation Location(CalObject obj, string? element, string? member) => new()
    {
        File = obj.SourceFile,
        Line = obj.Position.Line,
        ObjectType = obj.Type.ToString(),
        ObjectId = obj.Id,
        ObjectName = obj.Name,
        Element = element,
        Member = member,
    };
}
