using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class SyntaxKindExtensions
{
	internal static SpecialType GetSpecialType(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.VoidKeyword => SpecialType.System_Void, 
			SyntaxKind.BoolKeyword => SpecialType.System_Boolean, 
			SyntaxKind.ByteKeyword => SpecialType.System_Byte, 
			SyntaxKind.SByteKeyword => SpecialType.System_SByte, 
			SyntaxKind.ShortKeyword => SpecialType.System_Int16, 
			SyntaxKind.UShortKeyword => SpecialType.System_UInt16, 
			SyntaxKind.IntKeyword => SpecialType.System_Int32, 
			SyntaxKind.UIntKeyword => SpecialType.System_UInt32, 
			SyntaxKind.LongKeyword => SpecialType.System_Int64, 
			SyntaxKind.ULongKeyword => SpecialType.System_UInt64, 
			SyntaxKind.DoubleKeyword => SpecialType.System_Double, 
			SyntaxKind.FloatKeyword => SpecialType.System_Single, 
			SyntaxKind.DecimalKeyword => SpecialType.System_Decimal, 
			SyntaxKind.StringKeyword => SpecialType.System_String, 
			SyntaxKind.CharKeyword => SpecialType.System_Char, 
			SyntaxKind.ObjectKeyword => SpecialType.System_Object, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}
}
