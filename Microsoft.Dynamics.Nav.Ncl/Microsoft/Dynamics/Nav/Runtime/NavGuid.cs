using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavGuid : NavValue, IEquatable<NavGuid>
{
	private class NavGuidNull : NavGuid
	{
		public override bool IsNull => true;

		internal NavGuidNull()
			: base(Guid.Empty)
		{
		}
	}

	private struct GuidInternals
	{
		private const int SizeOfGuid = 16;

		private static readonly int[] guidCompareOrder = new int[16]
		{
			10, 11, 12, 13, 14, 15, 8, 9, 6, 7,
			4, 5, 0, 1, 2, 3
		};

		private unsafe fixed byte bytes[16];

		public unsafe static int SqlCompare(Guid x, Guid y)
		{
			GuidInternals* ptr = (GuidInternals*)(&x);
			GuidInternals* ptr2 = (GuidInternals*)(&y);
			for (int i = 0; i < 16; i++)
			{
				byte b = ptr->bytes[guidCompareOrder[i]];
				byte b2 = ptr2->bytes[guidCompareOrder[i]];
				if (b != b2)
				{
					if (b >= b2)
					{
						return 1;
					}
					return -1;
				}
			}
			return 0;
		}
	}

	private static readonly NavGuid defaultValue = new NavGuid(Guid.Empty);

	private static readonly NavGuid nullValue = new NavGuidNull();

	private readonly Guid value;

	public static NavGuid Default => defaultValue;

	public static NavGuid Null => nullValue;

	public Guid Value => value;

	public override NavNclType NclType => NavNclType.NavGuid;

	public override object ValueAsObject => value;

	public override int GetBytesSize => 16;

	public override bool IsZeroOrEmpty => value == Guid.Empty;

	public NavGuid(Guid initValue)
	{
		value = initValue;
	}

	public NavGuid(string initValue)
	{
		try
		{
			value = new Guid(initValue);
		}
		catch (FormatException)
		{
			throw new NavNCLInvalidGuidFormatException();
		}
		catch (OverflowException)
		{
			throw new NavNCLInvalidGuidFormatException();
		}
	}

	public NavGuid(byte[] initValue)
	{
		try
		{
			value = new Guid(initValue);
		}
		catch (ArgumentException)
		{
			throw new NavNCLInvalidGuidFormatException();
		}
	}

	public static NavGuid CreateFromBytes(byte[] data, int startIndex, int length)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (length != 16)
		{
			throw NavNCLTypeException.CreateWrongLengthException(NavType.GUID);
		}
		if (data.Length < startIndex + length || startIndex < 0)
		{
			throw NavNCLTypeException.CreateSourceHasWrongLengthException(NavType.GUID);
		}
		byte[] array = new byte[16];
		Array.Copy(data, startIndex, array, 0, 16);
		return new NavGuid(array);
	}

	public static NavGuid CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is string initValue)
		{
			return new NavGuid(initValue);
		}
		NavStringValue navStringValue = value as NavStringValue;
		if (navStringValue != null)
		{
			return new NavGuid(navStringValue);
		}
		if (value is Guid)
		{
			return new NavGuid((Guid)value);
		}
		if (value is NavDotNet navDotNet && navDotNet.Value is Guid)
		{
			return new NavGuid((Guid)navDotNet.Value);
		}
		if (value is byte[] initValue2)
		{
			return new NavGuid(initValue2);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavGuid), value);
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override byte[] GetBytes()
	{
		return value.ToByteArray();
	}

	public override Guid ToGuid()
	{
		return Value;
	}

	public bool Equals(NavGuid other)
	{
		if (other == null)
		{
			return false;
		}
		return value.Equals(other.value);
	}

	public override bool Equals(NavValue other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (other.NavType == NavType.GUID)
		{
			return Equals((NavGuid)other);
		}
		return ThrowUnableToCompareException(other);
	}

	public override bool Equals(object obj)
	{
		if (obj is NavValue other)
		{
			return Equals(other);
		}
		if (obj is Guid)
		{
			return Value == (Guid)obj;
		}
		return ThrowUnableToCompareException(obj);
	}

	public override int CompareTo(NavValue other)
	{
		if (other.NclType == NavNclType.NavGuid)
		{
			return GuidInternals.SqlCompare(value, other.ToGuid());
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
