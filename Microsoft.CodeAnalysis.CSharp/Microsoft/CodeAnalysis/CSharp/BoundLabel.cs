using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLabel : BoundExpression
{
	public override Symbol ExpressionSymbol => Label;

	public LabelSymbol Label { get; }

	public BoundLabel(SyntaxNode syntax, LabelSymbol label, TypeSymbol type, bool hasErrors)
		: base(BoundKind.Label, syntax, type, hasErrors)
	{
		Label = label;
	}

	public BoundLabel(SyntaxNode syntax, LabelSymbol label, TypeSymbol type)
		: base(BoundKind.Label, syntax, type)
	{
		Label = label;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLabel(this);
	}

	public BoundLabel Update(LabelSymbol label, TypeSymbol type)
	{
		if (label != Label || type != Type)
		{
			return new BoundLabel(Syntax, label, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
