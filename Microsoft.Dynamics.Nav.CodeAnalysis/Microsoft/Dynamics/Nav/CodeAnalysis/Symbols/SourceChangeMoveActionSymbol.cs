using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeMoveActionSymbol : SourceChangeMoveSymbol
{
	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.Action;

	internal new ActionMoveChangeSyntax Syntax => (ActionMoveChangeSyntax)base.Syntax;

	public static SourceChangeMoveActionSymbol Create(ApplicationObjectExtensionTypeSymbol owner, ActionMoveChangeSyntax syntax)
	{
		return new SourceChangeMoveActionSymbol(owner, syntax);
	}

	private SourceChangeMoveActionSymbol(ApplicationObjectExtensionTypeSymbol owner, ActionMoveChangeSyntax syntax)
		: base(owner, syntax)
	{
		ChangeKind = Syntax.ChangeKeyword.Kind.ToChangeKind();
	}

	protected override Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		return this.GetActionAnchor(Syntax.Anchor, diagnostics);
	}

	protected override ImmutableArray<Symbol> LazyGetMovedSymbols(DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return this.GetMovedSymbolsInTargetPage(targetPage, Syntax.Actions, (Symbol s) => s.Kind == SymbolKind.Action && ((ActionSymbol)s).ActionKind != ActionKind.Area, ErrorCode.ERR_ActionNotFound, ErrorCode.INF_PERS_ActionNotFound, diagnostics);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return MovedSymbols;
	}
}
