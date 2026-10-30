using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavDataUpgradeProcessExecutionState : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public string TenantId { get; set; }

	public int TotalFunctionCount => ExecutionDetails.Count();

	public int ExecutedFunctions => ExecutionDetails.Count((NavDataUpgradeExecutionDetails next) => next.State == NavDataUpgradeMethodExecutionState.Completed);

	[DataMember]
	public NavDataUpgradeExecutionMode UpgradeExecutionMode { get; private set; }

	public string Progress => NumericProgress.ToString("P", CultureInfo.InvariantCulture);

	[DataMember]
	[Obsolete("This property is obsolete and will be removed in future versions. Use the ExecutionDetails property instead")]
	public IEnumerable<NavDataUpgradeMethodExecutionDetails> Details
	{
		get
		{
			return ExecutionDetails.OfType<NavDataUpgradeMethodExecutionDetails>().ToList();
		}
		private set
		{
			ExecutionDetails = value;
		}
	}

	[Obsolete("This property is obsolete and will be removed in future versions. Use the ExecutionErrors property instead")]
	public IEnumerable<NavDataUpgradeMethodExecutionDetails> Errors
	{
		get
		{
			IEnumerable<NavDataUpgradeMethodExecutionDetails> enumerable = ExtractErrorMessages(Details).OfType<NavDataUpgradeMethodExecutionDetails>();
			if (!enumerable.Any())
			{
				return null;
			}
			return enumerable;
		}
	}

	[DataMember]
	public IEnumerable<NavDataUpgradeExecutionDetails> ExecutionDetails { get; private set; }

	public IEnumerable<NavDataUpgradeExecutionDetails> ExecutionErrors
	{
		get
		{
			IEnumerable<NavDataUpgradeExecutionDetails> enumerable = ExtractErrorMessages(ExecutionDetails);
			if (!enumerable.Any())
			{
				return null;
			}
			return enumerable;
		}
	}

	[DataMember]
	public NavWorkflowState State { get; private set; }

	[IgnoreDataMember]
	public double NumericProgress
	{
		get
		{
			if (TotalFunctionCount == 0)
			{
				return 0.0;
			}
			return (double)ExecutedFunctions / (double)TotalFunctionCount;
		}
	}

	[DataMember]
	public bool IsTenantInExclusiveAccess { get; set; }

	public static NavDataUpgradeProcessExecutionState Generate(IEnumerable<NavDataUpgradeExecutionDetails> executionDetails, NavWorkflowState currentState, string tenantId, bool isSingleTransactionUsed = false)
	{
		return new NavDataUpgradeProcessExecutionState(executionDetails, currentState, tenantId, isSingleTransactionUsed);
	}

	public NavDataUpgradeProcessExecutionState()
	{
	}

	public NavDataUpgradeProcessExecutionState(NavDataUpgradeProcessExecutionDetails navDataUpgradeProcessExecutionDetails)
	{
		ExecutionDetails = navDataUpgradeProcessExecutionDetails?.Details;
		State = navDataUpgradeProcessExecutionDetails?.State ?? NavWorkflowState.NotStarted;
		TenantId = navDataUpgradeProcessExecutionDetails?.TenantId;
		UpgradeExecutionMode = NavDataUpgradeExecutionMode.UsingParallelOrSerialTransaction;
	}

	private NavDataUpgradeProcessExecutionState(IEnumerable<NavDataUpgradeExecutionDetails> upgradeExecutionDetails, NavWorkflowState currentState, string tenantId, bool isSingleTransactionUsed)
	{
		if (upgradeExecutionDetails == null)
		{
			throw new ArgumentNullException("upgradeExecutionDetails");
		}
		if (string.IsNullOrEmpty(tenantId))
		{
			throw new ArgumentException("Invalid tenantId value", "tenantId");
		}
		ExecutionDetails = upgradeExecutionDetails;
		State = currentState;
		TenantId = tenantId;
		UpgradeExecutionMode = ((!isSingleTransactionUsed) ? NavDataUpgradeExecutionMode.UsingParallelOrSerialTransaction : NavDataUpgradeExecutionMode.UsingSingleTransaction);
	}

	private static IEnumerable<NavDataUpgradeExecutionDetails> ExtractErrorMessages(IEnumerable<NavDataUpgradeExecutionDetails> upgradeExecutionDetails)
	{
		return upgradeExecutionDetails.Where((NavDataUpgradeExecutionDetails detail) => (detail.State == NavDataUpgradeMethodExecutionState.Failed || detail.State == NavDataUpgradeMethodExecutionState.FailedPendingResume) && !string.IsNullOrEmpty(detail.Error));
	}

	public NavDataUpgradeProcessExecutionDetails ToNavDataUpgradeProcessExecutionDetails()
	{
		return new NavDataUpgradeProcessExecutionDetails(this);
	}
}
