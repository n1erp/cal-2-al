using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseCoalesceExpression : Operation, ICoalesceOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
			if (WhenNull != null)
			{
				yield return WhenNull;
			}
		}
	}

	public abstract IOperation Value { get; }

	public abstract IOperation WhenNull { get; }

	internal IConvertibleConversion ConvertibleValueConversion { get; }

	public CommonConversion ValueConversion => ConvertibleValueConversion.ToCommonConversion();

	protected BaseCoalesceExpression(IConvertibleConversion convertibleValueConversion, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Coalesce, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ConvertibleValueConversion = convertibleValueConversion;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCoalesce(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCoalesce(this, argument);
	}
}
