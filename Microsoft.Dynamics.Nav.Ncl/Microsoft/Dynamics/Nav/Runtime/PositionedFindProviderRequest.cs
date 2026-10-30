using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PositionedFindProviderRequest : FindProviderRequest
{
	private readonly MutableRecordBuffer startingPosition;

	private readonly bool includeCurrent;

	public MutableRecordBuffer StartingPosition
	{
		[DebuggerStepThrough]
		get
		{
			return startingPosition;
		}
	}

	public bool IncludeCurrent
	{
		[DebuggerStepThrough]
		get
		{
			return includeCurrent;
		}
	}

	public PositionedFindProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, DataLockState lockState, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering, FieldList autoCalcFields, SortingFieldList sortingFields, FindType findType, MutableRecordBuffer startingPosition, bool includeCurrent, int topNumberOfRowsToReturn = 0, int skipNumberOfRows = 0, int? timeout = null)
		: base(companyToken, metaApplicationObject, lockState, filtersAndMarks, globalAndSecurityFilters, flowFieldSecurityFiltering, autoCalcFields, sortingFields, findType, topNumberOfRowsToReturn, skipNumberOfRows, timeout)
	{
		this.startingPosition = startingPosition;
		this.includeCurrent = includeCurrent;
	}

	public PositionedFindProviderRequest(FindProviderRequest findProviderRequest, MutableRecordBuffer startingPosition, bool includeCurrent)
		: this(findProviderRequest.CompanyToken, findProviderRequest.MetaApplicationObject, findProviderRequest.LockState, findProviderRequest.FiltersAndMarks, findProviderRequest.GlobalAndSecurityFilters, findProviderRequest.FlowFieldSecurityFiltering, findProviderRequest.AutoCalcFields, findProviderRequest.SortingFields, findProviderRequest.FindType, startingPosition, includeCurrent, findProviderRequest.TopNumberOfRowsToReturn, findProviderRequest.SkipNumberOfRows, findProviderRequest.Timeout)
	{
	}

	public PositionedFindProviderRequest(DataProviderRequest dataProviderRequest, FieldList autoCalcFields, SortingFieldList sortingFields, FindType findType, MutableRecordBuffer startingPosition, bool includeCurrent)
		: this(dataProviderRequest.CompanyToken, dataProviderRequest.MetaApplicationObject, dataProviderRequest.LockState, dataProviderRequest.FiltersAndMarks, dataProviderRequest.GlobalAndSecurityFilters, dataProviderRequest.FlowFieldSecurityFiltering, autoCalcFields, sortingFields, findType, startingPosition, includeCurrent)
	{
	}
}
