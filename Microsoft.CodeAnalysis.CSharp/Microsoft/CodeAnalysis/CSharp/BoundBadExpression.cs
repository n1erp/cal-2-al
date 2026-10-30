using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBadExpression : BoundExpression
{
	private readonly LookupResultKind _ResultKind;

	protected override ImmutableArray<BoundNode> Children => StaticCast<BoundNode>.From(ChildBoundNodes);

	public override LookupResultKind ResultKind => _ResultKind;

	public ImmutableArray<Symbol> Symbols { get; }

	public ImmutableArray<BoundExpression> ChildBoundNodes { get; }

	public BoundBadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundExpression> childBoundNodes, TypeSymbol type)
		: this(syntax, resultKind, symbols, childBoundNodes, type, hasErrors: true)
	{
	}

	public BoundBadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundExpression> childBoundNodes, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.BadExpression, syntax, type, hasErrors || childBoundNodes.HasErrors())
	{
		_ResultKind = resultKind;
		Symbols = symbols;
		ChildBoundNodes = childBoundNodes;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBadExpression(this);
	}

	public BoundBadExpression Update(LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundExpression> childBoundNodes, TypeSymbol type)
	{
		if (resultKind != ResultKind || symbols != Symbols || childBoundNodes != ChildBoundNodes || type != Type)
		{
			return new BoundBadExpression(Syntax, resultKind, symbols, childBoundNodes, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
