using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExistsProviderRequest : DataProviderRequest
{
	private readonly SortingFieldList sortingFields;

	public SortingFieldList SortingFields
	{
		[DebuggerStepThrough]
		get
		{
			return sortingFields;
		}
	}

	public ExistsProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, DataLockState lockState, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering, SortingFieldList sortingFields)
		: base(companyToken, metaApplicationObject, lockState, filtersAndMarks, globalAndSecurityFilters, flowFieldSecurityFiltering)
	{
		this.sortingFields = sortingFields;
	}
}
