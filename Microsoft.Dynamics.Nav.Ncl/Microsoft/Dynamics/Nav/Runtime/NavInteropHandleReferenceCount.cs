using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class NavInteropHandleReferenceCount : IDisposable
{
	private int referenceCount;

	private bool isDisposing;

	private bool suppressDispose;

	internal bool SuppressDispose => suppressDispose;

	internal NavInteropHandleReferenceCount(bool suppressDispose)
	{
		referenceCount = 1;
		this.suppressDispose = suppressDispose;
	}

	internal void AddReference()
	{
		referenceCount++;
	}

	protected abstract void Dispose(bool disposing);

	public void Dispose()
	{
		lock (this)
		{
			if (!isDisposing && --referenceCount == 0)
			{
				isDisposing = true;
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}
		}
	}
}
