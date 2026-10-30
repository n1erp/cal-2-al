using System.Collections.Generic;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis;

internal static class SpecialTypes
{
	private static readonly string[] s_emittedNames;

	private static readonly Dictionary<string, SpecialType> s_nameToTypeIdMap;

	private static readonly PrimitiveTypeCode[] s_typeIdToTypeCodeMap;

	private static readonly SpecialType[] s_typeCodeToTypeIdMap;

	static SpecialTypes()
	{
		s_emittedNames = new string[44]
		{
			null, "System.Object", "System.Enum", "System.MulticastDelegate", "System.Delegate", "System.ValueType", "System.Void", "System.Boolean", "System.Char", "System.SByte",
			"System.Byte", "System.Int16", "System.UInt16", "System.Int32", "System.UInt32", "System.Int64", "System.UInt64", "System.Decimal", "System.Single", "System.Double",
			"System.String", "System.IntPtr", "System.UIntPtr", "System.Array", "System.Collections.IEnumerable", "System.Collections.Generic.IEnumerable`1", "System.Collections.Generic.IList`1", "System.Collections.Generic.ICollection`1", "System.Collections.IEnumerator", "System.Collections.Generic.IEnumerator`1",
			"System.Collections.Generic.IReadOnlyList`1", "System.Collections.Generic.IReadOnlyCollection`1", "System.Nullable`1", "System.DateTime", "System.Runtime.CompilerServices.IsVolatile", "System.IDisposable", "System.TypedReference", "System.ArgIterator", "System.RuntimeArgumentHandle", "System.RuntimeFieldHandle",
			"System.RuntimeMethodHandle", "System.RuntimeTypeHandle", "System.IAsyncResult", "System.AsyncCallback"
		};
		s_nameToTypeIdMap = new Dictionary<string, SpecialType>(43);
		for (int i = 1; i < s_emittedNames.Length; i++)
		{
			s_nameToTypeIdMap.Add(s_emittedNames[i], (SpecialType)i);
		}
		s_typeIdToTypeCodeMap = new PrimitiveTypeCode[44];
		for (int i = 0; i < s_typeIdToTypeCodeMap.Length; i++)
		{
			s_typeIdToTypeCodeMap[i] = PrimitiveTypeCode.NotPrimitive;
		}
		s_typeIdToTypeCodeMap[7] = PrimitiveTypeCode.Boolean;
		s_typeIdToTypeCodeMap[8] = PrimitiveTypeCode.Char;
		s_typeIdToTypeCodeMap[6] = PrimitiveTypeCode.Void;
		s_typeIdToTypeCodeMap[20] = PrimitiveTypeCode.String;
		s_typeIdToTypeCodeMap[15] = PrimitiveTypeCode.Int64;
		s_typeIdToTypeCodeMap[13] = PrimitiveTypeCode.Int32;
		s_typeIdToTypeCodeMap[11] = PrimitiveTypeCode.Int16;
		s_typeIdToTypeCodeMap[9] = PrimitiveTypeCode.Int8;
		s_typeIdToTypeCodeMap[16] = PrimitiveTypeCode.UInt64;
		s_typeIdToTypeCodeMap[14] = PrimitiveTypeCode.UInt32;
		s_typeIdToTypeCodeMap[12] = PrimitiveTypeCode.UInt16;
		s_typeIdToTypeCodeMap[10] = PrimitiveTypeCode.UInt8;
		s_typeIdToTypeCodeMap[18] = PrimitiveTypeCode.Float32;
		s_typeIdToTypeCodeMap[19] = PrimitiveTypeCode.Float64;
		s_typeIdToTypeCodeMap[21] = PrimitiveTypeCode.IntPtr;
		s_typeIdToTypeCodeMap[22] = PrimitiveTypeCode.UIntPtr;
		s_typeCodeToTypeIdMap = new SpecialType[20];
		for (int i = 0; i < s_typeCodeToTypeIdMap.Length; i++)
		{
			s_typeCodeToTypeIdMap[i] = SpecialType.None;
		}
		s_typeCodeToTypeIdMap[0] = SpecialType.System_Boolean;
		s_typeCodeToTypeIdMap[1] = SpecialType.System_Char;
		s_typeCodeToTypeIdMap[17] = SpecialType.System_Void;
		s_typeCodeToTypeIdMap[11] = SpecialType.System_String;
		s_typeCodeToTypeIdMap[7] = SpecialType.System_Int64;
		s_typeCodeToTypeIdMap[6] = SpecialType.System_Int32;
		s_typeCodeToTypeIdMap[5] = SpecialType.System_Int16;
		s_typeCodeToTypeIdMap[2] = SpecialType.System_SByte;
		s_typeCodeToTypeIdMap[15] = SpecialType.System_UInt64;
		s_typeCodeToTypeIdMap[14] = SpecialType.System_UInt32;
		s_typeCodeToTypeIdMap[13] = SpecialType.System_UInt16;
		s_typeCodeToTypeIdMap[12] = SpecialType.System_Byte;
		s_typeCodeToTypeIdMap[3] = SpecialType.System_Single;
		s_typeCodeToTypeIdMap[4] = SpecialType.System_Double;
		s_typeCodeToTypeIdMap[8] = SpecialType.System_IntPtr;
		s_typeCodeToTypeIdMap[16] = SpecialType.System_UIntPtr;
	}

	public static string GetMetadataName(this SpecialType id)
	{
		return s_emittedNames[(int)id];
	}

	public static SpecialType GetTypeFromMetadataName(string metadataName)
	{
		if (s_nameToTypeIdMap.TryGetValue(metadataName, out var value))
		{
			return value;
		}
		return SpecialType.None;
	}

	public static SpecialType GetTypeFromMetadataName(PrimitiveTypeCode typeCode)
	{
		return s_typeCodeToTypeIdMap[(int)typeCode];
	}

	public static PrimitiveTypeCode GetTypeCode(SpecialType typeId)
	{
		return s_typeIdToTypeCodeMap[(int)typeId];
	}
}
