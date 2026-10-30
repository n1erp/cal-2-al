using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavClientHandle : NavInteropHandleReferenceCount
{
	private NavAutomationHandle handle;

	internal NavAutomationHandle Handle => handle;

	internal NavClientHandle(NavAutomationHandle handle, bool suppressDispose)
		: base(suppressDispose)
	{
		this.handle = handle;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (handle.Handle != -1 && NavCurrentThread.Session != null && NavCurrentThread.Session.CallbackAllowed)
			{
				NavCurrentThread.Session.ClientCallback.DisposeAutomationObject(handle.Handle, SuppressDispose);
			}
			handle.Handle = -1;
		}
	}
}
