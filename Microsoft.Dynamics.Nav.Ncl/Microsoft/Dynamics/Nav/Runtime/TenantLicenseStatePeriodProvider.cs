using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class TenantLicenseStatePeriodProvider
{
	private static readonly Dictionary<TenantLicenseState, int> periods = new Dictionary<TenantLicenseState, int>
	{
		{
			TenantLicenseState.Trial,
			30
		},
		{
			TenantLicenseState.Warning,
			30
		},
		{
			TenantLicenseState.Suspended,
			90
		}
	};

	public static int GetPeriod(TenantLicenseState state)
	{
		if (periods.TryGetValue(state, out var value))
		{
			return value;
		}
		return -1;
	}

	public static int ALGetPeriod(int state)
	{
		NavSession session = NavCurrentThread.Session;
		if (session == null || !session.IsOpen)
		{
			throw new InvalidOperationException("Current Nav session must exist and be open");
		}
		if (!Enum.IsDefined(typeof(TenantLicenseState), state))
		{
			throw new InvalidOperationException("Unknown tenant license state");
		}
		return GetPeriod((TenantLicenseState)state);
	}
}
