using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FindProviderRequest : DataProviderRequest
{
	private readonly SortingFieldList sortingFields;

	private readonly FindType findType;

	private readonly int skipNumberOfRows;

	private readonly int topNumberOfRowsToReturn;

	private readonly FieldList autoCalcFields;

	private readonly int? timeout;

	public SortingFieldList SortingFields
	{
		[DebuggerStepThrough]
		get
		{
			return sortingFields;
		}
	}

	public FindType FindType
	{
		[DebuggerStepThrough]
		get
		{
			return findType;
		}
	}

	public int SkipNumberOfRows
	{
		[DebuggerStepThrough]
		get
		{
			return skipNumberOfRows;
		}
	}

	public int TopNumberOfRowsToReturn
	{
		[DebuggerStepThrough]
		get
		{
			return topNumberOfRowsToReturn;
		}
	}

	public FieldList AutoCalcFields
	{
		[DebuggerStepThrough]
		get
		{
			return autoCalcFields;
		}
	}

	public int? Timeout
	{
		[DebuggerStepThrough]
		get
		{
			return timeout;
		}
	}

	public FindProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, DataLockState lockState, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering, FieldList autoCalcFields, SortingFieldList sortingFields, FindType findType, int topNumberOfRowsToReturn = 0, int skipNumberOfRows = 0, int? timeout = null)
		: base(companyToken, metaApplicationObject, lockState, filtersAndMarks, globalAndSecurityFilters, flowFieldSecurityFiltering)
	{
		this.sortingFields = sortingFields;
		this.findType = findType;
		this.topNumberOfRowsToReturn = topNumberOfRowsToReturn;
		this.skipNumberOfRows = skipNumberOfRows;
		this.autoCalcFields = autoCalcFields;
		this.timeout = timeout;
	}

	public FindProviderRequest(DataProviderRequest dataProviderRequest, FieldList autoCalcFields, SortingFieldList sortingFields, FindType findType, int? timeout = null)
		: this(dataProviderRequest.CompanyToken, dataProviderRequest.MetaApplicationObject, dataProviderRequest.LockState, dataProviderRequest.FiltersAndMarks, dataProviderRequest.GlobalAndSecurityFilters, dataProviderRequest.FlowFieldSecurityFiltering, autoCalcFields, sortingFields, findType, 0, 0, timeout)
	{
	}
}
