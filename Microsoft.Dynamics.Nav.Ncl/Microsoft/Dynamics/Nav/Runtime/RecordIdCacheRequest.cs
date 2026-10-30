using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class RecordIdCacheRequest : DataCacheRequest
{
	private readonly NavRecordId recordId;

	private readonly bool cacheOnly;

	public NavRecordId RecordId
	{
		[DebuggerStepThrough]
		get
		{
			return recordId;
		}
	}

	public bool CacheOnly => cacheOnly;

	public RecordIdCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, NavRecordId recordId, bool cacheOnly)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		if (recordId == null)
		{
			throw new ArgumentNullException("recordId");
		}
		this.recordId = recordId;
		this.cacheOnly = cacheOnly;
	}

	protected override int CalculateHashCode()
	{
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), RecordId.GetHashCode(), CacheOnly ? 1 : 0);
	}

	public override bool Equals(DataCacheRequest other)
	{
		if (!base.Equals(other))
		{
			return false;
		}
		if (other is RecordIdCacheRequest recordIdCacheRequest && RecordId.Equals(recordIdCacheRequest.RecordId))
		{
			return CacheOnly == recordIdCacheRequest.CacheOnly;
		}
		return false;
	}
}
