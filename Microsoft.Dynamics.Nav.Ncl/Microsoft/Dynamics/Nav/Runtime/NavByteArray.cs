using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavByteArray : NavValue
{
	private byte[] valueByteArray;

	public override NavNclType NclType => NavNclType.NavClrObject;

	public override int GetBytesSize
	{
		get
		{
			if (valueByteArray == null)
			{
				return 0;
			}
			return valueByteArray.Length;
		}
	}

	public override object ValueAsObject => valueByteArray;

	public override bool IsZeroOrEmpty
	{
		get
		{
			if (valueByteArray != null)
			{
				return valueByteArray.Length == 0;
			}
			return true;
		}
	}

	public byte[] Value => valueByteArray;

	public NavByteArray(byte[] value)
	{
		valueByteArray = value;
	}

	public NavByteArray(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length <= 0)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.ClrByteArray);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.ClrByteArray);
		}
		valueByteArray = new byte[length];
		Array.ConstrainedCopy(data, startIndex, valueByteArray, 0, length);
	}

	public override int GetHashCode()
	{
		if (valueByteArray == null)
		{
			return 0;
		}
		return valueByteArray.GetHashCode();
	}

	public override byte[] GetBytes()
	{
		return valueByteArray;
	}

	public static NavByteArray CreateFromBytes(byte[] data, int startIndex, int length)
	{
		return new NavByteArray(data, startIndex, length);
	}

	public static NavByteArray CreateFromObject(object value)
	{
		if (value is byte[] value2)
		{
			return new NavByteArray(value2);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavByteArray), value);
	}
}
