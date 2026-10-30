using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumReportFormat : NavEnumBase<NavReportFormat>
{
	private static readonly NavEnumReportFormat DefaultValue = new NavEnumReportFormat(NavReportFormat.None);

	public override NavNclType NclType => NavNclType.NavReportFormat;

	public static NavEnumReportFormat Default => DefaultValue;

	public NavEnumReportFormat(NavReportFormat initValue)
		: base(initValue)
	{
	}

	public NavEnumReportFormat(int initValue)
		: base((NavReportFormat)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.ObjectType)
			{
				return Value.CompareTo(((NavEnumReportFormat)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
