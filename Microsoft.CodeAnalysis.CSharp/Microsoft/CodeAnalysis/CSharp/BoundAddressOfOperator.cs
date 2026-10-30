using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundAddressOfOperator : BoundExpression
{
	public BoundExpression Operand { get; }

	public bool IsManaged { get; }

	public BoundAddressOfOperator(SyntaxNode syntax, BoundExpression operand, TypeSymbol type, bool hasErrors = false)
		: this(syntax, operand, isManaged: false, type, hasErrors)
	{
	}

	public BoundAddressOfOperator(SyntaxNode syntax, BoundExpression operand, bool isManaged, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.AddressOfOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
		IsManaged = isManaged;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAddressOfOperator(this);
	}

	public BoundAddressOfOperator Update(BoundExpression operand, bool isManaged, TypeSymbol type)
	{
		if (operand != Operand || isManaged != IsManaged || type != Type)
		{
			return new BoundAddressOfOperator(Syntax, operand, isManaged, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
