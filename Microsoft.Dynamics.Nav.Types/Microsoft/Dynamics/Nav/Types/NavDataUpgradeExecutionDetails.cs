using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[KnownType(typeof(NavDataUpgradeMethodExecutionDetails))]
[KnownType(typeof(NavDataUpgradeEventExecutionDetails))]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavDataUpgradeExecutionDetails
{
	private DateTime? startTimeUtc;

	[DataMember]
	public Guid ExecutionSourceId { get; set; }

	[DataMember]
	public virtual int SessionId { get; set; }

	[DataMember]
	public virtual string CompanyName { get; set; }

	[DataMember]
	public virtual DateTime? StartTime { get; set; }

	[DataMember]
	public virtual TimeSpan? Duration { get; set; }

	[DataMember]
	public virtual NavDataUpgradeMethodExecutionState State { get; set; }

	[DataMember]
	public virtual string Error { get; set; }

	[DataMember]
	public bool PostUpgrade { get; set; }

	public NavDataUpgradeExecutionDetails()
	{
	}

	public NavDataUpgradeExecutionDetails(Guid sourceId, string companyName = null)
	{
		ExecutionSourceId = sourceId;
		CompanyName = companyName;
		Duration = null;
		State = NavDataUpgradeMethodExecutionState.Pending;
	}

	public void Start()
	{
		startTimeUtc = DateTime.UtcNow;
		StartTime = startTimeUtc.Value.ToLocalTime();
	}

	public void Stop()
	{
		if (!startTimeUtc.HasValue)
		{
			Duration = TimeSpan.FromSeconds(0.0);
		}
		else
		{
			Duration = DateTime.UtcNow - startTimeUtc.Value;
		}
	}
}
