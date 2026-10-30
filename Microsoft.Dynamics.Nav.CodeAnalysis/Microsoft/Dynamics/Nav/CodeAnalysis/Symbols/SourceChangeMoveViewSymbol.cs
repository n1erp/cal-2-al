using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeMoveViewSymbol : SourceChangeMoveSymbol
{
	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind { get; } = SymbolKind.View;

	internal new ViewMoveChangeSyntax Syntax => (ViewMoveChangeSyntax)base.Syntax;

	public static SourceChangeMoveViewSymbol Create(Symbol owner, ViewMoveChangeSyntax syntax)
	{
		return new SourceChangeMoveViewSymbol(owner, syntax);
	}

	private SourceChangeMoveViewSymbol(Symbol owner, ViewMoveChangeSyntax syntax)
		: base(owner, syntax)
	{
		ChangeKind = Syntax.ChangeKeyword.Kind.ToChangeKind();
	}

	protected override Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		return this.GetViewAnchor(Syntax.Anchor, diagnostics);
	}

	protected override ImmutableArray<Symbol> LazyGetMovedSymbols(DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return this.GetMovedSymbolsInTargetPage(targetPage, Syntax.Views, (Symbol s) => s.Kind == SymbolKind.View, ErrorCode.ERR_ViewNotFound, ErrorCode.INF_PERS_ViewNotFound, diagnostics);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return MovedSymbols;
	}
}
