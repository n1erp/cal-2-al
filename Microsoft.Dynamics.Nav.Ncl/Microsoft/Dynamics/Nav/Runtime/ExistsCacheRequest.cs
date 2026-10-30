using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ExistsCacheRequest : DataCacheRequest
{
	private readonly bool forceNonLockingProviderRequest;

	private readonly SortingFieldList sortingFields;

	public bool ForceNonLockingProviderRequest
	{
		[DebuggerStepThrough]
		get
		{
			return forceNonLockingProviderRequest;
		}
	}

	public SortingFieldList SortingFields
	{
		[DebuggerStepThrough]
		get
		{
			return sortingFields;
		}
	}

	public ExistsCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, bool forceNonLockingProviderRequest, SortingFieldList sortingFields = null)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		this.forceNonLockingProviderRequest = forceNonLockingProviderRequest;
		this.sortingFields = sortingFields;
	}

	protected override int CalculateHashCode()
	{
		int h = ((sortingFields != null) ? sortingFields.GetHashCode() : int.MaxValue);
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), forceNonLockingProviderRequest ? 1 : 0, h);
	}

	public override bool Equals(DataCacheRequest other)
	{
		if (!base.Equals(other))
		{
			return false;
		}
		if (!(other is ExistsCacheRequest existsCacheRequest))
		{
			return false;
		}
		if (forceNonLockingProviderRequest != existsCacheRequest.forceNonLockingProviderRequest)
		{
			return false;
		}
		return object.Equals(SortingFields, existsCacheRequest.SortingFields);
	}
}
