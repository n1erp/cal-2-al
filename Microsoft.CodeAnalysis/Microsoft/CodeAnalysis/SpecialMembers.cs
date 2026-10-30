using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.RuntimeMembers;

namespace Microsoft.CodeAnalysis;

internal static class SpecialMembers
{
	private static readonly ImmutableArray<MemberDescriptor> s_descriptors;

	static SpecialMembers()
	{
		byte[] buffer = new byte[986]
		{
			4, 20, 0, 1, 64, 6, 29, 64, 8, 33,
			20, 0, 2, 64, 20, 64, 20, 64, 20, 33,
			20, 0, 3, 64, 20, 64, 20, 64, 20, 64,
			20, 33, 20, 0, 4, 64, 20, 64, 20, 64,
			20, 64, 20, 64, 20, 33, 20, 0, 1, 64,
			20, 29, 64, 20, 33, 20, 0, 1, 64, 20,
			64, 1, 33, 20, 0, 2, 64, 20, 64, 1,
			64, 1, 33, 20, 0, 3, 64, 20, 64, 1,
			64, 1, 64, 1, 33, 20, 0, 1, 64, 20,
			29, 64, 1, 33, 20, 0, 2, 64, 7, 64,
			20, 64, 20, 33, 20, 0, 2, 64, 7, 64,
			20, 64, 20, 8, 20, 0, 0, 64, 13, 8,
			20, 0, 1, 64, 8, 64, 13, 33, 20, 0,
			2, 64, 20, 64, 20, 29, 64, 1, 33, 4,
			0, 2, 64, 4, 64, 4, 64, 4, 33, 4,
			0, 2, 64, 4, 64, 4, 64, 4, 33, 4,
			0, 2, 64, 7, 64, 4, 64, 4, 33, 4,
			0, 2, 64, 7, 64, 4, 64, 4, 34, 17,
			0, 64, 17, 34, 17, 0, 64, 17, 34, 17,
			0, 64, 17, 4, 17, 0, 1, 64, 6, 64,
			13, 4, 17, 0, 1, 64, 6, 64, 14, 4,
			17, 0, 1, 64, 6, 64, 15, 4, 17, 0,
			1, 64, 6, 64, 16, 4, 17, 0, 1, 64,
			6, 64, 18, 4, 17, 0, 1, 64, 6, 64,
			19, 4, 17, 0, 5, 64, 6, 64, 13, 64,
			13, 64, 13, 64, 7, 64, 10, 33, 17, 0,
			2, 64, 17, 64, 17, 64, 17, 33, 17, 0,
			2, 64, 17, 64, 17, 64, 17, 33, 17, 0,
			2, 64, 17, 64, 17, 64, 17, 33, 17, 0,
			2, 64, 17, 64, 17, 64, 17, 33, 17, 0,
			2, 64, 17, 64, 17, 64, 17, 33, 17, 0,
			1, 64, 17, 64, 17, 33, 17, 0, 1, 64,
			17, 64, 17, 33, 17, 0, 1, 64, 17, 64,
			17, 33, 17, 0, 1, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 17, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 17, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 17, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 17, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 17, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 17, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 13, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 7, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 7, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 7, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 7, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 7, 64, 17, 64, 17, 33,
			17, 0, 2, 64, 7, 64, 17, 64, 17, 33,
			17, 0, 1, 64, 17, 64, 10, 33, 17, 0,
			1, 64, 17, 64, 8, 33, 17, 0, 1, 64,
			17, 64, 11, 33, 17, 0, 1, 64, 17, 64,
			13, 33, 17, 0, 1, 64, 17, 64, 15, 33,
			17, 0, 1, 64, 17, 64, 9, 33, 17, 0,
			1, 64, 17, 64, 12, 33, 17, 0, 1, 64,
			17, 64, 14, 33, 17, 0, 1, 64, 17, 64,
			16, 33, 17, 0, 1, 64, 10, 64, 17, 33,
			17, 0, 1, 64, 12, 64, 17, 33, 17, 0,
			1, 64, 9, 64, 17, 33, 17, 0, 1, 64,
			11, 64, 17, 33, 17, 0, 1, 64, 18, 64,
			17, 33, 17, 0, 1, 64, 19, 64, 17, 33,
			17, 0, 1, 64, 8, 64, 17, 33, 17, 0,
			1, 64, 16, 64, 17, 33, 17, 0, 1, 64,
			13, 64, 17, 33, 17, 0, 1, 64, 14, 64,
			17, 33, 17, 0, 1, 64, 15, 64, 17, 33,
			17, 0, 1, 64, 17, 64, 19, 33, 17, 0,
			1, 64, 17, 64, 18, 34, 33, 0, 64, 33,
			4, 33, 0, 1, 64, 6, 64, 15, 33, 33,
			0, 2, 64, 13, 64, 33, 64, 33, 33, 33,
			0, 2, 64, 7, 64, 33, 64, 33, 33, 33,
			0, 2, 64, 7, 64, 33, 64, 33, 33, 33,
			0, 2, 64, 7, 64, 33, 64, 33, 33, 33,
			0, 2, 64, 7, 64, 33, 64, 33, 33, 33,
			0, 2, 64, 7, 64, 33, 64, 33, 33, 33,
			0, 2, 64, 7, 64, 33, 64, 33, 65, 24,
			0, 0, 64, 28, 80, 28, 0, 0, 64, 1,
			72, 28, 0, 0, 64, 1, 65, 28, 0, 0,
			64, 7, 65, 28, 0, 0, 64, 6, 65, 25,
			0, 0, 21, 64, 29, 1, 19, 0, 80, 29,
			0, 0, 19, 0, 72, 29, 0, 0, 19, 0,
			65, 35, 0, 0, 64, 6, 16, 23, 0, 0,
			64, 13, 16, 23, 0, 0, 64, 15, 1, 23,
			0, 1, 64, 13, 64, 13, 1, 23, 0, 1,
			64, 13, 64, 13, 65, 1, 0, 0, 64, 13,
			65, 1, 0, 1, 64, 7, 64, 1, 65, 1,
			0, 0, 64, 20, 33, 1, 0, 2, 64, 7,
			64, 1, 64, 1, 33, 21, 0, 1, 15, 64,
			6, 64, 21, 33, 21, 0, 1, 64, 13, 64,
			21, 33, 21, 0, 1, 64, 15, 64, 21, 33,
			21, 0, 1, 64, 21, 15, 64, 6, 33, 21,
			0, 1, 64, 21, 64, 13, 33, 21, 0, 1,
			64, 21, 64, 15, 33, 22, 0, 1, 15, 64,
			6, 64, 22, 33, 22, 0, 1, 64, 14, 64,
			22, 33, 22, 0, 1, 64, 16, 64, 22, 33,
			22, 0, 1, 64, 22, 15, 64, 6, 33, 22,
			0, 1, 64, 22, 64, 14, 33, 22, 0, 1,
			64, 22, 64, 16, 1, 32, 0, 0, 19, 0,
			8, 32, 0, 0, 19, 0, 8, 32, 0, 0,
			64, 7, 4, 32, 0, 1, 64, 6, 19, 0,
			33, 32, 0, 1, 64, 32, 19, 0, 33, 32,
			0, 1, 19, 0, 64, 32
		};
		string[] nameTable = new string[116]
		{
			".ctor", "Concat", "Concat", "Concat", "Concat", "Concat", "Concat", "Concat", "Concat", "op_Equality",
			"op_Inequality", "get_Length", "get_Chars", "Format", "Combine", "Remove", "op_Equality", "op_Inequality", "Zero", "MinusOne",
			"One", ".ctor", ".ctor", ".ctor", ".ctor", ".ctor", ".ctor", ".ctor", "op_Addition", "op_Subtraction",
			"op_Multiply", "op_Division", "op_Modulus", "op_UnaryNegation", "op_Increment", "op_Decrement", "Negate", "Remainder", "Add", "Subtract",
			"Multiply", "Divide", "Remainder", "Compare", "op_Equality", "op_Inequality", "op_GreaterThan", "op_GreaterThanOrEqual", "op_LessThan", "op_LessThanOrEqual",
			"op_Implicit", "op_Implicit", "op_Implicit", "op_Implicit", "op_Implicit", "op_Implicit", "op_Implicit", "op_Implicit", "op_Implicit", "op_Explicit",
			"op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit",
			"op_Explicit", "op_Explicit", "MinValue", ".ctor", "Compare", "op_Equality", "op_Inequality", "op_GreaterThan", "op_GreaterThanOrEqual", "op_LessThan",
			"op_LessThanOrEqual", "GetEnumerator", "Current", "get_Current", "MoveNext", "Reset", "GetEnumerator", "Current", "get_Current", "Dispose",
			"Length", "LongLength", "GetLowerBound", "GetUpperBound", "GetHashCode", "Equals", "ToString", "ReferenceEquals", "op_Explicit", "op_Explicit",
			"op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit", "op_Explicit",
			"GetValueOrDefault", "get_Value", "get_HasValue", ".ctor", "op_Implicit", "op_Explicit"
		};
		s_descriptors = MemberDescriptor.InitializeFromStream(new MemoryStream(buffer, writable: false), nameTable);
	}

	public static MemberDescriptor GetDescriptor(SpecialMember member)
	{
		return s_descriptors[(int)member];
	}
}
