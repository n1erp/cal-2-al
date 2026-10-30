using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseArgument : Operation, IArgumentOperation, IOperation
{
	public ArgumentKind ArgumentKind { get; }

	public IParameterSymbol Parameter { get; }

	internal IConvertibleConversion InConversionConvertibleOpt { get; }

	internal IConvertibleConversion OutConversionConvertibleOpt { get; }

	public CommonConversion InConversion => InConversionConvertibleOpt?.ToCommonConversion() ?? Identity();

	public CommonConversion OutConversion => OutConversionConvertibleOpt?.ToCommonConversion() ?? Identity();

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
		}
	}

	public abstract IOperation Value { get; }

	protected BaseArgument(ArgumentKind argumentKind, IParameterSymbol parameter, IConvertibleConversion inConversionOpt, IConvertibleConversion outConversionOpt, SemanticModel semanticModel, SyntaxNode syntax, bool isImplicit)
		: base(OperationKind.Argument, semanticModel, syntax, null, default, isImplicit)
	{
		ArgumentKind = argumentKind;
		Parameter = parameter;
		InConversionConvertibleOpt = inConversionOpt;
		OutConversionConvertibleOpt = outConversionOpt;
	}

	private static CommonConversion Identity()
	{
		return new CommonConversion(exists: true, isIdentity: true, isNumeric: false, isReference: false, isImplicit: true, null);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArgument(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitArgument(this, argument);
	}
}
