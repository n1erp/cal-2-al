using System;
using System.Diagnostics;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcFieldsForRecordCacheRequest : CalcFieldsCacheRequest
{
	private readonly IRecordBuffer recordBuffer;

	public IRecordBuffer RecordBuffer
	{
		[DebuggerStepThrough]
		get
		{
			return recordBuffer;
		}
	}

	public CalcFieldsForRecordCacheRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, bool cacheable, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, IRecordBuffer recordBuffer, FieldList fieldsToCalculate)
		: base(companyToken, metaApplicationObject, cacheable, filtersAndMarks, flowFieldSecurityFiltering, fieldsToCalculate)
	{
		if (recordBuffer == null)
		{
			throw new ArgumentNullException("recordBuffer");
		}
		this.recordBuffer = recordBuffer;
	}

	protected override int CalculateHashCode()
	{
		return HashCodeHelper.CombineHashCodes(base.CalculateHashCode(), recordBuffer.GetRecordId().GetHashCode());
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
		if (other is CalcFieldsForRecordCacheRequest calcFieldsForRecordCacheRequest)
		{
			return calcFieldsForRecordCacheRequest.RecordBuffer.GetRecordId().Equals(RecordBuffer.GetRecordId());
		}
		return false;
	}
}
