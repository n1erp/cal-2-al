using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CalcSumsProviderRequest : DataProviderRequest
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

	public CalcSumsProviderRequest(int companyToken, NCLMetaApplicationObject metaApplicationObject, DataLockState lockState, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalculate)
		: base(companyToken, metaApplicationObject, lockState, filtersAndMarks, null, flowFieldSecurityFiltering)
	{
		if (fieldsToCalculate == null)
		{
			throw new ArgumentNullException("fieldsToCalculate");
		}
		this.fieldsToCalculate = fieldsToCalculate;
	}

	public CalcSumsProviderRequest(DataProviderRequest dataProviderRequest, FieldList fieldsToCalculate)
		: this(dataProviderRequest.CompanyToken, dataProviderRequest.MetaApplicationObject, dataProviderRequest.LockState, dataProviderRequest.FiltersAndMarks, dataProviderRequest.FlowFieldSecurityFiltering, fieldsToCalculate)
	{
	}
}
