using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDefaultExpression : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public override object Display => ((object)Type) ?? ((object)"default");

	public ConstantValue ConstantValueOpt { get; }

	public BoundDefaultExpression(SyntaxNode syntax, TypeSymbol type, bool hasErrors = false)
		: this(syntax, type.GetDefaultValue(), type, hasErrors)
	{
	}

	public BoundDefaultExpression(SyntaxNode syntax, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors)
		: base(BoundKind.DefaultExpression, syntax, type, hasErrors)
	{
		ConstantValueOpt = constantValueOpt;
	}

	public BoundDefaultExpression(SyntaxNode syntax, ConstantValue constantValueOpt, TypeSymbol type)
		: base(BoundKind.DefaultExpression, syntax, type)
	{
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDefaultExpression(this);
	}

	public BoundDefaultExpression Update(ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundDefaultExpression(Syntax, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
