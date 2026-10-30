using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundTextIndexAccess : BoundExpression, ITextIndexAccess, IOperation
{
	public override Symbol ExpressionSymbol => Type;

	public BoundExpression Expression { get; private set; }

	public BoundExpression Index { get; private set; }

	IOperation ITextIndexAccess.TextExpression => Expression;

	IOperation ITextIndexAccess.Index => Index;

	protected override OperationKind ExpressionKind => OperationKind.FieldAccess;

	public BoundTextIndexAccess(SyntaxNode syntax, BoundExpression expression, BoundExpression index, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TextIndexAccess, syntax, type, hasErrors || expression.HasErrors() || index.HasErrors())
	{
		Expression = expression;
		Index = index;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTextIndexAccess(this);
	}

	public BoundTextIndexAccess Update(BoundExpression expression, BoundExpression index, TypeSymbol type)
	{
		if (expression != Expression || index != Index || type != Type)
		{
			return new BoundTextIndexAccess(Syntax, expression, index, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTextIndexAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTextIndexAccess(this, argument);
	}
}
