using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundHostObjectMemberReference : BoundExpression
{
	public BoundHostObjectMemberReference(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.HostObjectMemberReference, syntax, type, hasErrors)
	{
	}

	public BoundHostObjectMemberReference(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.HostObjectMemberReference, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitHostObjectMemberReference(this);
	}

	public BoundHostObjectMemberReference Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundHostObjectMemberReference(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
