using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcNumericProviderRequest : DataProviderRequest
{
	private readonly IRecordBuffer recordBuffer;

	private readonly FieldList fieldsToCalculate;

	public IRecordBuffer RecordBuffer
	{
		[DebuggerStepThrough]
		get
		{
			return recordBuffer;
		}
	}

	public FieldList FieldsToCalculate
	{
		[DebuggerStepThrough]
		get
		{
			return fieldsToCalculate;
		}
	}

	public CalcNumericProviderRequest(int companyToken, NCLMetaApplicationObject sourceMetaApplicationObject, FiltersAndMarks sourceFiltersAndMarks, SecurityFiltering sourceSecurityFiltering, IRecordBuffer recordBuffer, FieldList fieldsToCalculate, DataLockState dataLockState)
		: base(companyToken, sourceMetaApplicationObject, dataLockState, sourceFiltersAndMarks, null, sourceSecurityFiltering)
	{
		if (recordBuffer == null)
		{
			throw new ArgumentNullException("recordBuffer");
		}
		if (fieldsToCalculate == null)
		{
			throw new ArgumentNullException("fieldsToCalculate");
		}
		if (fieldsToCalculate.AsNclMetaFields().Any((NCLMetaField field) => sourceMetaApplicationObject != field.CalculationFormula.SourceField.Parent))
		{
			throw new ArgumentException("Fields must be from same applicationobject");
		}
		this.recordBuffer = recordBuffer;
		this.fieldsToCalculate = fieldsToCalculate;
	}

	public CalcNumericProviderRequest(DataProviderRequest dataProviderRequest, IRecordBuffer recordBuffer, FieldList fieldsToCalculate)
		: this(dataProviderRequest.CompanyToken, dataProviderRequest.MetaApplicationObject, dataProviderRequest.FiltersAndMarks, dataProviderRequest.FlowFieldSecurityFiltering, recordBuffer, fieldsToCalculate, dataProviderRequest.LockState)
	{
	}
}
