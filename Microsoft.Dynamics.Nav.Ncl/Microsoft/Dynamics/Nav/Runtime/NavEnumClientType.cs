using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumClientType : NavEnumBase<NavClientType>
{
	private static NavEnumClientType defaultValue = new NavEnumClientType(NavClientType.Current);

	public override NavNclType NclType => NavNclType.NavClientType;

	public static NavEnumClientType Default => defaultValue;

	public NavEnumClientType(NavClientType initValue)
		: base(initValue)
	{
	}

	public NavEnumClientType(byte[] value)
		: base((NavClientType)BitConverter.ToInt32(value, 0))
	{
	}

	public NavEnumClientType(int initValue)
		: base((NavClientType)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.ClientType)
			{
				return Value.CompareTo(((NavEnumClientType)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
