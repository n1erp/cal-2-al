using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcFieldsCacheRequest : DataCacheRequest
{
	private readonly FieldList fieldsToCalculate;

	public FieldList FieldsToCalculate
	{
		[DebuggerStepThrough]
		get
		{
			return fieldsToCalculate;
		}
	}

	public CalcFieldsCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalculate)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		if (fieldsToCalculate == null)
		{
			throw new ArgumentNullException("fieldsToCalculate");
		}
		this.fieldsToCalculate = fieldsToCalculate;
	}

	protected override int CalculateHashCode()
	{
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), fieldsToCalculate.GetHashCode());
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
		if (other is CalcFieldsCacheRequest calcFieldsCacheRequest)
		{
			return calcFieldsCacheRequest.FieldsToCalculate.Equals(FieldsToCalculate);
		}
		return false;
	}
}
