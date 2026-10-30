using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumDefaultLayout : NavEnumBase<NavDefaultLayout>
{
	private static readonly NavEnumDefaultLayout DefaultValue = new NavEnumDefaultLayout(NavDefaultLayout.RDLC);

	public override NavNclType NclType => NavNclType.NavDefaultLayout;

	public static NavEnumDefaultLayout Default => DefaultValue;

	public NavEnumDefaultLayout(NavDefaultLayout initValue)
		: base(initValue)
	{
	}

	public NavEnumDefaultLayout(int initValue)
		: base((NavDefaultLayout)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.DefaultLayout)
			{
				return Value.CompareTo(((NavEnumDefaultLayout)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
