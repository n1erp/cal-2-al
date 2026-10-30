using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundLocal : BoundExpression, ILocalReferenceExpression, IOperation
{
	public override Symbol ExpressionSymbol => LocalSymbol;

	public VariableSymbol LocalSymbol { get; private set; }

	IVariableSymbol ILocalReferenceExpression.LocalVariable => LocalSymbol;

	protected override OperationKind ExpressionKind => OperationKind.LocalReferenceExpression;

	public BoundLocal(SyntaxNode syntax, VariableSymbol localSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Local, syntax, type, hasErrors)
	{
		LocalSymbol = localSymbol;
	}

	public BoundLocal(SyntaxNode syntax, VariableSymbol localSymbol, TypeSymbol type)
		: base(BoundKind.Local, syntax, type)
	{
		LocalSymbol = localSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLocal(this);
	}

	public BoundLocal Update(VariableSymbol localSymbol, TypeSymbol type)
	{
		if (localSymbol != LocalSymbol || type != Type)
		{
			return new BoundLocal(Syntax, localSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLocalReferenceExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLocalReferenceExpression(this, argument);
	}
}
