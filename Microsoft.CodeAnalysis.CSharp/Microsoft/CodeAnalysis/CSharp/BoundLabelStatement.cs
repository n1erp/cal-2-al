using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLabelStatement : BoundStatement
{
	public LabelSymbol Label { get; }

	public BoundLabelStatement(SyntaxNode syntax, LabelSymbol label, bool hasErrors)
		: base(BoundKind.LabelStatement, syntax, hasErrors)
	{
		Label = label;
	}

	public BoundLabelStatement(SyntaxNode syntax, LabelSymbol label)
		: base(BoundKind.LabelStatement, syntax)
	{
		Label = label;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLabelStatement(this);
	}

	public BoundLabelStatement Update(LabelSymbol label)
	{
		if (label != Label)
		{
			return new BoundLabelStatement(Syntax, label, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
