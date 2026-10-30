using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class SpecialTypeExtensions
{
	public static bool IsClrInteger(this SpecialType specialType)
	{
		if ((uint)(specialType - 7) <= 9u || (uint)(specialType - 21) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsBlittable(this SpecialType specialType)
	{
		if ((uint)(specialType - 7) <= 9u || (uint)(specialType - 18) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static int SizeInBytes(this SpecialType specialType)
	{
		return specialType switch
		{
			SpecialType.System_SByte => 1, 
			SpecialType.System_Byte => 1, 
			SpecialType.System_Int16 => 2, 
			SpecialType.System_UInt16 => 2, 
			SpecialType.System_Int32 => 4, 
			SpecialType.System_UInt32 => 4, 
			SpecialType.System_Int64 => 8, 
			SpecialType.System_UInt64 => 8, 
			SpecialType.System_Char => 2, 
			SpecialType.System_Single => 4, 
			SpecialType.System_Double => 8, 
			SpecialType.System_Boolean => 1, 
			SpecialType.System_Decimal => 16, 
			_ => 0, 
		};
	}

	public static bool IsPrimitiveRecursiveStruct(this SpecialType specialType)
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
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_Single:
		case SpecialType.System_Double:
		case SpecialType.System_IntPtr:
		case SpecialType.System_UIntPtr:
			return true;
		default:
			return false;
		}
	}

	public static bool IsValidEnumUnderlyingType(this SpecialType specialType)
	{
		if ((uint)(specialType - 9) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool IsNumericType(this SpecialType specialType)
	{
		if ((uint)(specialType - 9) <= 10u)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnsignedIntegralType(this SpecialType specialType)
	{
		switch (specialType)
		{
		case SpecialType.System_Byte:
		case SpecialType.System_UInt16:
		case SpecialType.System_UInt32:
		case SpecialType.System_UInt64:
			return true;
		default:
			return false;
		}
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

	public static int VBForToShiftBits(this SpecialType specialType)
	{
		return specialType switch
		{
			SpecialType.System_SByte => 7, 
			SpecialType.System_Int16 => 15, 
			SpecialType.System_Int32 => 31, 
			SpecialType.System_Int64 => 63, 
			_ => throw ExceptionUtilities.UnexpectedValue(specialType), 
		};
	}

	public static SpecialType FromRuntimeTypeOfLiteralValue(object value)
	{
		if ((object)value.GetType() == typeof(int))
		{
			return SpecialType.System_Int32;
		}
		if ((object)value.GetType() == typeof(string))
		{
			return SpecialType.System_String;
		}
		if ((object)value.GetType() == typeof(bool))
		{
			return SpecialType.System_Boolean;
		}
		if ((object)value.GetType() == typeof(char))
		{
			return SpecialType.System_Char;
		}
		if ((object)value.GetType() == typeof(long))
		{
			return SpecialType.System_Int64;
		}
		if ((object)value.GetType() == typeof(double))
		{
			return SpecialType.System_Double;
		}
		if ((object)value.GetType() == typeof(uint))
		{
			return SpecialType.System_UInt32;
		}
		if ((object)value.GetType() == typeof(ulong))
		{
			return SpecialType.System_UInt64;
		}
		if ((object)value.GetType() == typeof(float))
		{
			return SpecialType.System_Single;
		}
		if ((object)value.GetType() == typeof(decimal))
		{
			return SpecialType.System_Decimal;
		}
		if ((object)value.GetType() == typeof(short))
		{
			return SpecialType.System_Int16;
		}
		if ((object)value.GetType() == typeof(ushort))
		{
			return SpecialType.System_UInt16;
		}
		if ((object)value.GetType() == typeof(DateTime))
		{
			return SpecialType.System_DateTime;
		}
		if ((object)value.GetType() == typeof(byte))
		{
			return SpecialType.System_Byte;
		}
		if ((object)value.GetType() == typeof(sbyte))
		{
			return SpecialType.System_SByte;
		}
		return SpecialType.None;
	}
}
