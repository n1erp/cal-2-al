namespace Microsoft.Dynamics.Nav.Runtime;

internal class CountCacheRequest : DataCacheRequest
{
	public CountCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
	}
}
