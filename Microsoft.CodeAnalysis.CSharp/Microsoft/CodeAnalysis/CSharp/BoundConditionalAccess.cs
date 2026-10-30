using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConditionalAccess : BoundExpression
{
	public BoundExpression Receiver { get; }

	public BoundExpression AccessExpression { get; }

	public BoundConditionalAccess(SyntaxNode syntax, BoundExpression receiver, BoundExpression accessExpression, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ConditionalAccess, syntax, type, hasErrors || receiver.HasErrors() || accessExpression.HasErrors())
	{
		Receiver = receiver;
		AccessExpression = accessExpression;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConditionalAccess(this);
	}

	public BoundConditionalAccess Update(BoundExpression receiver, BoundExpression accessExpression, TypeSymbol type)
	{
		if (receiver != Receiver || accessExpression != AccessExpression || type != Type)
		{
			return new BoundConditionalAccess(Syntax, receiver, accessExpression, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
