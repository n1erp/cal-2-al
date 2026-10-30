using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavPermissionTelemetryRegistrar
{
	private static readonly ConcurrentDictionary<int, Action<NavPermissionTelemetryEvent>> sessionListeners = new ConcurrentDictionary<int, Action<NavPermissionTelemetryEvent>>();

	internal static bool HasListeners => sessionListeners.Any();

	public static void RegisterListener(int sessionId, Action<NavPermissionTelemetryEvent> callback)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		if (sessionId != -1)
		{
			sessionListeners.TryAdd(sessionId, callback);
		}
	}

	public static void UnregisterListener(int sessionId)
	{
		if (sessionId != -1)
		{
			sessionListeners.TryRemove(sessionId, out var _);
		}
	}

	internal static void OnPermissionTelemetry(int sessionId, NavPermissionTelemetryEvent npte)
	{
		if (sessionListeners.TryGetValue(sessionId, out var value))
		{
			try
			{
				value(npte);
			}
			catch (Exception)
			{
				UnregisterListener(sessionId);
			}
		}
	}
}
