using System.Text;

namespace Cal2AL.Converter.Engine;

/// <summary>Where generated files go. Paths are relative to the output root and use '/' separators.</summary>
public interface IOutputSink
{
    /// <summary>A readable description of the destination, for reports.</summary>
    string Description { get; }

    void Write(string relativePath, byte[] content);
}

public static class OutputSinkExtensions
{
    /// <summary>AL and XLIFF files are written as UTF-8 with BOM, like the AL tooling does.</summary>
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public static void WriteText(this IOutputSink sink, string relativePath, string content) =>
        sink.Write(relativePath, [.. Utf8WithBom.GetPreamble(), .. Utf8WithBom.GetBytes(content)]);
}

/// <summary>Writes files below a folder on disk. Never writes outside it.</summary>
public sealed class FileSystemOutputSink : IOutputSink
{
    private readonly string root;

    public FileSystemOutputSink(string root)
    {
        this.root = Path.GetFullPath(root);
    }

    public string Description => root;

    public void Write(string relativePath, byte[] content)
    {
        var full = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootWithSeparator, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            throw new IOException($"Refusing to write '{relativePath}' outside the output folder '{root}'.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllBytes(full, content);
    }
}

/// <summary>Keeps generated files in memory (tests, analyze/validate commands).</summary>
public sealed class InMemoryOutputSink : IOutputSink
{
    private readonly Dictionary<string, byte[]> files = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock gate = new();

    public string Description => "(memory)";

    public IReadOnlyDictionary<string, byte[]> Files
    {
        get
        {
            lock (gate)
            {
                return new Dictionary<string, byte[]>(files, StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    public void Write(string relativePath, byte[] content)
    {
        lock (gate)
        {
            files[relativePath] = content;
        }
    }

    /// <summary>The text of a generated file without the BOM, or null when it was not generated.</summary>
    public string? GetText(string relativePath)
    {
        lock (gate)
        {
            if (!files.TryGetValue(relativePath, out var bytes))
            {
                return null;
            }

            var text = Encoding.UTF8.GetString(bytes);
            return text.Length > 0 && text[0] == '﻿' ? text[1..] : text;
        }
    }
}
