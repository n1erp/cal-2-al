using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTableConnectionType : NavEnumBase<TableConnectionType>
{
	private static NavTableConnectionType defaultValue = new NavTableConnectionType(TableConnectionType.CRM);

	public override NavNclType NclType => NavNclType.NavTableConnectionType;

	public static NavTableConnectionType Default => defaultValue;

	public NavTableConnectionType(TableConnectionType initValue)
		: base(initValue)
	{
	}

	public NavTableConnectionType(int initValue)
		: base((TableConnectionType)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.TableConnectionType)
			{
				return Value.CompareTo(((NavTableConnectionType)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
