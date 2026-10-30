using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundPairOfConstantValues : BoundExpression
{
	public ConstantValue First { get; private set; }

	public ConstantValue Second { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundPairOfConstantValues(SyntaxNode syntax, ConstantValue first, ConstantValue second, TypeSymbol type, bool hasErrors)
		: base(BoundKind.PairOfConstantValues, syntax, type, hasErrors)
	{
		First = first;
		Second = second;
	}

	public BoundPairOfConstantValues(SyntaxNode syntax, ConstantValue first, ConstantValue second, TypeSymbol type)
		: base(BoundKind.PairOfConstantValues, syntax, type)
	{
		First = first;
		Second = second;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPairOfConstantValues(this);
	}

	public BoundPairOfConstantValues Update(ConstantValue first, ConstantValue second, TypeSymbol type)
	{
		if (first != First || second != Second || type != Type)
		{
			return new BoundPairOfConstantValues(Syntax, first, second, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
