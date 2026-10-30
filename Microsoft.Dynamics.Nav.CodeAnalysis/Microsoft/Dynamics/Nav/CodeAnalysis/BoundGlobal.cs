using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundGlobal : BoundExpression, IGlobalReferenceExpression, IOperation
{
	public override Symbol ExpressionSymbol => GlobalSymbol;

	public VariableSymbol GlobalSymbol { get; private set; }

	IVariableSymbol IGlobalReferenceExpression.GlobalVariable => GlobalSymbol;

	protected override OperationKind ExpressionKind => OperationKind.GlobalReferenceExpression;

	public BoundGlobal(SyntaxNode syntax, VariableSymbol globalSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Global, syntax, type, hasErrors)
	{
		GlobalSymbol = globalSymbol;
	}

	public BoundGlobal(SyntaxNode syntax, VariableSymbol globalSymbol, TypeSymbol type)
		: base(BoundKind.Global, syntax, type)
	{
		GlobalSymbol = globalSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitGlobal(this);
	}

	public BoundGlobal Update(VariableSymbol globalSymbol, TypeSymbol type)
	{
		if (globalSymbol != GlobalSymbol || type != Type)
		{
			return new BoundGlobal(Syntax, globalSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitGlobalReferenceExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitGlobalReferenceExpression(this, argument);
	}
}
