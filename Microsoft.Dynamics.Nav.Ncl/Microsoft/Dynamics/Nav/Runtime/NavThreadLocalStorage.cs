using System;
using System.Threading;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavThreadLocalStorage : TypesThreadLocalStorage
{
	[ThreadStatic]
	private static NavThreadLocalStorage instance;

	public new static NavThreadLocalStorage Current
	{
		get
		{
			if (instance == null)
			{
				instance = new NavThreadLocalStorage();
				TypesThreadLocalStorage.typesInstance = instance;
				SynchronizationContext.SetSynchronizationContext(NavSynchronizationContext.Instance);
			}
			return instance;
		}
	}

	public IReference<NavSession> Session { get; set; }

	public string FormPersonalizationId { get; set; }

	public string DrillDownPersonalizationId { get; set; }

	public NavAppGroup OverriddenAppGroup { get; set; }

	public bool InitializingAppGroup { get; set; }

	public NavSqlConnectionScope CurrentSqlConnectionScope { get; set; }

	public ExclusiveTenantActionElevatedScope CurrentExclusiveTenantActionElevatedScope { get; set; }

	private NavThreadLocalStorage()
	{
	}

	public static void Clear()
	{
		instance = null;
		TypesThreadLocalStorage.typesInstance = null;
	}

	public static void Restore(NavThreadLocalStorage originalInstance)
	{
		instance = originalInstance;
		TypesThreadLocalStorage.typesInstance = originalInstance;
		SynchronizationContext.SetSynchronizationContext(NavSynchronizationContext.Instance);
	}
}
