using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavEnumBase<T> : NavValue where T : struct
{
	private readonly T value;

	public T Value => value;

	public override int GetBytesSize
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override bool IsZeroOrEmpty => GetHashCode() == 0;

	internal NavEnumBase(T initValue)
	{
		value = initValue;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override string ToString()
	{
		return EnumHelper<T>.EnumToString(value);
	}

	public override int ToInt32()
	{
		if (!((object)value is IConvertible convertible))
		{
			throw new NavNCLConversionException(GetType(), typeof(int));
		}
		return convertible.ToInt32(CultureInfo.CurrentCulture);
	}

	public override bool Equals(object obj)
	{
		if (obj is int || obj is T)
		{
			return Value.Equals(obj);
		}
		if (obj is NavEnumBase<T> navEnumBase)
		{
			return Value.Equals(navEnumBase.Value);
		}
		return false;
	}
}
