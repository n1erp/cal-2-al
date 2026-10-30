using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class AutoCalcFieldsCacheRequest : DataCacheRequest
{
	private readonly FieldList autoCalcFields;

	public FieldList AutoCalcFields
	{
		[DebuggerStepThrough]
		get
		{
			return autoCalcFields;
		}
	}

	public AutoCalcFieldsCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, TableFilterDictionary globalAndSecurityFilters, SecurityFiltering flowFieldSecurityFiltering, FieldList autoCalcFields)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, globalAndSecurityFilters, flowFieldSecurityFiltering)
	{
		this.autoCalcFields = autoCalcFields;
	}

	public override bool CanUseResultSetCache()
	{
		if (base.CanUseResultSetCache())
		{
			return autoCalcFields == null;
		}
		return false;
	}

	protected override int CalculateHashCode()
	{
		int h = ((autoCalcFields != null) ? autoCalcFields.GetHashCode() : 0);
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), h);
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
		if (other is AutoCalcFieldsCacheRequest autoCalcFieldsCacheRequest && autoCalcFields == null == (autoCalcFieldsCacheRequest.autoCalcFields == null))
		{
			if (autoCalcFields != null)
			{
				return autoCalcFields.Equals(autoCalcFieldsCacheRequest.autoCalcFields);
			}
			return true;
		}
		return false;
	}
}
