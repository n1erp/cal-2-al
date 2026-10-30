using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
public sealed class AnalyzerImageReference : AnalyzerReference
{
	private readonly ImmutableArray<DiagnosticAnalyzer> _analyzers;

	private readonly string _fullPath;

	private readonly string _display;

	private readonly string _id;

	public override string FullPath => _fullPath;

	public override string Display => _display ?? _fullPath ?? CodeAnalysisResources.InMemoryAssembly;

	public override object Id => _id;

	public AnalyzerImageReference(ImmutableArray<DiagnosticAnalyzer> analyzers, string fullPath = null, string display = null)
	{
		if (analyzers.Any((DiagnosticAnalyzer a) => a == null))
		{
			throw new ArgumentException("Cannot have null-valued analyzer", "analyzers");
		}
		_analyzers = analyzers;
		_fullPath = fullPath;
		_display = display;
		_id = Guid.NewGuid().ToString();
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzersForAllLanguages()
	{
		return _analyzers;
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(string language)
	{
		return _analyzers;
	}

	private string GetDebuggerDisplay()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Assembly");
		if (_fullPath != null)
		{
			stringBuilder.Append(" Path='");
			stringBuilder.Append(_fullPath);
			stringBuilder.Append("'");
		}
		if (_display != null)
		{
			stringBuilder.Append(" Display='");
			stringBuilder.Append(_display);
			stringBuilder.Append("'");
		}
		return stringBuilder.ToString();
	}
}
