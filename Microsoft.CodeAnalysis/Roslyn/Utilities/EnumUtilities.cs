using System;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class EnumUtilities
{
	internal static ulong ConvertEnumUnderlyingTypeToUInt64(object value, SpecialType specialType)
	{
		return specialType switch
		{
			SpecialType.System_SByte => (ulong)(sbyte)value, 
			SpecialType.System_Int16 => (ulong)(short)value, 
			SpecialType.System_Int32 => (ulong)(int)value, 
			SpecialType.System_Int64 => (ulong)(long)value, 
			SpecialType.System_Byte => (byte)value, 
			SpecialType.System_UInt16 => (ushort)value, 
			SpecialType.System_UInt32 => (uint)value, 
			SpecialType.System_UInt64 => (ulong)value, 
			_ => throw new InvalidOperationException($"{specialType} is not a valid underlying type for an enum"), 
		};
	}

	internal static T[] GetValues<T>() where T : struct
	{
		return (T[])Enum.GetValues(typeof(T));
	}
}
