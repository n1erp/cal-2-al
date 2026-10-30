namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDataUpgradeWorkflowResumeRequest
{
	public bool ResumeAllMode { get; set; }

	public int CodeunitId { get; set; }

	public string MethodName { get; set; }

	public string CompanyName { get; set; }
}
