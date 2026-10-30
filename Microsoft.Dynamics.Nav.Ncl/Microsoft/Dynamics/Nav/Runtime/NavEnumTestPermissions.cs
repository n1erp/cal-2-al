using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumTestPermissions : NavEnumBase<NavTestPermissions>
{
	private static NavEnumTestPermissions defaultValue = new NavEnumTestPermissions(NavTestPermissions.InheritFromTestCodeunit);

	public override NavNclType NclType => NavNclType.NavTestPermissions;

	public static NavEnumTestPermissions Default => defaultValue;

	public NavEnumTestPermissions(NavTestPermissions initValue)
		: base(initValue)
	{
	}

	public NavEnumTestPermissions(byte[] value)
		: base((NavTestPermissions)BitConverter.ToInt32(value, 0))
	{
	}

	public NavEnumTestPermissions(int initValue)
		: base((NavTestPermissions)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.TestPermissions)
			{
				return Value.CompareTo(((NavEnumTestPermissions)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
