using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class OfficeEntitlementStatusCache
{
	private class ExpiringOfficeEntitlementStatusValue : ExpiringValue<OfficeEntitlementStatus>
	{
		internal ExpiringOfficeEntitlementStatusValue(OfficeEntitlementStatus value)
			: base(value, ((int)value == 1) ? DateTime.UtcNow.AddHours(1.0) : DateTime.UtcNow.AddMinutes(2.0))
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Invalid comparison between Unknown and I4
		}
	}

	private readonly ConcurrentDictionary<string, ExpiringOfficeEntitlementStatusValue> cache = new ConcurrentDictionary<string, ExpiringOfficeEntitlementStatusValue>();

	private long cleanupRunning;

	internal OfficeEntitlementStatus GetOfficeEntitlementStatus(string userPrincipalName, NavDiagnostics diagnostics, Func<string, OfficeEntitlementStatus> officeEntitlementStatusFunc)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetActiveValue(userPrincipalName, out var status))
		{
			return status;
		}
		diagnostics?.SendTraceTag("000011P", Category.Security, Verbosity.Normal, "Unable to find a matching cached OfficeEntitlementStatus for the upn {0}", DiagnosticParameter.Euii(userPrincipalName));
		UpdateCache(userPrincipalName, (OfficeEntitlementStatus)0);
		NavTaskFactory.RunTask(() =>
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			status = officeEntitlementStatusFunc(userPrincipalName);
			diagnostics?.SendTraceTag("000011Q", Category.Security, Verbosity.Normal, "Updating the cache with the OfficeEntitlementStatus for the upn {0}", DiagnosticParameter.Euii(userPrincipalName));
			UpdateCache(userPrincipalName, status);
		}).LogOrFailOnException();
		return status;
	}

	internal OfficeEntitlementStatus WaitOnStatus(string userPrincipalName, int millisecondsTimeout = 30000)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		OfficeEntitlementStatus status = (OfficeEntitlementStatus)0;
		Func<bool> condition = () =>
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Invalid comparison between Unknown and I4
			Thread.Sleep(0);
			return TryGetActiveValue(userPrincipalName, out status) && (int)status > 0;
		};
		if (SpinWait.SpinUntil(condition, millisecondsTimeout))
		{
			return status;
		}
		return (OfficeEntitlementStatus)4;
	}

	private bool TryGetActiveValue(string key, out OfficeEntitlementStatus status)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected I4, but got Unknown
		status = (OfficeEntitlementStatus)0;
		if (!string.IsNullOrEmpty(key) && cache.TryGetValue(key, out var value) && !value.Expired)
		{
			status = (OfficeEntitlementStatus)(int)value.Value;
			return true;
		}
		return false;
	}

	private void UpdateCache(string key, OfficeEntitlementStatus status)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		RemoveExpiredValues();
		if ((int)status == 4)
		{
			return;
		}
		ExpiringOfficeEntitlementStatusValue expiringValue = new ExpiringOfficeEntitlementStatusValue(status);
		if (!expiringValue.Expired)
		{
			cache.AddOrUpdate(key, expiringValue, (string k, ExpiringOfficeEntitlementStatusValue v) => expiringValue);
		}
	}

	private void RemoveExpiredValues()
	{
		if (Interlocked.CompareExchange(ref cleanupRunning, 1L, 0L) != 0L)
		{
			return;
		}
		try
		{
			List<string> list = new List<string>();
			foreach (string item in cache.Keys.ToList())
			{
				if (cache.TryGetValue(item, out var value) && value.Expired)
				{
					cache.TryRemove(item, out value);
					list.Add(item);
				}
			}
		}
		finally
		{
			Interlocked.CompareExchange(ref cleanupRunning, 0L, 1L);
		}
	}
}
