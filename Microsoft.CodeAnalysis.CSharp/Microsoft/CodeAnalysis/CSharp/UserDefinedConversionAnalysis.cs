using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class UserDefinedConversionAnalysis
{
	public readonly TypeSymbol FromType;

	public readonly TypeSymbol ToType;

	public readonly MethodSymbol Operator;

	public readonly Conversion SourceConversion;

	public readonly Conversion TargetConversion;

	public readonly UserDefinedConversionAnalysisKind Kind;

	public static UserDefinedConversionAnalysis Normal(MethodSymbol op, Conversion sourceConversion, Conversion targetConversion, TypeSymbol fromType, TypeSymbol toType)
	{
		return new UserDefinedConversionAnalysis(UserDefinedConversionAnalysisKind.ApplicableInNormalForm, op, sourceConversion, targetConversion, fromType, toType);
	}

	public static UserDefinedConversionAnalysis Lifted(MethodSymbol op, Conversion sourceConversion, Conversion targetConversion, TypeSymbol fromType, TypeSymbol toType)
	{
		return new UserDefinedConversionAnalysis(UserDefinedConversionAnalysisKind.ApplicableInLiftedForm, op, sourceConversion, targetConversion, fromType, toType);
	}

	private UserDefinedConversionAnalysis(UserDefinedConversionAnalysisKind kind, MethodSymbol op, Conversion sourceConversion, Conversion targetConversion, TypeSymbol fromType, TypeSymbol toType)
	{
		Kind = kind;
		Operator = op;
		SourceConversion = sourceConversion;
		TargetConversion = targetConversion;
		FromType = fromType;
		ToType = toType;
	}
}
