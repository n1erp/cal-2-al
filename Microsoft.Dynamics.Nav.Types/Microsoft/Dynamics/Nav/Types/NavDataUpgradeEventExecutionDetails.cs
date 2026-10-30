using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavDataUpgradeEventExecutionDetails : NavDataUpgradeExecutionDetails
{
	[DataMember]
	public string ModuleName { get; private set; }

	[DataMember]
	public string WorkflowStep { get; private set; }

	public NavDataUpgradeEventExecutionDetails(string moduleName, string workflowStep, Guid sourceId, string companyName = null)
		: base(sourceId, companyName)
	{
		if (string.IsNullOrWhiteSpace(moduleName))
		{
			throw new ArgumentException("Invalid module name", "moduleName");
		}
		if (string.IsNullOrEmpty(workflowStep))
		{
			throw new ArgumentException("Invalid workflow step", "workflowStep");
		}
		ModuleName = moduleName;
		WorkflowStep = workflowStep;
	}
}
