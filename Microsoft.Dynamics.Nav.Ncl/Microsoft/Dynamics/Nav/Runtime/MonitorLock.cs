using System;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class MonitorLock : IDisposable
{
	private readonly object lockObject;

	public MonitorLock(object lockObject, string name, int timeout)
	{
		this.lockObject = lockObject;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009IS", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.CurrentCulture, "Try enter \"{0}\" lock. Timeout {1} milliseconds.", name, timeout));
		if (Monitor.TryEnter(this.lockObject, timeout))
		{
			return;
		}
		string message = string.Format(CultureInfo.CurrentCulture, "Could not acquire \"{0}\" lock for {1} milliseconds.", name, timeout);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00009IQ", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, message);
		throw new NavLockTimeoutException(message);
	}

	public void Dispose()
	{
		Monitor.Exit(lockObject);
	}
}
