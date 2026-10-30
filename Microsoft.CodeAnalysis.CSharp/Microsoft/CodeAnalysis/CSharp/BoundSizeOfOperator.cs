using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSizeOfOperator : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public BoundTypeExpression SourceType { get; }

	public ConstantValue ConstantValueOpt { get; }

	public BoundSizeOfOperator(SyntaxNode syntax, BoundTypeExpression sourceType, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.SizeOfOperator, syntax, type, hasErrors || sourceType.HasErrors())
	{
		SourceType = sourceType;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSizeOfOperator(this);
	}

	public BoundSizeOfOperator Update(BoundTypeExpression sourceType, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (sourceType != SourceType || constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundSizeOfOperator(Syntax, sourceType, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
