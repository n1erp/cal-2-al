using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseConversionExpression : Operation, IConversionOperation, IOperation
{
	internal IConvertibleConversion ConvertibleConversion { get; }

	public CommonConversion Conversion => ConvertibleConversion.ToCommonConversion();

	public bool IsTryCast { get; }

	public bool IsChecked { get; }

	public IMethodSymbol OperatorMethod => Conversion.MethodSymbol;

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operand != null)
			{
				yield return Operand;
			}
		}
	}

	public abstract IOperation Operand { get; }

	protected BaseConversionExpression(IConvertibleConversion convertibleConversion, bool isTryCast, bool isChecked, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Conversion, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsTryCast = isTryCast;
		IsChecked = isChecked;
		ConvertibleConversion = convertibleConversion;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConversion(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConversion(this, argument);
	}
}
