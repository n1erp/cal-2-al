using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class ConversionKindExtensions
{
	public static bool IsDynamic(this ConversionKind conversionKind)
	{
		if (conversionKind != ConversionKind.ImplicitDynamic)
		{
			return conversionKind == ConversionKind.ExplicitDynamic;
		}
		return true;
	}

	public static bool IsImplicitConversion(this ConversionKind conversionKind)
	{
		switch (conversionKind)
		{
		case ConversionKind.NoConversion:
			return false;
		case ConversionKind.Identity:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ImplicitThrow:
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ImplicitNullable:
		case ConversionKind.DefaultOrNullLiteral:
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
		case ConversionKind.PointerToVoid:
		case ConversionKind.NullToPointer:
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ImplicitConstant:
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.AnonymousFunction:
		case ConversionKind.MethodGroup:
		case ConversionKind.InterpolatedString:
		case ConversionKind.Deconstruction:
		case ConversionKind.StackAllocToPointerType:
		case ConversionKind.StackAllocToSpanType:
			return true;
		case ConversionKind.ExplicitTupleLiteral:
		case ConversionKind.ExplicitTuple:
		case ConversionKind.ExplicitDynamic:
		case ConversionKind.ExplicitNumeric:
		case ConversionKind.ExplicitEnumeration:
		case ConversionKind.ExplicitNullable:
		case ConversionKind.ExplicitReference:
		case ConversionKind.Unboxing:
		case ConversionKind.ExplicitUserDefined:
		case ConversionKind.PointerToPointer:
		case ConversionKind.IntegerToPointer:
		case ConversionKind.PointerToInteger:
		case ConversionKind.IntPtr:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(conversionKind);
		}
	}

	public static bool IsUserDefinedConversion(this ConversionKind conversionKind)
	{
		if (conversionKind == ConversionKind.ImplicitUserDefined || conversionKind == ConversionKind.ExplicitUserDefined)
		{
			return true;
		}
		return false;
	}

	public static bool IsPointerConversion(this ConversionKind kind)
	{
		if (kind - 13 <= ConversionKind.Identity || kind - 27 <= ConversionKind.ImplicitNumeric)
		{
			return true;
		}
		return false;
	}
}
