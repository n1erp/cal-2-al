using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundBadExpression : BoundExpression, IInvalidExpression, IOperation
{
	private readonly LookupResultKind _ResultKind;

	public override LookupResultKind ResultKind => _ResultKind;

	public ImmutableArray<Symbol> Symbols { get; private set; }

	public ImmutableArray<BoundNode> ChildBoundNodes { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.InvalidExpression;

	public BoundBadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundNode> childBoundNodes, TypeSymbol type)
		: this(syntax, resultKind, symbols, childBoundNodes, type, hasErrors: true)
	{
		DebugAssertHelper.Assert((object)type != null);
	}

	public BoundBadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundNode> childBoundNodes, TypeSymbol type, bool hasErrors = false)
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

	public BoundBadExpression Update(LookupResultKind resultKind, ImmutableArray<Symbol> symbols, ImmutableArray<BoundNode> childBoundNodes, TypeSymbol type)
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

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInvalidExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInvalidExpression(this, argument);
	}
}
