using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeAddControlSymbol : SourceChangeAddSymbol
{
	public override string Name { get; }

	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.Control;

	private new ControlAddChangeSyntax Syntax => (ControlAddChangeSyntax)base.Syntax;

	public static SourceChangeAddControlSymbol Create(Symbol owner, ControlAddChangeSyntax syntax)
	{
		return new SourceChangeAddControlSymbol(owner, syntax);
	}

	private SourceChangeAddControlSymbol(Symbol owner, ControlAddChangeSyntax syntax)
		: base(owner, syntax)
	{
		ChangeKind = syntax.ChangeKeyword.Kind.ToChangeKind();
		Name = syntax.GetName();
	}

	protected override Symbol LazyGetAnchor(DiagnosticBag diagnostics)
	{
		return this.GetControlAnchor(Syntax.Anchor, diagnostics);
	}

	protected override ImmutableArray<Symbol> LazyGetAddedSymbols()
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (ControlBaseSyntax control in Syntax.Controls)
		{
			instance.Add(SourceControlSymbol.Create(this, control));
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return AddedSymbols;
	}
}
