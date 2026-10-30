using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundParameter : BoundExpression, IParameterReferenceExpression, IOperation
{
	public override Symbol ExpressionSymbol => ParameterSymbol;

	public ParameterSymbol ParameterSymbol { get; private set; }

	IParameterSymbol IParameterReferenceExpression.Parameter => ParameterSymbol;

	protected override OperationKind ExpressionKind => OperationKind.ParameterReferenceExpression;

	public BoundParameter(SyntaxNode syntax, ParameterSymbol parameterSymbol, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Parameter, syntax, type, hasErrors)
	{
		ParameterSymbol = parameterSymbol;
	}

	public BoundParameter(SyntaxNode syntax, ParameterSymbol parameterSymbol, TypeSymbol type)
		: base(BoundKind.Parameter, syntax, type)
	{
		ParameterSymbol = parameterSymbol;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitParameter(this);
	}

	public BoundParameter Update(ParameterSymbol parameterSymbol, TypeSymbol type)
	{
		if (parameterSymbol != ParameterSymbol || type != Type)
		{
			return new BoundParameter(Syntax, parameterSymbol, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitParameterReferenceExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitParameterReferenceExpression(this, argument);
	}
}
