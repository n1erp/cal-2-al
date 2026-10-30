using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class ExclusiveTenantActionElevatedScope : IDisposable
{
	private ExclusiveTenantActionScope currentScope;

	public static bool HasElevatedAccess
	{
		get
		{
			if (NavThreadLocalStorage.Current.CurrentExclusiveTenantActionElevatedScope != null)
			{
				return NavThreadLocalStorage.Current.CurrentExclusiveTenantActionElevatedScope.currentScope.IsActive;
			}
			return false;
		}
	}

	internal ExclusiveTenantActionElevatedScope(ExclusiveTenantActionScope scope)
	{
		if (scope == null)
		{
			throw new ArgumentNullException("scope");
		}
		if (scope.IsActive)
		{
			currentScope = scope;
			NavThreadLocalStorage.Current.CurrentExclusiveTenantActionElevatedScope = this;
		}
	}

	public void Dispose()
	{
		if (currentScope != null)
		{
			currentScope = null;
			NavThreadLocalStorage.Current.CurrentExclusiveTenantActionElevatedScope = null;
		}
	}
}
