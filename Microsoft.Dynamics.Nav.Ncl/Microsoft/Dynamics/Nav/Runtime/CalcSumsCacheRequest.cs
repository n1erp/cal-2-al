using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcSumsCacheRequest : DataCacheRequest
{
	private readonly CalcFieldsCacheRequest calcFieldsCacheRequest;

	public FieldList FieldsToCalculate
	{
		[DebuggerStepThrough]
		get
		{
			return calcFieldsCacheRequest.FieldsToCalculate;
		}
	}

	internal CalcFieldsCacheRequest Wrapper => calcFieldsCacheRequest;

	public CalcSumsCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalculate)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		calcFieldsCacheRequest = new CalcFieldsCacheRequest(companyToken, metaApplicationObject, cacheable, filtersAndMarks, flowFieldSecurityFiltering, fieldsToCalculate);
	}

	protected override int CalculateHashCode()
	{
		return calcFieldsCacheRequest.GetHashCode();
	}

	public override bool Equals(DataCacheRequest other)
	{
		if (this == other)
		{
			return true;
		}
		if (other is CalcSumsCacheRequest calcSumsCacheRequest)
		{
			return calcSumsCacheRequest.Wrapper.Equals(calcFieldsCacheRequest);
		}
		return false;
	}
}
