using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeMoveControlSymbol : SourceChangeMoveSymbol
{
	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.Control;

	internal new ControlMoveChangeSyntax Syntax => (ControlMoveChangeSyntax)base.Syntax;

	public static SourceChangeMoveControlSymbol Create(Symbol owner, ControlMoveChangeSyntax syntax)
	{
		return new SourceChangeMoveControlSymbol(owner, syntax);
	}

	private SourceChangeMoveControlSymbol(Symbol owner, ControlMoveChangeSyntax syntax)
		: base(owner, syntax)
	{
		ChangeKind = Syntax.ChangeKeyword.Kind.ToChangeKind();
	}

	protected override Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		return this.GetControlAnchor(Syntax.Anchor, diagnostics);
	}

	protected override ImmutableArray<Symbol> LazyGetMovedSymbols(DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return this.GetMovedSymbolsInTargetPage(targetPage, Syntax.Controls, (Symbol s) => s.Kind == SymbolKind.Control && ((ControlSymbol)s).ControlKind != ControlKind.Area, ErrorCode.ERR_ControlNotFound, ErrorCode.INF_PERS_ControlNotFound, diagnostics);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return MovedSymbols;
	}
}
