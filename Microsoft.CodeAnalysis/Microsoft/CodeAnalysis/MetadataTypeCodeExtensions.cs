using System.Reflection.Metadata;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class MetadataTypeCodeExtensions
{
	internal static SpecialType ToSpecialType(this SignatureTypeCode typeCode)
	{
		return typeCode switch
		{
			SignatureTypeCode.TypedReference => SpecialType.System_TypedReference, 
			SignatureTypeCode.Void => SpecialType.System_Void, 
			SignatureTypeCode.Boolean => SpecialType.System_Boolean, 
			SignatureTypeCode.SByte => SpecialType.System_SByte, 
			SignatureTypeCode.Byte => SpecialType.System_Byte, 
			SignatureTypeCode.Int16 => SpecialType.System_Int16, 
			SignatureTypeCode.UInt16 => SpecialType.System_UInt16, 
			SignatureTypeCode.Int32 => SpecialType.System_Int32, 
			SignatureTypeCode.UInt32 => SpecialType.System_UInt32, 
			SignatureTypeCode.Int64 => SpecialType.System_Int64, 
			SignatureTypeCode.UInt64 => SpecialType.System_UInt64, 
			SignatureTypeCode.Single => SpecialType.System_Single, 
			SignatureTypeCode.Double => SpecialType.System_Double, 
			SignatureTypeCode.Char => SpecialType.System_Char, 
			SignatureTypeCode.String => SpecialType.System_String, 
			SignatureTypeCode.IntPtr => SpecialType.System_IntPtr, 
			SignatureTypeCode.UIntPtr => SpecialType.System_UIntPtr, 
			SignatureTypeCode.Object => SpecialType.System_Object, 
			_ => throw ExceptionUtilities.UnexpectedValue(typeCode), 
		};
	}

	internal static bool HasShortFormSignatureEncoding(this SpecialType type)
	{
		switch (type)
		{
		case SpecialType.System_Object:
		case SpecialType.System_Void:
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
		case SpecialType.System_String:
		case SpecialType.System_IntPtr:
		case SpecialType.System_UIntPtr:
		case SpecialType.System_TypedReference:
			return true;
		default:
			return false;
		}
	}

	internal static SerializationTypeCode ToSerializationType(this SpecialType specialType)
	{
		return specialType switch
		{
			SpecialType.System_Boolean => SerializationTypeCode.Boolean, 
			SpecialType.System_SByte => SerializationTypeCode.SByte, 
			SpecialType.System_Byte => SerializationTypeCode.Byte, 
			SpecialType.System_Int16 => SerializationTypeCode.Int16, 
			SpecialType.System_Int32 => SerializationTypeCode.Int32, 
			SpecialType.System_Int64 => SerializationTypeCode.Int64, 
			SpecialType.System_UInt16 => SerializationTypeCode.UInt16, 
			SpecialType.System_UInt32 => SerializationTypeCode.UInt32, 
			SpecialType.System_UInt64 => SerializationTypeCode.UInt64, 
			SpecialType.System_Single => SerializationTypeCode.Single, 
			SpecialType.System_Double => SerializationTypeCode.Double, 
			SpecialType.System_Char => SerializationTypeCode.Char, 
			SpecialType.System_String => SerializationTypeCode.String, 
			SpecialType.System_Object => SerializationTypeCode.TaggedObject, 
			_ => throw ExceptionUtilities.UnexpectedValue(specialType), 
		};
	}
}
