using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class DebugRuntimeManager
{
	private ConcurrentDictionary<string, DebugRuntime> connections = new ConcurrentDictionary<string, DebugRuntime>();

	internal event EventHandler<DebugSessionStartEventArgs> DebuggingSessionStarted;

	internal void CreateRuntime(string debuggingContext, NavSession session, bool waitForDebugAdapterConfiguration)
	{
		if (connections.ContainsKey(debuggingContext))
		{
			RemoveRuntime(debuggingContext);
		}
		DebugRuntime debugRuntime = DebugRuntimeFactory.CreateDebugRuntimeInstance(session, enableWaitForBreakpoints: true);
		if (!connections.TryAdd(debuggingContext, debugRuntime))
		{
			throw new FailedToStartDebuggingSessionException(Lang.DebuggingSessionAlreadyStarted);
		}
		OnDebuggingSessionStarted(new DebugSessionStartEventArgs(debuggingContext));
		if (waitForDebugAdapterConfiguration)
		{
			debugRuntime.WaitForDebugAdapterToSignalItsConfigurationDone();
		}
	}

	internal void AddDebuggerCallback(string debuggingContext, INavDebuggerCallback callback)
	{
		if (connections.TryGetValue(debuggingContext, out var value))
		{
			value.Environment.DebuggerCallback = callback;
		}
	}

	internal DebugRuntime GetDebugRuntime(string debuggingContext)
	{
		connections.TryGetValue(debuggingContext, out var value);
		return value;
	}

	internal void RemoveRuntime(string debuggingContext)
	{
		connections.TryRemove(debuggingContext, out var value);
		if (value != null)
		{
			if (value.Environment.DebuggerCallback is IDisposable disposable)
			{
				disposable.Dispose();
			}
			value.Dispose();
		}
	}

	internal void DetachSession(string debuggingContext, NavSession session)
	{
		GetDebugRuntime(debuggingContext)?.EnsureDetachedFromConnection(session);
	}

	internal void OnDebuggingSessionStarted(DebugSessionStartEventArgs e)
	{
		Volatile.Read(ref DebuggingSessionStarted)?.Invoke(this, e);
	}
}
