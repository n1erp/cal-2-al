using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavNotificationScope : NavEnumBase<NotificationScope>
{
	public static NavNotificationScope Default => new NavNotificationScope(NotificationScope.LocalScope);

	public override NavNclType NclType => NavNclType.NavNotificationScope;

	public NavNotificationScope(NotificationScope value)
		: base(value)
	{
	}

	public NavNotificationScope(int notificationScopeValue)
		: base((NotificationScope)notificationScopeValue)
	{
	}

	public override bool Equals(object obj)
	{
		if (obj is int || obj is NotificationScope)
		{
			return Value.Equals(obj);
		}
		if (obj is NavNotificationScope navNotificationScope)
		{
			return Value.Equals(navNotificationScope.Value);
		}
		return false;
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null && other.NavType == NavType.NotificationScope)
		{
			return Value.CompareTo(((NavNotificationScope)other).Value);
		}
		ThrowUnableToCompareException(other);
		return 0;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}
}
