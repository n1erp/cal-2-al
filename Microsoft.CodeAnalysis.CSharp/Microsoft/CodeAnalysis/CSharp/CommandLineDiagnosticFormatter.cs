using System;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CommandLineDiagnosticFormatter : CSharpDiagnosticFormatter
{
	private readonly string _baseDirectory;

	private readonly Lazy<string> _lazyNormalizedBaseDirectory;

	private readonly bool _displayFullPaths;

	private readonly bool _displayEndLocations;

	internal CommandLineDiagnosticFormatter(string baseDirectory, bool displayFullPaths, bool displayEndLocations)
	{
		_baseDirectory = baseDirectory;
		_displayFullPaths = displayFullPaths;
		_displayEndLocations = displayEndLocations;
		_lazyNormalizedBaseDirectory = new Lazy<string>(() => FileUtilities.TryNormalizeAbsolutePath(baseDirectory));
	}

	internal override string FormatSourceSpan(LinePositionSpan span, IFormatProvider formatter)
	{
		if (_displayEndLocations)
		{
			return string.Format(formatter, "({0},{1},{2},{3})", span.Start.Line + 1, span.Start.Character + 1, span.End.Line + 1, span.End.Character + 1);
		}
		return string.Format(formatter, "({0},{1})", span.Start.Line + 1, span.Start.Character + 1);
	}

	internal override string FormatSourcePath(string path, string basePath, IFormatProvider formatter)
	{
		string text = FileUtilities.NormalizeRelativePath(path, basePath, _baseDirectory);
		if (text == null)
		{
			return path;
		}
		if (!_displayFullPaths)
		{
			return RelativizeNormalizedPath(text);
		}
		return text;
	}

	internal string RelativizeNormalizedPath(string normalizedPath)
	{
		string value = _lazyNormalizedBaseDirectory.Value;
		if (value == null)
		{
			return normalizedPath;
		}
		if (PathUtilities.IsSameDirectoryOrChildOf(PathUtilities.GetDirectoryName(normalizedPath), value))
		{
			return normalizedPath.Substring(PathUtilities.IsDirectorySeparator(value.Last()) ? value.Length : (value.Length + 1));
		}
		return normalizedPath;
	}
}
