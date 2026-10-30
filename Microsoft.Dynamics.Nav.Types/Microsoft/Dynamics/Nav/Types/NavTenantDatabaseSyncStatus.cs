using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavTenantDatabaseSyncStatus
{
	private string detailedDatabaseSyncState;

	private int processedOperationsCount;

	private readonly int totalOperationsCount;

	[DataMember]
	public string DatabaseSyncStateDetailed
	{
		get
		{
			return detailedDatabaseSyncState;
		}
		set
		{
			detailedDatabaseSyncState = value;
		}
	}

	[DataMember]
	public int TotalOperationCount => totalOperationsCount;

	[DataMember]
	public int ProcessedOperationCount
	{
		get
		{
			return processedOperationsCount;
		}
		set
		{
			if (value < 0 || value > TotalOperationCount)
			{
				processedOperationsCount = TotalOperationCount;
			}
			else
			{
				processedOperationsCount = value;
			}
		}
	}

	public NavTenantDatabaseSyncStatus(int processedOperationsCount, int totalOperationCount, string detailedSyncStatus = "")
	{
		if (processedOperationsCount < 0)
		{
			throw new ArgumentOutOfRangeException("processedOperationsCount");
		}
		if (processedOperationsCount > totalOperationCount)
		{
			throw new ArgumentOutOfRangeException("processedOperationsCount");
		}
		if (totalOperationCount < 0)
		{
			throw new ArgumentOutOfRangeException("totalOperationCount");
		}
		this.processedOperationsCount = processedOperationsCount;
		totalOperationsCount = totalOperationCount;
		detailedDatabaseSyncState = detailedSyncStatus;
	}

	public override string ToString()
	{
		return detailedDatabaseSyncState;
	}
}
