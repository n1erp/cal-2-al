using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeAddViewSymbol : SourceChangeAddSymbol
{
	public override string Name { get; }

	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind { get; } = SymbolKind.View;

	private new ViewAddChangeSyntax Syntax => (ViewAddChangeSyntax)base.Syntax;

	public static SourceChangeAddViewSymbol Create(Symbol owner, ViewAddChangeSyntax syntax)
	{
		return new SourceChangeAddViewSymbol(owner, syntax);
	}

	private SourceChangeAddViewSymbol(Symbol owner, ViewAddChangeSyntax syntax)
		: base(owner, syntax)
	{
		ChangeKind = syntax.ChangeKeyword.Kind.ToChangeKind();
		Name = syntax.GetName();
	}

	protected override Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		return this.GetViewAnchor(Syntax.Anchor, diagnostics);
	}

	protected override ImmutableArray<Symbol> LazyGetAddedSymbols()
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (PageViewSyntax view in Syntax.Views)
		{
			instance.Add(SourceViewSymbol.Create(this, view));
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return AddedSymbols;
	}
}
