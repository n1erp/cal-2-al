using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundReturnValue : BoundExpression, IReturnValueReferenceExpression, IOperation
{
	public override Symbol ExpressionSymbol => ReturnValueSymbol;

	public ReturnValueSymbol ReturnValueSymbol { get; private set; }

	IReturnValueSymbol IReturnValueReferenceExpression.ReturnValue => ReturnValueSymbol;

	protected override OperationKind ExpressionKind => OperationKind.ReturnValueReferenceExpression;

	public BoundReturnValue(SyntaxNode syntax, ReturnValueSymbol returnValueSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ReturnValue, syntax, type, hasErrors)
	{
		ReturnValueSymbol = returnValueSymbol;
	}

	public BoundReturnValue(SyntaxNode syntax, ReturnValueSymbol returnValueSymbol, TypeSymbol type)
		: base(BoundKind.ReturnValue, syntax, type)
	{
		ReturnValueSymbol = returnValueSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitReturnValue(this);
	}

	public BoundReturnValue Update(ReturnValueSymbol returnValueSymbol, TypeSymbol type)
	{
		if (returnValueSymbol != ReturnValueSymbol || type != Type)
		{
			return new BoundReturnValue(Syntax, returnValueSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitReturnValueReferenceExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitReturnValueReferenceExpression(this, argument);
	}
}
