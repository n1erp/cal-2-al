using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDataUpgradeWorkflowStartRequest
{
	public string TenantId { get; set; }

	public Guid ClientSessionId { get; set; }

	public bool ContinueOnErrorEnabled { get; set; }

	public bool ParallelExecutionEnabled { get; set; }

	public bool SingleTransaction { get; set; }

	public bool SkipCompanyInitialization { get; set; }

	public bool ExcludeExtensions { get; set; }

	public int LanguageLcid { get; set; }

	public IEnumerable<string> Companies { get; set; }

	public Action<NavDataUpgradeProcessExecutionState> OnDone { get; set; }

	public bool StopRunningSessions { get; set; }
}
