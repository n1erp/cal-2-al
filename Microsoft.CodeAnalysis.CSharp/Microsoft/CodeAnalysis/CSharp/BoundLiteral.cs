using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLiteral : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override object Display
	{
		get
		{
			if (!ConstantValue.IsNull)
			{
				return base.Display;
			}
			return MessageID.IDS_NULL.Localize();
		}
	}

	public ConstantValue ConstantValueOpt { get; }

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
}
