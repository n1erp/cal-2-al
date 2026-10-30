using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class RecordIdProviderRequest : DataProviderRequest
{
	private readonly NavRecordId recordId;

	public NavRecordId RecordId
	{
		[DebuggerStepThrough]
		get
		{
			return recordId;
		}
	}

	public RecordIdProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, DataLockState lockState, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, NavRecordId recordId)
		: base(companyToken, metaApplicationObject, lockState, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		if (recordId == null)
		{
			throw new ArgumentNullException("recordId");
		}
		this.recordId = recordId;
	}

	public RecordIdProviderRequest(DataProviderRequest dataProviderRequest, NavRecordId recordId)
		: this(dataProviderRequest.CompanyToken, dataProviderRequest.MetaApplicationObject, dataProviderRequest.LockState, dataProviderRequest.FiltersAndMarks, dataProviderRequest.FlowFieldSecurityFiltering, recordId)
	{
	}
}
