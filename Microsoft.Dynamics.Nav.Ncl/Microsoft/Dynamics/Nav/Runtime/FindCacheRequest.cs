using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FindCacheRequest : AutoCalcFieldsCacheRequest
{
	private readonly SortingFieldList sortingFields;

	private readonly FindType findType;

	private readonly int skipNumberOfRows;

	private readonly int topNumberOfRowsToReturn;

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

	public virtual bool IsFromPosition
	{
		[DebuggerStepThrough]
		get
		{
			return false;
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

	public override bool ResultCanBeFoundInPrivateNonTransactionalCache => true;

	public override bool ResultCanBeStoredInPrivateNonTransactionalCache => true;

	public FindCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering, FieldList autoCalcFields, SortingFieldList sortingFields, FindType findType, int topNumberOfRowsToReturn = 0, int skipNumberOfRows = 0, int? timeout = null)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, globalAndSecurityFilters, flowFieldSecurityFiltering, autoCalcFields)
	{
		this.sortingFields = sortingFields;
		this.findType = findType;
		this.topNumberOfRowsToReturn = topNumberOfRowsToReturn;
		this.skipNumberOfRows = skipNumberOfRows;
		this.timeout = timeout;
	}

	public override bool CanUseResultSetCache()
	{
		if (base.CanUseResultSetCache())
		{
			return !sortingFields.HasFlowFields;
		}
		return false;
	}

	protected override int CalculateHashCode()
	{
		int h = ((sortingFields != null) ? sortingFields.GetHashCode() : int.MaxValue);
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), h, topNumberOfRowsToReturn.GetHashCode(), skipNumberOfRows.GetHashCode());
	}

	public override bool Equals(DataCacheRequest other)
	{
		if (this == other)
		{
			return true;
		}
		if (!base.Equals(other))
		{
			return false;
		}
		if (other is FindCacheRequest findCacheRequest && object.Equals(SortingFields, findCacheRequest.SortingFields) && topNumberOfRowsToReturn == findCacheRequest.topNumberOfRowsToReturn)
		{
			return skipNumberOfRows == findCacheRequest.skipNumberOfRows;
		}
		return false;
	}
}
