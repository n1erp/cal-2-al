using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class HostCompilationStartAnalysisScope : HostAnalysisScope
{
	private readonly HostSessionStartAnalysisScope sessionScope;

	public override ImmutableArray<CompilationAnalyzerAction> CompilationEndActions => base.CompilationEndActions.AddRange(sessionScope.CompilationEndActions);

	public override ImmutableArray<SemanticModelAnalyzerAction> SemanticModelActions => base.SemanticModelActions.AddRange(sessionScope.SemanticModelActions);

	public override ImmutableArray<SyntaxTreeAnalyzerAction> SyntaxTreeActions => base.SyntaxTreeActions.AddRange(sessionScope.SyntaxTreeActions);

	public override ImmutableArray<SymbolAnalyzerAction> SymbolActions => base.SymbolActions.AddRange(sessionScope.SymbolActions);

	public override ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => base.CodeBlockEndActions.AddRange(sessionScope.CodeBlockEndActions);

	public override ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => base.CodeBlockActions.AddRange(sessionScope.CodeBlockActions);

	public override bool HasCodeBlockEndActions
	{
		get
		{
			if (!base.HasCodeBlockEndActions)
			{
				return sessionScope.HasCodeBlockEndActions;
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
				return sessionScope.HasCodeBlockActions;
			}
			return true;
		}
	}

	public HostCompilationStartAnalysisScope(HostSessionStartAnalysisScope sessionScope)
	{
		this.sessionScope = sessionScope;
	}

	public override bool HasCodeBlockStartActions()
	{
		if (!base.HasCodeBlockStartActions())
		{
			return sessionScope.HasCodeBlockStartActions();
		}
		return true;
	}

	public override ImmutableArray<CodeBlockStartAnalyzerAction> GetCodeBlockStartActions()
	{
		return base.GetCodeBlockStartActions().AddRange(sessionScope.GetCodeBlockStartActions());
	}

	public override ImmutableArray<SyntaxNodeAnalyzerAction> GetSyntaxNodeActions()
	{
		return base.GetSyntaxNodeActions().AddRange(sessionScope.GetSyntaxNodeActions());
	}

	public override AnalyzerActions GetAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		AnalyzerActions analyzerActions = base.GetAnalyzerActions(analyzer);
		AnalyzerActions analyzerActions2 = sessionScope.GetAnalyzerActions(analyzer);
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
