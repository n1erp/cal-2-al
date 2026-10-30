namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class SpecialTypeExtensions
{
	public static bool IsIntegralType(this SpecialType specialType)
	{
		if ((uint)(specialType - 9) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool IsSignedIntegralType(this SpecialType specialType)
	{
		switch (specialType)
		{
		case SpecialType.System_SByte:
		case SpecialType.System_Int16:
		case SpecialType.System_Int32:
		case SpecialType.System_Int64:
			return true;
		default:
			return false;
		}
	}

	public static bool CanBeConst(this SpecialType specialType)
	{
		if ((uint)(specialType - 7) <= 13u)
		{
			return true;
		}
		return false;
	}

	public static bool IsIntrinsicType(this SpecialType specialType)
	{
		if ((uint)(specialType - 7) <= 12u)
		{
			return true;
		}
		return false;
	}

	public static bool IsValidVolatileFieldType(this SpecialType specialType)
	{
		switch (specialType)
		{
		case SpecialType.System_Boolean:
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Single:
		case SpecialType.System_IntPtr:
		case SpecialType.System_UIntPtr:
			return true;
		default:
			return false;
		}
	}

	public static int FixedBufferElementSizeInBytes(this SpecialType specialType)
	{
		if (specialType != SpecialType.System_Decimal)
		{
			return specialType.SizeInBytes();
		}
		return 0;
	}
}
