using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumObjectType : NavEnumBase<NavObjectType>
{
	private static readonly NavEnumObjectType DefaultValue = new NavEnumObjectType(NavObjectType.TableData);

	public override NavNclType NclType => NavNclType.NavObjectType;

	public static NavEnumObjectType Default => DefaultValue;

	public NavEnumObjectType(NavObjectType initValue)
		: base(initValue)
	{
	}

	public NavEnumObjectType(byte[] value)
		: base((NavObjectType)BitConverter.ToInt32(value, 0))
	{
	}

	public NavEnumObjectType(int initValue)
		: base((NavObjectType)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.ObjectType)
			{
				return Value.CompareTo(((NavEnumObjectType)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
