using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataProviderRequest
{
	private readonly int companyToken;

	private readonly NCLMetaApplicationObject metaApplicationObject;

	private readonly DataLockState lockState;

	private readonly FiltersAndMarks filtersAndMarks;

	private readonly TableFilterDictionary globalAndSecurityFilters;

	private readonly SecurityFiltering flowFieldSecurityFiltering;

	public int CompanyToken
	{
		[DebuggerStepThrough]
		get
		{
			return companyToken;
		}
	}

	public NCLMetaApplicationObject MetaApplicationObject
	{
		[DebuggerStepThrough]
		get
		{
			return metaApplicationObject;
		}
	}

	public DataLockState LockState
	{
		[DebuggerStepThrough]
		get
		{
			return lockState;
		}
	}

	public FiltersAndMarks FiltersAndMarks
	{
		[DebuggerStepThrough]
		get
		{
			return filtersAndMarks;
		}
	}

	public TableFilterDictionary GlobalAndSecurityFilters
	{
		[DebuggerStepThrough]
		get
		{
			return globalAndSecurityFilters;
		}
	}

	public SecurityFiltering FlowFieldSecurityFiltering
	{
		[DebuggerStepThrough]
		get
		{
			return flowFieldSecurityFiltering;
		}
	}

	public DataProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, DataLockState lockState, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering)
	{
		if (metaApplicationObject == null)
		{
			throw new ArgumentNullException("metaApplicationObject");
		}
		if (filtersAndMarks == null)
		{
			throw new ArgumentNullException("filtersAndMarks");
		}
		this.companyToken = companyToken;
		this.metaApplicationObject = metaApplicationObject;
		this.lockState = lockState;
		this.filtersAndMarks = filtersAndMarks;
		this.globalAndSecurityFilters = globalAndSecurityFilters;
		this.flowFieldSecurityFiltering = flowFieldSecurityFiltering;
	}
}
