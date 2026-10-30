using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundLiteral : BoundExpression, ILiteralExpression, IOperation
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override Symbol ExpressionSymbol => Type;

	public ConstantValue ConstantValueOpt { get; private set; }

	string ILiteralExpression.Text => Syntax.ToString();

	protected override OperationKind ExpressionKind => OperationKind.LiteralExpression;

	public BoundLiteral(SyntaxNode syntax, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Literal, syntax, type, hasErrors)
	{
		ConstantValueOpt = constantValueOpt;
	}

	public BoundLiteral(SyntaxNode syntax, ConstantValue constantValueOpt, TypeSymbol type)
		: base(BoundKind.Literal, syntax, type)
	{
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLiteral(this);
	}

	public BoundLiteral Update(ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundLiteral(Syntax, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLiteralExpression(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLiteralExpression(this, argument);
	}
}
