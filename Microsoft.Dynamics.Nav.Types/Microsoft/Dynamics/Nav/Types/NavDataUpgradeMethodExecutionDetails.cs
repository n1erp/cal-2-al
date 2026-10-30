using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavDataUpgradeMethodExecutionDetails : NavDataUpgradeExecutionDetails
{
	[Obsolete("Kept for cloud manager backcompat")]
	private DateTime? startTimeUtc = DateTime.MinValue;

	public override int SessionId { get; set; }

	public int CodeunitId { get; private set; }

	public string FunctionName { get; private set; }

	public override string CompanyName { get; set; }

	public override DateTime? StartTime { get; set; }

	public override TimeSpan? Duration { get; set; }

	public override NavDataUpgradeMethodExecutionState State { get; set; }

	public override string Error { get; set; }

	public NavDataUpgradeMethodExecutionDetails(int codeunitId, string functionName, Guid sourceId, string companyName = null)
		: base(sourceId, companyName)
	{
		if (codeunitId < 1)
		{
			throw new ArgumentOutOfRangeException("codeunitId");
		}
		if (string.IsNullOrEmpty(functionName))
		{
			throw new ArgumentException("Invalid function name", "functionName");
		}
		CodeunitId = codeunitId;
		FunctionName = functionName;
	}
}
