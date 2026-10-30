using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundImplicitReceiver : BoundExpression
{
	public BoundImplicitReceiver(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.ImplicitReceiver, syntax, type, hasErrors)
	{
	}

	public BoundImplicitReceiver(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.ImplicitReceiver, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitImplicitReceiver(this);
	}

	public BoundImplicitReceiver Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundImplicitReceiver(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
