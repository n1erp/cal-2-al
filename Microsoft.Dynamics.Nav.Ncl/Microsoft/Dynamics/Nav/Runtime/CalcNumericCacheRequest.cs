using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcNumericCacheRequest : DataCacheRequest
{
	private readonly CalcFieldsForRecordCacheRequest calcFieldsForRecordCacheRequest;

	public IRecordBuffer RecordBuffer
	{
		[DebuggerStepThrough]
		get
		{
			return calcFieldsForRecordCacheRequest.RecordBuffer;
		}
	}

	public FieldList FieldsToCalculate
	{
		[DebuggerStepThrough]
		get
		{
			return calcFieldsForRecordCacheRequest.FieldsToCalculate;
		}
	}

	internal CalcFieldsForRecordCacheRequest Wrapper => calcFieldsForRecordCacheRequest;

	public CalcNumericCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, IRecordBuffer recordBuffer, FieldList fieldsToCalculate)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		calcFieldsForRecordCacheRequest = new CalcFieldsForRecordCacheRequest(companyToken, metaApplicationObject, cacheable, filtersAndMarks, flowFieldSecurityFiltering, recordBuffer, fieldsToCalculate);
	}

	protected override int CalculateHashCode()
	{
		return calcFieldsForRecordCacheRequest.GetHashCode();
	}

	public override bool Equals(DataCacheRequest other)
	{
		if (this == other)
		{
			return true;
		}
		if (other is CalcNumericCacheRequest calcNumericCacheRequest)
		{
			return calcNumericCacheRequest.Wrapper.Equals(calcFieldsForRecordCacheRequest);
		}
		return false;
	}
}
