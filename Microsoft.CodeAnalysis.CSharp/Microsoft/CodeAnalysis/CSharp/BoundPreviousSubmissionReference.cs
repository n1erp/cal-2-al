using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPreviousSubmissionReference : BoundExpression
{
	public BoundPreviousSubmissionReference(SyntaxNode syntax, TypeSymbol type, bool hasErrors)
		: base(BoundKind.PreviousSubmissionReference, syntax, type, hasErrors)
	{
	}

	public BoundPreviousSubmissionReference(SyntaxNode syntax, TypeSymbol type)
		: base(BoundKind.PreviousSubmissionReference, syntax, type)
	{
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPreviousSubmissionReference(this);
	}

	public BoundPreviousSubmissionReference Update(TypeSymbol type)
	{
		if (type != Type)
		{
			return new BoundPreviousSubmissionReference(Syntax, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
