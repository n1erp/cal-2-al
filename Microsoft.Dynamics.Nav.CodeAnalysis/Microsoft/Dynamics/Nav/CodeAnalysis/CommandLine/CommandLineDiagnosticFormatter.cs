using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CommandLine;

internal sealed class CommandLineDiagnosticFormatter : DiagnosticFormatter
{
	private readonly string baseDirectory;

	private readonly Lazy<string> lazyNormalizedBaseDirectory;

	private readonly bool displayFullPaths;

	private readonly bool displayEndLocations;

	internal CommandLineDiagnosticFormatter(string baseDirectory, bool displayFullPaths, bool displayEndLocations)
	{
		this.baseDirectory = baseDirectory;
		this.displayFullPaths = displayFullPaths;
		this.displayEndLocations = displayEndLocations;
		lazyNormalizedBaseDirectory = new Lazy<string>(() => PathUtilities.TryNormalizeAbsolutePath(baseDirectory));
	}

	internal override string FormatSourceSpan(LinePositionSpan span, IFormatProvider formatter)
	{
		if (displayEndLocations)
		{
			return string.Format(formatter, "({0},{1},{2},{3})", span.Start.Line + 1, span.Start.Character + 1, span.End.Line + 1, span.End.Character + 1);
		}
		return string.Format(formatter, "({0},{1})", span.Start.Line + 1, span.Start.Character + 1);
	}

	internal override string FormatSourcePath(string path, string basePath, IFormatProvider formatter)
	{
		string text = PathUtilities.NormalizeRelativePath(path, basePath, baseDirectory);
		if (text == null)
		{
			return path;
		}
		if (!displayFullPaths)
		{
			return RelativizeNormalizedPath(text);
		}
		return text;
	}

	internal string RelativizeNormalizedPath(string normalizedPath)
	{
		string value = lazyNormalizedBaseDirectory.Value;
		if (value == null)
		{
			return normalizedPath;
		}
		if (string.Compare(normalizedPath.GetDirectoryName(), 0, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0)
		{
			return normalizedPath.Substring(value.Length + 1);
		}
		return normalizedPath;
	}
}
