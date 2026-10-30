using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundBaseReference : BoundExpression
{
	public override bool SuppressVirtualCalls => true;

	public BoundBaseReference(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.BaseReference, syntax, type, hasErrors)
	{
	}

	public BoundBaseReference(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.BaseReference, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitBaseReference(this);
	}

	public BoundBaseReference Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundBaseReference(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
