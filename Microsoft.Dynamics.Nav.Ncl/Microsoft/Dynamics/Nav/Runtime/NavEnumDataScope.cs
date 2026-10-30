using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumDataScope : NavEnumBase<DataScope>
{
	private static readonly NavEnumDataScope DefaultValue = new NavEnumDataScope(DataScope.Module);

	public override NavNclType NclType => NavNclType.NavDataScope;

	public static NavEnumDataScope Default => DefaultValue;

	public NavEnumDataScope(DataScope initValue)
		: base(initValue)
	{
	}

	public NavEnumDataScope(int initValue)
		: base((DataScope)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.DataScope)
			{
				return Value.CompareTo(((NavEnumDataScope)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
