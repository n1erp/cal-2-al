using System.Runtime.InteropServices;

namespace System.Reflection.Internal;

internal abstract class CriticalDisposableObject : CriticalHandle
{
	public sealed override bool IsInvalid => true;

	public CriticalDisposableObject()
		: base(IntPtr.Zero)
	{
	}

	protected sealed override bool ReleaseHandle()
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected new void SetHandle(IntPtr handle)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected sealed override void Dispose(bool disposing)
	{
		Release();
		if (disposing)
		{
			GC.SuppressFinalize(this);
		}
	}

	protected abstract void Release();
}
