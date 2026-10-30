using System.Globalization;
using Cal2AL.Core.Model;
using Cal2AL.Generator;

namespace Cal2AL.Converter.Engine;

/// <summary>Decides where each generated file goes, and keeps file names unique within a run.</summary>
public sealed class OutputLayout(ConversionOptions options, string sourceRoot)
{
    private readonly HashSet<string> used = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock gate = new();

    /// <summary>The AL file type token used in file names (<c>Table</c>, <c>PageExt</c>, <c>Enum</c>, …).</summary>
    public static string TypeToken(string alKind) => alKind.ToLowerInvariant() switch
    {
        "table" => "Table",
        "page" => "Page",
        "report" => "Report",
        "codeunit" => "Codeunit",
        "xmlport" => "XmlPort",
        "query" => "Query",
        "tableextension" => "TableExt",
        "pageextension" => "PageExt",
        "enum" => "Enum",
        _ => AlSyntax.FileNameSafe(alKind),
    };

    /// <summary>The relative path of the .al file of an object, and whether it had to be made unique.</summary>
    public (string Path, bool Renamed) ObjectPath(CalObject source, string alKind, int id, string alName, string? targetName = null, int? targetId = null)
    {
        var isExtension = targetName is not null;
        var pattern = isExtension ? options.ExtensionFileNamePattern : options.ObjectFileNamePattern;
        var requested = Combine(Folder(source.SourceFile, alKind), Expand(pattern, alName, alKind, id, targetName, targetId) + ".al");
        var path = Claim(requested, id);
        return (path, !string.Equals(path, requested, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The relative path of a run-level file (enum objects, the .NET package).</summary>
    public string RunFilePath(string alKind, int id, string alName) =>
        Claim(Combine(options.Structure == OutputStructure.ByType ? Plural(alKind) : string.Empty,
            Expand(options.ObjectFileNamePattern, alName, alKind, id, null, null) + ".al"), id);

    public string FixedPath(string relative) => Claim(relative.Replace('\\', '/'), null);

    public string LayoutPath(string fileName) => Claim(Combine(options.LayoutFolder, fileName), null);

    public string ArtifactPath(string fileName) => Claim(Combine(Combine(options.ArtifactFolder, "artifacts"), fileName), null);

    private static string Expand(string pattern, string alName, string alKind, int id, string? targetName, int? targetId) =>
        pattern
            .Replace("{name}", AlSyntax.FileNameSafe(AlSyntax.Unquote(alName)), StringComparison.OrdinalIgnoreCase)
            .Replace("{type}", TypeToken(alKind), StringComparison.OrdinalIgnoreCase)
            .Replace("{id}", id.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{targetName}", targetName is null ? string.Empty : AlSyntax.FileNameSafe(AlSyntax.Unquote(targetName)), StringComparison.OrdinalIgnoreCase)
            .Replace("{targetId}", targetId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, StringComparison.OrdinalIgnoreCase);

    private string Folder(string sourceFile, string alKind) => options.Structure switch
    {
        OutputStructure.Flat => string.Empty,
        OutputStructure.ByType => Plural(alKind),
        _ => MirrorFolder(sourceFile),
    };

    private string MirrorFolder(string sourceFile)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(sourceFile)) ?? string.Empty;
        var relative = Path.GetRelativePath(sourceRoot, dir);
        return relative == "." || relative.StartsWith("..", StringComparison.Ordinal) ? string.Empty : relative.Replace('\\', '/');
    }

    private static string Plural(string alKind) => TypeToken(alKind) switch
    {
        "TableExt" => "TableExtensions",
        "PageExt" => "PageExtensions",
        "Query" => "Queries",
        var token => token + "s",
    };

    private static string Combine(string folder, string name) =>
        string.IsNullOrEmpty(folder) ? name : $"{folder.Trim('/', '\\').Replace('\\', '/')}/{name}";

    /// <summary>Reserves a path; on a clash inserts the object ID (or a counter) before the extension.</summary>
    private string Claim(string path, int? id)
    {
        lock (gate)
        {
            if (used.Add(path))
            {
                return path;
            }

            var extension = Path.GetExtension(path);
            var stem = path[..^extension.Length];
            if (id is int objectId && used.Add($"{stem}.{objectId.ToString(CultureInfo.InvariantCulture)}{extension}"))
            {
                return $"{stem}.{objectId.ToString(CultureInfo.InvariantCulture)}{extension}";
            }

            for (var n = 2; ; n++)
            {
                var candidate = $"{stem}.{n.ToString(CultureInfo.InvariantCulture)}{extension}";
                if (used.Add(candidate))
                {
                    return candidate;
                }
            }
        }
    }
}
