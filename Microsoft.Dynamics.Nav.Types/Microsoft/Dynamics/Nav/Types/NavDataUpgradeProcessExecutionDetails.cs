using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavDataUpgradeProcessExecutionDetails
{
	public string TenantId { get; set; }

	public int TotalFunctionCount => Details.Count();

	public int ExecutedFunctions => Details.Count((NavDataUpgradeMethodExecutionDetails next) => next.State == NavDataUpgradeMethodExecutionState.Completed);

	public string Progress => GetNumericProgress().ToString("P", CultureInfo.InvariantCulture);

	public IEnumerable<NavDataUpgradeMethodExecutionDetails> Details { get; private set; }

	public IEnumerable<NavDataUpgradeMethodExecutionDetails> Errors
	{
		get
		{
			IEnumerable<NavDataUpgradeMethodExecutionDetails> enumerable = ExtractErrorMessages(Details);
			if (!enumerable.Any())
			{
				return null;
			}
			return enumerable;
		}
	}

	public NavWorkflowState State { get; private set; }

	public static NavDataUpgradeProcessExecutionDetails Generate(IEnumerable<NavDataUpgradeMethodExecutionDetails> codeunitsExecutionDetails, NavWorkflowState currentState, string tenantId)
	{
		return new NavDataUpgradeProcessExecutionDetails(codeunitsExecutionDetails, currentState, tenantId);
	}

	private NavDataUpgradeProcessExecutionDetails(IEnumerable<NavDataUpgradeMethodExecutionDetails> upgradeExecutionDetails, NavWorkflowState currentState, string tenantId)
	{
		if (upgradeExecutionDetails == null)
		{
			throw new ArgumentNullException("upgradeExecutionDetails");
		}
		if (string.IsNullOrEmpty(tenantId))
		{
			throw new ArgumentException("Invalid tenantId value", "tenantId");
		}
		Details = upgradeExecutionDetails;
		State = currentState;
		TenantId = tenantId;
	}

	public NavDataUpgradeProcessExecutionDetails(NavDataUpgradeProcessExecutionState status)
	{
		Details = status.ExecutionDetails.OfType<NavDataUpgradeMethodExecutionDetails>().ToList();
		State = status.State;
		TenantId = status.TenantId;
	}

	public double GetNumericProgress()
	{
		if (TotalFunctionCount == 0)
		{
			return 0.0;
		}
		return (double)ExecutedFunctions / (double)TotalFunctionCount;
	}

	private static IEnumerable<NavDataUpgradeMethodExecutionDetails> ExtractErrorMessages(IEnumerable<NavDataUpgradeMethodExecutionDetails> codeunitsExecutionDetails)
	{
		return codeunitsExecutionDetails.Where((NavDataUpgradeMethodExecutionDetails detail) => (detail.State == NavDataUpgradeMethodExecutionState.Failed || detail.State == NavDataUpgradeMethodExecutionState.FailedPendingResume) && !string.IsNullOrEmpty(detail.Error));
	}
}
