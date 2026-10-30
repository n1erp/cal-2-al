using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Diagnostics.Tracing;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavServerTraceSpan : IDisposable
{
	protected bool IsDisposed { get; private set; }

	protected string UserName { get; private set; }

	protected string TenantId { get; private set; }

	protected int SessionId { get; private set; }

	protected string ServerInstanceName { get; private set; }

	protected EventTask Task
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	internal NavServerTraceSpan(EventTask task, string serverInstanceName, string tenantId, int sessionId, string userName)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Task = task;
		TenantId = tenantId;
		SessionId = sessionId;
		UserName = userName;
		IsDisposed = false;
		ServerInstanceName = serverInstanceName;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public abstract void Enter();

	public abstract void Leave();

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !IsDisposed)
		{
			Leave();
			IsDisposed = true;
		}
	}

	[Conditional("DEBUG")]
	protected static void ThrowInvalidTaskException(EventTask eventTask)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid EventTask, Task: {0}", eventTask));
	}
}
