using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundTypeOfOperator : BoundTypeOf
{
	public BoundTypeExpression SourceType { get; }

	public BoundTypeOfOperator(SyntaxNode syntax, BoundTypeExpression sourceType, MethodSymbol getTypeFromHandle, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.TypeOfOperator, syntax, getTypeFromHandle, type, hasErrors || sourceType.HasErrors())
	{
		SourceType = sourceType;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitTypeOfOperator(this);
	}

	public BoundTypeOfOperator Update(BoundTypeExpression sourceType, MethodSymbol getTypeFromHandle, TypeSymbol type)
	{
		if (sourceType != SourceType || getTypeFromHandle != GetTypeFromHandle || type != Type)
		{
			return new BoundTypeOfOperator(Syntax, sourceType, getTypeFromHandle, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
