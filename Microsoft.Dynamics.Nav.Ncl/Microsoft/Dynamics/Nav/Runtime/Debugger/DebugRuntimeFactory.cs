using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal static class DebugRuntimeFactory
{
	internal static SingleInstanceDebugRuntime CreateSingleInstanceDebugRuntime(INavDebuggerCallback debuggerCallback)
	{
		return CreateSingleInstanceDebugRuntime(debuggerCallback, null, NavGlobal.NCLMetadata.CodeEnvironment);
	}

	internal static SingleInstanceDebugRuntime CreateSingleInstanceDebugRuntime(INavDebuggerCallback debuggerCallback, NavSession session, ALCodeEnvironment codeEnvironment)
	{
		DebugEnvironment environment = new DebugEnvironment(debuggerCallback, codeEnvironment);
		SingleInstanceDebugRuntime singleInstanceDebugRuntime = new SingleInstanceDebugRuntime(environment, session);
		if (!AddListener(singleInstanceDebugRuntime, session))
		{
			singleInstanceDebugRuntime.Dispose();
			throw new AnotherDebuggerAlreadyConnectedException(Lang.AnotherDebuggerAlreadyConnected);
		}
		SingleInstanceDebugRuntime.Instance = singleInstanceDebugRuntime;
		return singleInstanceDebugRuntime;
	}

	internal static DebugRuntime CreateDebugRuntimeInstance(NavSession session, bool enableWaitForBreakpoints)
	{
		return CreateDebugRuntimeInstance(null, session, NavGlobal.NCLMetadata.CodeEnvironment, enableWaitForBreakpoints);
	}

	internal static DebugRuntime CreateDebugRuntimeInstance(INavDebuggerCallback debuggerCallback, bool enableWaitForBreakpoints)
	{
		return CreateDebugRuntimeInstance(debuggerCallback, null, NavGlobal.NCLMetadata.CodeEnvironment, enableWaitForBreakpoints);
	}

	internal static DebugRuntime CreateDebugRuntimeInstance(INavDebuggerCallback debuggerCallback, NavSession session, ALCodeEnvironment codeEnvironment, bool enableWaitForBreakpoints)
	{
		DebugEnvironment environment = ((debuggerCallback == null) ? new DebugEnvironment(codeEnvironment) : new DebugEnvironment(debuggerCallback, codeEnvironment));
		DebugRuntime debugRuntime = new DebugRuntime(environment, session, enableWaitForBreakpoints);
		if (!AddListener(debugRuntime, session))
		{
			return null;
		}
		return debugRuntime;
	}

	private static bool AddListener(DebugRuntime runtime, NavSession session)
	{
		if (DataModificationListener.AddListener(runtime) && ExecutionListener.AddListener(runtime))
		{
			if (session != null && !runtime.DebuggedSession.IsOpen)
			{
				runtime.Dispose();
				throw new ConnectionToBeDebuggedIsClosedException(Lang.ConnectionToBeDebuggedIsClosed);
			}
			return true;
		}
		return false;
	}
}
