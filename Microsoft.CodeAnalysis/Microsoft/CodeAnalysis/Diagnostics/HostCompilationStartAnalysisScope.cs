using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class HostCompilationStartAnalysisScope : HostAnalysisScope
{
	private readonly HostSessionStartAnalysisScope _sessionScope;

	public override ImmutableArray<CompilationAnalyzerAction> CompilationEndActions => base.CompilationEndActions.AddRange(_sessionScope.CompilationEndActions);

	public override ImmutableArray<SemanticModelAnalyzerAction> SemanticModelActions => base.SemanticModelActions.AddRange(_sessionScope.SemanticModelActions);

	public override ImmutableArray<SyntaxTreeAnalyzerAction> SyntaxTreeActions => base.SyntaxTreeActions.AddRange(_sessionScope.SyntaxTreeActions);

	public override ImmutableArray<SymbolAnalyzerAction> SymbolActions => base.SymbolActions.AddRange(_sessionScope.SymbolActions);

	public override ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => base.CodeBlockEndActions.AddRange(_sessionScope.CodeBlockEndActions);

	public override ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => base.CodeBlockActions.AddRange(_sessionScope.CodeBlockActions);

	public override bool HasCodeBlockEndActions
	{
		get
		{
			if (!base.HasCodeBlockEndActions)
			{
				return _sessionScope.HasCodeBlockEndActions;
			}
			return true;
		}
	}

	public override bool HasCodeBlockActions
	{
		get
		{
			if (!base.HasCodeBlockActions)
			{
				return _sessionScope.HasCodeBlockActions;
			}
			return true;
		}
	}

	public override ImmutableArray<OperationAnalyzerAction> OperationActions => base.OperationActions.AddRange(_sessionScope.OperationActions);

	public HostCompilationStartAnalysisScope(HostSessionStartAnalysisScope sessionScope)
	{
		_sessionScope = sessionScope;
	}

	public override bool HasCodeBlockStartActions<TLanguageKindEnum>()
	{
		if (!base.HasCodeBlockStartActions<TLanguageKindEnum>())
		{
			return _sessionScope.HasCodeBlockStartActions<TLanguageKindEnum>();
		}
		return true;
	}

	public override ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> GetCodeBlockStartActions<TLanguageKindEnum>()
	{
		return base.GetCodeBlockStartActions<TLanguageKindEnum>().AddRange(_sessionScope.GetCodeBlockStartActions<TLanguageKindEnum>());
	}

	public override ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> GetSyntaxNodeActions<TLanguageKindEnum>()
	{
		return base.GetSyntaxNodeActions<TLanguageKindEnum>().AddRange(_sessionScope.GetSyntaxNodeActions<TLanguageKindEnum>());
	}

	public override AnalyzerActions GetAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		AnalyzerActions analyzerActions = base.GetAnalyzerActions(analyzer);
		AnalyzerActions analyzerActions2 = _sessionScope.GetAnalyzerActions(analyzer);
		if (analyzerActions2 == null)
		{
			return analyzerActions;
		}
		if (analyzerActions == null)
		{
			return analyzerActions2;
		}
		return analyzerActions.Append(analyzerActions2);
	}

	public AnalyzerActions GetCompilationOnlyAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		return base.GetAnalyzerActions(analyzer);
	}
}
