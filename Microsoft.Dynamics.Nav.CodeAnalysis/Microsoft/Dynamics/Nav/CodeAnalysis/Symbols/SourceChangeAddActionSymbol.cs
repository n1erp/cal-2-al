using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeAddActionSymbol : SourceChangeAddSymbol
{
	public override string Name { get; }

	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.Action;

	private new ActionAddChangeSyntax Syntax => (ActionAddChangeSyntax)base.Syntax;

	public static SourceChangeAddActionSymbol Create(ApplicationObjectExtensionTypeSymbol owner, ActionAddChangeSyntax syntax)
	{
		return new SourceChangeAddActionSymbol(owner, syntax);
	}

	private SourceChangeAddActionSymbol(ApplicationObjectExtensionTypeSymbol owner, ActionAddChangeSyntax syntax)
		: base(owner, syntax)
	{
		ChangeKind = Syntax.ChangeKeyword.Kind.ToChangeKind();
		Name = syntax.GetName();
	}

	protected override Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		return this.GetActionAnchor(Syntax.Anchor, diagnostics);
	}

	protected override ImmutableArray<Symbol> LazyGetAddedSymbols()
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (ActionBaseSyntax action in Syntax.Actions)
		{
			instance.Add(SourceActionSymbol.Create(this, action));
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return AddedSymbols;
	}
}
