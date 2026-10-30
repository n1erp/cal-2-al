using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DatabaseTenantNavAppRetriever : ITenantNavAppRetriever
{
	public IEnumerable<TenantNavAppRecord> GetAllRecords()
	{
		return GetRecordsInternal(null);
	}

	public IEnumerable<TenantNavAppRecord> GetRecordsForTenant(string tenantId)
	{
		return GetRecordsInternal(tenantId);
	}

	private static IEnumerable<TenantNavAppRecord> GetRecordsInternal(string tenantId)
	{
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected Obj, but got Unknown
		string text = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		List<TenantNavAppRecord> list = new List<TenantNavAppRecord>();
		try
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000151);
			NCLMetaField fieldByNo = metaTableById.GetFieldByNo(1);
			NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(2);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SF", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Looking up installed tenant apps from application database -- Tenant:{0}", text);
			FiltersAndMarks filtersAndMarks = FiltersAndMarks.Empty;
			if (text != null)
			{
				UnaryFilterExpression item = new UnaryFilterExpression(FilterExpressionType.Equal, NavText.Create(text), fieldByNo.ExpressionContext);
				filtersAndMarks = new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, item)));
			}
			PositionedFindProviderRequest request = new PositionedFindProviderRequest(0, metaTableById, DataLockState.None, filtersAndMarks, null, SecurityFiltering.Ignored, FieldList.Empty, metaTableById.PrimaryKey.DefinedSortingFields, FindType.All, null, includeCurrent: false);
			foreach (ReadOnlyRecordBuffer item3 in NavSqlIsamQueryCommand.FindAll(NavGlobal.AppDatabase, request, () => false))
			{
				string value = ((NavText)item3[fieldByNo.FieldIndex]).Value;
				PackageId val = PackageId.op_Implicit(((NavGuid)item3[fieldByNo2.FieldIndex]).Value);
				TenantNavAppRecord item2 = new TenantNavAppRecord(value, val);
				list.Add(item2);
			}
			return list;
		}
		catch (NavSqlException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000077", Category.Extensions, exception, Verbosity.Warning);
			return new List<TenantNavAppRecord>();
		}
	}
}
