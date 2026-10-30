using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public class DiagnosticFormatter
{
	internal static readonly DiagnosticFormatter Instance = new DiagnosticFormatter();

	public virtual string Format(Diagnostic diagnostic, IFormatProvider formatter = null)
	{
		if (diagnostic == null)
		{
			throw new ArgumentNullException("diagnostic");
		}
		CultureInfo cultureInfo = formatter as CultureInfo;
		LocationKind kind = diagnostic.Location.Kind;
		if (kind == LocationKind.SourceFile || (uint)(kind - 3) <= 1u)
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
				return string.Format(formatter, "{0}{1}: {2}: {3}", FormatSourcePath(path, basePath, formatter), FormatSourceSpan(mappedLineSpan.Span, formatter), GetMessagePrefix(diagnostic, cultureInfo), diagnostic.GetMessage(cultureInfo));
			}
		}
		return string.Format(formatter, "{0}: {1}", GetMessagePrefix(diagnostic, cultureInfo), diagnostic.GetMessage(cultureInfo));
	}

	internal virtual string FormatSourcePath(string path, string basePath, IFormatProvider formatter)
	{
		return path;
	}

	internal virtual string FormatSourceSpan(LinePositionSpan span, IFormatProvider formatter)
	{
		return string.Format(CultureInfo.InvariantCulture, "({0},{1})", span.Start.Line + 1, span.Start.Character + 1);
	}

	internal static string GetMessagePrefix(Diagnostic diagnostic, CultureInfo culture)
	{
		return string.Format(culture, "{0} {1}", diagnostic.Severity switch
		{
			DiagnosticSeverity.Hidden => (object)CodeAnalysisResources.ResourceManager.GetString("SeverityHidden", culture), 
			DiagnosticSeverity.Info => CodeAnalysisResources.ResourceManager.GetString("SeverityInfo", culture), 
			DiagnosticSeverity.Warning => CodeAnalysisResources.ResourceManager.GetString("SeverityWarning", culture), 
			DiagnosticSeverity.Error => CodeAnalysisResources.ResourceManager.GetString("SeverityError", culture), 
			_ => throw new NotImplementedException(diagnostic.Severity.ToString()), 
		}, diagnostic.Id);
	}
}
