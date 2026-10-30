using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumNotificationScopeEnumEvaluator : NavEnumTypeEvaluator<NotificationScope, NavNotificationScope>
{
	internal static readonly NavEnumNotificationScopeEnumEvaluator Instance = new NavEnumNotificationScopeEnumEvaluator();

	private NavEnumNotificationScopeEnumEvaluator()
		: base((Func<NotificationScope, NavNotificationScope>)((NotificationScope scope) => new NavNotificationScope(scope)))
	{
	}

	protected override NavNotificationScope GetDefaultValue(INavValueMetadata metadata)
	{
		return NavNotificationScope.Default;
	}
}
