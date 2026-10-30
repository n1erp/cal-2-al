using System;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavMetadataAccessContext : IDisposable
{
	private int refCount;

	private bool disposed;

	private bool CanBeDisposed => refCount == 0;

	public NavSession Session { get; private set; }

	public string ActivityId { get; }

	private void AddReference()
	{
		refCount++;
	}

	private void Release()
	{
		refCount--;
		if (refCount < 0)
		{
			throw new NavInvalidCallException();
		}
		if (CanBeDisposed)
		{
			InnerDispose();
			disposed = true;
		}
	}

	private NavMetadataAccessContext(NavSession session, string activityId)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (string.IsNullOrEmpty(activityId))
		{
			throw new ArgumentNullException("activityId");
		}
		ActivityId = activityId;
		Session = session;
		Session.SetAccessToRemovedMetadataContext(this);
	}

	public static NavMetadataAccessContext GetAccessToRemoved(NavSession session, string activityId)
	{
		if (session == null)
		{
			return null;
		}
		NavMetadataAccessContext navMetadataAccessContext = session.RemovedMetadataAccessContext ?? new NavMetadataAccessContext(session, activityId);
		navMetadataAccessContext.AddReference();
		return navMetadataAccessContext;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed && disposing)
		{
			Release();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		if (disposed)
		{
			GC.SuppressFinalize(this);
		}
	}

	protected virtual void InnerDispose()
	{
		Session.ClearAccessToRemovedMetadataContext();
		Session = null;
	}
}
