using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavApplicationServerConfiguration
{
	public const int DefaultRetryDelayAfter3Failures = 30000;

	public const int DefaultRetryDelayAfter6Failures = 60000;

	private readonly int startupCodeunit;

	private readonly string startupMethod;

	private readonly string argument;

	private readonly bool debugMode;

	private readonly TimeSpan retryPeriod;

	private readonly int retryDelayAfter3Failures;

	private readonly int retryDelayAfter6Failures;

	internal int StartupCodeunit => startupCodeunit;

	internal string StartupMethod => startupMethod;

	internal string Argument => argument;

	internal bool DebugMode => debugMode;

	public TimeSpan RetryPeriod => retryPeriod;

	public int RetryDelayAfter3Failures => retryDelayAfter3Failures;

	public int RetryDelayAfter6Failures => retryDelayAfter6Failures;

	public NavApplicationServerConfiguration(int startupCodeunit, string startupMethod, string argument, bool debugMode, TimeSpan retryPeriod, int retryDelayAfter3Failures = 30000, int retryDelayAfter6Failures = 60000)
	{
		this.startupCodeunit = startupCodeunit;
		this.startupMethod = startupMethod;
		this.argument = argument;
		this.debugMode = debugMode;
		this.retryPeriod = retryPeriod;
		this.retryDelayAfter3Failures = retryDelayAfter3Failures;
		this.retryDelayAfter6Failures = retryDelayAfter6Failures;
	}
}
