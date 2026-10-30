using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class ConversionKindExtensions
{
	public static bool IsImplicitConversion(this ConversionKind conversionKind)
	{
		switch (conversionKind)
		{
		case ConversionKind.NoConversion:
			return false;
		case ConversionKind.FromJoker:
		case ConversionKind.ImplicitJoker:
		case ConversionKind.ImplicitArray:
		case ConversionKind.NarrowingFromDotNet:
		case ConversionKind.WideningFromDotNet:
		case ConversionKind.PreservingFromDotNet:
		case ConversionKind.NarrowingToDotNet:
		case ConversionKind.WideningToDotNet:
		case ConversionKind.PreservingToDotNet:
		case ConversionKind.NarrowingFromDotNetToDotNet:
		case ConversionKind.WideningFromDotNetToDotNet:
		case ConversionKind.PreservingFromDotNetToDotNet:
		case ConversionKind.ImplicitNumericToSmaller:
		case ConversionKind.ImplicitTextToSmaller:
		case ConversionKind.ToOption:
		case ConversionKind.ToChar:
		case ConversionKind.ToVariant:
		case ConversionKind.FromVariant:
		case ConversionKind.ToGuid:
		case ConversionKind.ToText:
		case ConversionKind.ImplicitRecord:
		case ConversionKind.ImplicitCodeunit:
		case ConversionKind.ImplicitText:
		case ConversionKind.ToDecimal:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitOption:
		case ConversionKind.ImplicitConstant:
		case ConversionKind.Identity:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(conversionKind);
		}
	}

	public static bool IsPureImplicitConversion(this ConversionKind conversionKind)
	{
		switch (conversionKind)
		{
		case ConversionKind.NoConversion:
		case ConversionKind.FromJoker:
		case ConversionKind.ImplicitJoker:
		case ConversionKind.ImplicitArray:
		case ConversionKind.NarrowingFromDotNet:
		case ConversionKind.WideningFromDotNet:
		case ConversionKind.PreservingFromDotNet:
		case ConversionKind.NarrowingToDotNet:
		case ConversionKind.WideningToDotNet:
		case ConversionKind.PreservingToDotNet:
		case ConversionKind.NarrowingFromDotNetToDotNet:
		case ConversionKind.WideningFromDotNetToDotNet:
		case ConversionKind.PreservingFromDotNetToDotNet:
		case ConversionKind.ImplicitNumericToSmaller:
		case ConversionKind.ImplicitTextToSmaller:
		case ConversionKind.ToOption:
		case ConversionKind.ToVariant:
		case ConversionKind.FromVariant:
		case ConversionKind.ImplicitCodeunit:
		case ConversionKind.ImplicitOption:
			return false;
		case ConversionKind.ToChar:
		case ConversionKind.ToGuid:
		case ConversionKind.ToText:
		case ConversionKind.ImplicitRecord:
		case ConversionKind.ImplicitText:
		case ConversionKind.ToDecimal:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitConstant:
		case ConversionKind.Identity:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(conversionKind);
		}
	}

	public static bool IsValidForArrayConversion(this ConversionKind conversionKind)
	{
		switch (conversionKind)
		{
		case ConversionKind.ImplicitJoker:
		case ConversionKind.ImplicitTextToSmaller:
		case ConversionKind.ImplicitRecord:
		case ConversionKind.ImplicitText:
		case ConversionKind.ImplicitOption:
		case ConversionKind.Identity:
			return true;
		default:
			return false;
		}
	}

	public static bool IsImplicitToSmaller(this ConversionKind conversionKind)
	{
		if (conversionKind - 13 <= ConversionKind.FromJoker)
		{
			return true;
		}
		return false;
	}

	public static bool IsDotNetToDotNet(this ConversionKind conversionKind)
	{
		if (conversionKind - 10 <= ConversionKind.ImplicitJoker)
		{
			return true;
		}
		return false;
	}
}
