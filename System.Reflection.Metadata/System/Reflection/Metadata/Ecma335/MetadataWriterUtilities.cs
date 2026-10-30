using System.Collections.Immutable;

namespace System.Reflection.Metadata.Ecma335;

internal static class MetadataWriterUtilities
{
	public static SignatureTypeCode GetConstantTypeCode(object value)
	{
		if (value == null)
		{
			return (SignatureTypeCode)18;
		}
		if ((object)value.GetType() == typeof(int))
		{
			return SignatureTypeCode.Int32;
		}
		if ((object)value.GetType() == typeof(string))
		{
			return SignatureTypeCode.String;
		}
		if ((object)value.GetType() == typeof(bool))
		{
			return SignatureTypeCode.Boolean;
		}
		if ((object)value.GetType() == typeof(char))
		{
			return SignatureTypeCode.Char;
		}
		if ((object)value.GetType() == typeof(byte))
		{
			return SignatureTypeCode.Byte;
		}
		if ((object)value.GetType() == typeof(long))
		{
			return SignatureTypeCode.Int64;
		}
		if ((object)value.GetType() == typeof(double))
		{
			return SignatureTypeCode.Double;
		}
		if ((object)value.GetType() == typeof(short))
		{
			return SignatureTypeCode.Int16;
		}
		if ((object)value.GetType() == typeof(ushort))
		{
			return SignatureTypeCode.UInt16;
		}
		if ((object)value.GetType() == typeof(uint))
		{
			return SignatureTypeCode.UInt32;
		}
		if ((object)value.GetType() == typeof(sbyte))
		{
			return SignatureTypeCode.SByte;
		}
		if ((object)value.GetType() == typeof(ulong))
		{
			return SignatureTypeCode.UInt64;
		}
		if ((object)value.GetType() == typeof(float))
		{
			return SignatureTypeCode.Single;
		}
		throw new ArgumentException(SR.Format(SR.InvalidConstantValueOfType, value.GetType()), "value");
	}

	internal static void SerializeRowCounts(BlobBuilder writer, ImmutableArray<int> rowCounts)
	{
		for (int i = 0; i < rowCounts.Length; i++)
		{
			int num = rowCounts[i];
			if (num > 0)
			{
				writer.WriteInt32(num);
			}
		}
	}
}
