using System;
using System.Globalization;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public class DiagnosticFormatter
{
	internal static readonly DiagnosticFormatter Instance = new DiagnosticFormatter();

	public virtual string Format(Diagnostic diagnostic, IFormatProvider formatter = null)
	{
		if (diagnostic == null)
		{
			throw new ArgumentNullException("diagnostic");
		}
		CultureInfo formatProvider = formatter as CultureInfo;
		LocationKind kind = diagnostic.Location.Kind;
		if (kind == LocationKind.SourceFile || kind - 3 <= LocationKind.SourceFile)
		{
			FileLinePositionSpan lineSpan = diagnostic.Location.GetLineSpan();
			FileLinePositionSpan mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
			if (lineSpan.IsValid && mappedLineSpan.IsValid)
			{
				string path;
				string basePath;
				if (mappedLineSpan.HasMappedPath)
				{
					path = mappedLineSpan.Path;
					basePath = lineSpan.Path;
				}
				else
				{
					path = lineSpan.Path;
					basePath = null;
				}
				return string.Format(formatter, "{0}{1}: {2}: {3}", FormatSourcePath(path, basePath, formatter), FormatSourceSpan(mappedLineSpan.Span, formatter), GetMessagePrefix(diagnostic), diagnostic.GetMessage(formatProvider));
			}
		}
		return string.Format(formatter, "{0}: {1}", GetMessagePrefix(diagnostic), diagnostic.GetMessage(formatProvider));
	}

	internal virtual string FormatSourcePath(string path, string basePath, IFormatProvider formatter)
	{
		return path;
	}

	internal virtual string FormatSourceSpan(LinePositionSpan span, IFormatProvider formatter)
	{
		return $"({span.Start.Line + 1},{span.Start.Character + 1})";
	}

	internal string GetMessagePrefix(Diagnostic diagnostic)
	{
		return string.Format("{0} {1}", diagnostic.Severity switch
		{
			DiagnosticSeverity.Hidden => (object?)"hidden", 
			DiagnosticSeverity.Info => "info", 
			DiagnosticSeverity.Warning => "warning", 
			DiagnosticSeverity.Error => "error", 
			_ => throw ExceptionUtilities.UnexpectedValue(diagnostic.Severity), 
		}, diagnostic.Id);
	}
}
