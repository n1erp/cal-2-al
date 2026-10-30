using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class PageInfoAndFieldsDataProvider : MetadataDataProvider
{
	private const int FirstFieldNo = 1;

	public const string RoleCenterFormId = "00000000-0000-0000-0000-000000000001";

	public const string ReportRequestFormId = "00000000-0000-0000-0000-000000000002";

	public const string LinksFormId = "00000000-0000-0000-0000-000000000003";

	public const string NotesFormId = "00000000-0000-0000-0000-000000000004";

	public const string ReportViewerFormId = "00000000-0000-0000-0000-000000000005";

	public const string SystemFormId = "00000000-0000-0000-0000-000000000006";

	private static NavText MaskedValue = NavText.Create("●●●●●●●●●●");

	private static NavText SpaceValue = NavText.Create(Lang.InspectorSpaceValue);

	private static NavText BLOBValue = NavText.Create(Lang.InspectorBlobValue);

	private static NavText ByteValue = NavText.Create(Lang.InspectorByteValue);

	private static NavText BlankValue = NavText.Create(Lang.InspectorBlankValue);

	private static NavText emptyNavText = NavText.Empty;

	private static NavText dataTypeTable = NavText.Create("Table");

	private static NavText dataTypeQuery = NavText.Create("Query");

	private static NCLMetaTable recordLinksMetadata;

	public override int TableId => 2000000204;

	private NCLMetaTable RecordLinksMetadata
	{
		get
		{
			if (recordLinksMetadata == null)
			{
				recordLinksMetadata = NclMetadata.GetMetaTableById(2000000068, -1, requireCompiled: false);
			}
			return recordLinksMetadata;
		}
	}

	public PageInfoAndFieldsDataProvider(NCLMetadata nclMetadata, MetadataProvider metadataProvider)
		: base(nclMetadata, metadataProvider)
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetValuesWithinRangeForKeyField(NCLMetaField keyField, ReadOnlyRecordBuffer precedingKeyFieldValues, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		switch (keyField.FieldNo)
		{
		case 1:
		{
			if (range?.LowValue != null && Guid.TryParse(range.LowValue.ToString(), out var _))
			{
				return new ReadOnlyRecordBuffer[1]
				{
					new ReadOnlyRecordBuffer(VirtualDataProvider.VirtualTimeStamp, range.LowValue, null)
				};
			}
			return new ReadOnlyRecordBuffer[0];
		}
		case 2:
			return GetFieldsOnForm((NavText)precedingKeyFieldValues[1], range, sortOrder, nonPrimaryKeyFilters);
		default:
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007S9", Category.Runtime, new NotSupportedException(), Verbosity.Error, DiagnosticParameter.SystemMetadata("Unexpected key field number."));
		}
	}

	private IEnumerable<ReadOnlyRecordBuffer> GetFieldsOnForm(NavText currentFormId, Range range, SortOrder sortOrder, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007KO", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading data for Page Inspector.");
		if (!range.GetInclusiveIntegerBounds(1, int.MaxValue, out var low, out var high))
		{
			return new ReadOnlyRecordBuffer[0];
		}
		new List<ReadOnlyRecordBuffer>();
		try
		{
			switch (currentFormId)
			{
			case "00000000-0000-0000-0000-000000000001":
			case "00000000-0000-0000-0000-000000000002":
				return new List<ReadOnlyRecordBuffer> { GetResultForReportOrRoleCenter(nonPrimaryKeyFilters, currentFormId) };
			case "00000000-0000-0000-0000-000000000005":
				return new List<ReadOnlyRecordBuffer> { GetResultForReportViewer(currentFormId) };
			case "00000000-0000-0000-0000-000000000006":
				return new List<ReadOnlyRecordBuffer> { GetResultForSystemPage(currentFormId) };
			case "00000000-0000-0000-0000-000000000003":
				return GetResultForSystemPart(Lang.InspectorLinksPageText, "00000000-0000-0000-0000-000000000003", GetAccessibleTableFieldsInRange(RecordLinksMetadata, sortOrder, low, high), nonPrimaryKeyFilters);
			case "00000000-0000-0000-0000-000000000004":
				return GetResultForSystemPart(Lang.InspectorNotesPageText, "00000000-0000-0000-0000-000000000004", GetAccessibleTableFieldsInRange(RecordLinksMetadata, sortOrder, low, high), nonPrimaryKeyFilters);
			default:
				return GetResultsForPage(currentFormId, nonPrimaryKeyFilters, sortOrder, low, high);
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00007KQ", Category.Runtime, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata("Something went wrong while trying to retrive data for Page Inspector."));
			throw;
		}
	}

	private ReadOnlyRecordBuffer GetResultForReportOrRoleCenter(FilterFieldDictionary nonPrimaryKeyFilters, NavText currentFormId)
	{
		int result = 0;
		string pageName = string.Empty;
		if (!nonPrimaryKeyFilters.IsNullOrEmpty())
		{
			Dictionary<INavFieldMetadata, FilterExpression> source = nonPrimaryKeyFilters.ToDictionary();
			string s = source.Where((KeyValuePair<INavFieldMetadata, FilterExpression> f) => f.Key.ColumnIndex == 3)?.FirstOrDefault().Value?.ToString() ?? string.Empty;
			if (!int.TryParse(s, out result))
			{
				result = 0;
			}
			pageName = source.Where((KeyValuePair<INavFieldMetadata, FilterExpression> f) => f.Key.ColumnIndex == 4)?.FirstOrDefault().Value?.ToString() ?? string.Empty;
		}
		string pageType;
		if (currentFormId == "00000000-0000-0000-0000-000000000001")
		{
			pageType = Lang.InspectorRoleCenterPageText;
		}
		else
		{
			pageType = ((currentFormId == "00000000-0000-0000-0000-000000000002") ? Lang.InspectorReportRequestPageText : string.Empty);
		}
		return CreateEntryWithoutFieldValues(currentFormId, 0, result, pageName, pageType, dataTypeTable, 0, emptyNavText);
	}

	private ReadOnlyRecordBuffer GetResultForReportViewer(NavText currentFormId)
	{
		string inspectorReportViewerPageText = Lang.InspectorReportViewerPageText;
		return CreateEntryWithoutFieldValues(currentFormId, 0, 0, emptyNavText, inspectorReportViewerPageText, emptyNavText, 0, emptyNavText);
	}

	private ReadOnlyRecordBuffer GetResultForSystemPage(NavText currentFormId)
	{
		string inspectorSystemPageText = Lang.InspectorSystemPageText;
		return CreateEntryWithoutFieldValues(currentFormId, 0, 0, emptyNavText, inspectorSystemPageText, emptyNavText, 0, emptyNavText);
	}

	private List<ReadOnlyRecordBuffer> GetResultForSystemPart(string systemPartName, string formHandle, List<NCLMetaField> fields, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		foreach (NCLMetaField field in fields)
		{
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavText.Create(formHandle), NavInteger.Create(field.FieldNo), NavInteger.Create(0), NavText.Create(systemPartName), NavText.Create(Lang.InspectorSystemPartPageText), dataTypeTable, NavInteger.Create(2000000068), NavText.Create(Lang.InspectorRecordLinksPageText), NavText.Create(field.FieldName), NavText.Create(field.FieldNavType.ToString()), emptyNavText, NavBoolean.Create(field.FieldIsPartOfPrimaryKey), emptyNavText, emptyNavText, emptyNavText, NavText.Create(GetFieldInfo(field)));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				list.Add(readOnlyRecordBuffer);
			}
		}
		return list;
	}

	private List<ReadOnlyRecordBuffer> GetResultsForPage(NavText currentFormId, FilterFieldDictionary nonPrimaryKeyFilters, SortOrder sortOrder, int low, int high)
	{
		List<ReadOnlyRecordBuffer> result = new List<ReadOnlyRecordBuffer>();
		NavForm registeredForm = NavCurrentThread.Session.Company.GetRegisteredForm(new Guid(currentFormId));
		if (registeredForm != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007KP", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Loading data for page {0} into Page Inspector.", registeredForm.ObjectId.ObjectNumber));
			NCLMetaTable table;
			NCLMetaApplicationObject nCLMetaApplicationObject;
			if (registeredForm.MasterPage.PageProperties.SourceObject.PageDataSource == DataSourceType.Query)
			{
				NCLMetaQuery nclMetaQuery = (registeredForm as NavQueryForm).NclMetaQuery;
				table = NclMetadata.GetMetaQueryTableById(nclMetaQuery.MetaQuery.Id);
				nCLMetaApplicationObject = nclMetaQuery;
			}
			else
			{
				nCLMetaApplicationObject = (SystemPages.IsServerSystemPageId(registeredForm.ObjectId.ObjectNumber) ? null : registeredForm.NclMetaForm);
				table = ((registeredForm.SourceTable != null) ? NclMetadata.GetMetaTableById(registeredForm.SourceTable.ObjectId.ObjectNumber, -1, requireCompiled: false) : null);
			}
			if (registeredForm.SourceTable == null)
			{
				NCLMetaForm nCLMetaForm = nCLMetaApplicationObject as NCLMetaForm;
				result = new List<ReadOnlyRecordBuffer> { CreateEntryWithoutFieldValues(currentFormId, -1, nCLMetaForm?.FormId ?? 0, nCLMetaForm?.Name, nCLMetaForm?.PageType.ToString(), dataTypeTable, 0, emptyNavText) };
			}
			else
			{
				result = GetResultsForPageWithSourceObject(currentFormId, registeredForm, nCLMetaApplicationObject, GetAccessibleTableFieldsInRange(table, sortOrder, low, high), nonPrimaryKeyFilters);
			}
		}
		return result;
	}

	private List<ReadOnlyRecordBuffer> GetResultsForPageWithSourceObject(NavText currentFormId, NavForm currForm, NCLMetaApplicationObject metaForm, IEnumerable<NCLMetaField> fields, FilterFieldDictionary nonPrimaryKeyFilters)
	{
		NavText navText = ((currForm.MasterPage.PageProperties.SourceObject.PageDataSource == DataSourceType.Query) ? dataTypeQuery : dataTypeTable);
		string sourceObjectName = string.Empty;
		int sourceObjectId = 0;
		GetPageInfo(currForm, metaForm, out var pageId, out var pageName, out var pageType, out sourceObjectId, out sourceObjectName);
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		foreach (NCLMetaField field in fields)
		{
			object obj;
			if (field.AppOrigin != null)
			{
				NavAppRuntimeMetadata val = NavCurrentThread.Session.Tenant.NavAppGroup.OrderedAppMetadata.FirstOrDefault((NavAppRuntimeMetadata app) =>
				{
					Guid value2 = ((WrappedValue<Guid>)(object)app.AppId).Value;
					Guid? appId = field.AppOrigin.AppId;
					return value2 == appId;
				});
				obj = ((val != null) ? val.Name : null);
			}
			else
			{
				obj = string.Empty;
			}
			string value = (string)obj;
			List<KeyValuePair<NavFilter, string>> list2 = (from filter in GetCurrentTableFilters(currForm.SourceTable)
				where filter.Key.FilterField == field.FieldNo
				select filter).ToList();
			bool flag = false;
			if (currForm.MasterPage.PageProperties.SourceObject.PageDataSource == DataSourceType.Table)
			{
				ControlDataboundDefinition controlDataboundDefinition = (metaForm as NCLMetaForm)?.Fields.Where((ControlDataboundDefinition ctrl) => ctrl.DataColumnName == field.FieldNo.ToString(CultureInfo.InvariantCulture)).FirstOrDefault();
				flag = field.ExtendedDataType == ExtendedDatatype.Masked || (controlDataboundDefinition != null && controlDataboundDefinition.ExtendedDatatype == ExtendedDatatype.Masked);
			}
			ReadOnlyRecordBuffer readOnlyRecordBuffer = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, currentFormId, NavInteger.Create(field.FieldNo), NavInteger.Create(pageId), NavText.Create(pageName), NavText.Create(pageType), navText, NavInteger.Create(currForm.SourceTable.TableID), NavText.Create(currForm.SourceTable.ObjectName), NavText.Create(field.FieldName), NavText.Create(field.FieldNavType.ToString()), flag ? MaskedValue : GetFieldValue(currForm.SourceTable, field), NavBoolean.Create(field.FieldIsPartOfPrimaryKey), NavText.Create(value), NavText.Create((list2.Count <= 0) ? string.Empty : (flag ? ((string)MaskedValue) : list2.FirstOrDefault().Key?.OptionsAsCaptionsFilterValue)), NavText.Create((list2.Count > 0) ? list2.FirstOrDefault().Value : string.Empty), NavText.Create(GetFieldInfo(field)));
			if (readOnlyRecordBuffer.PassesFieldFilters(nonPrimaryKeyFilters))
			{
				list.Add(readOnlyRecordBuffer);
			}
		}
		return list;
	}

	private static void GetPageInfo(NavForm navForm, NCLMetaApplicationObject metaObject, out int pageId, out string pageName, out string pageType, out int sourceObjectId, out string sourceObjectName)
	{
		SourceObjectDefinition sourceObject = navForm.MasterPage.PageProperties.SourceObject;
		if (sourceObject != null && sourceObject.PageDataSource == DataSourceType.Query)
		{
			NavQueryForm navQueryForm = navForm as NavQueryForm;
			pageId = 0;
			pageName = Lang.InspectorRunQueryPageText;
			pageType = navQueryForm?.MasterPage.PageProperties.PageType.ToString();
			sourceObjectId = navQueryForm?.NclMetaQuery.MetaQuery.Id ?? 0;
			sourceObjectName = navQueryForm?.NclMetaQuery.MetaQuery.Name;
			return;
		}
		sourceObjectId = 0;
		pageName = string.Empty;
		pageType = string.Empty;
		sourceObjectId = navForm.SourceTable?.TableID ?? 0;
		sourceObjectName = navForm.SourceTable?.ObjectName;
		switch (navForm.ObjectId.ObjectNumber)
		{
		case 9598:
			pageId = navForm.ObjectId.ObjectNumber;
			pageName = Lang.InspectorRunFilterPageText;
			pageType = Lang.InspectorSystemPageText;
			break;
		case 9599:
			pageId = navForm.ObjectId.ObjectNumber;
			pageName = Lang.InspectorRunTablePageText;
			pageType = Lang.InspectorSystemPageText;
			break;
		default:
		{
			NCLMetaForm nCLMetaForm = metaObject as NCLMetaForm;
			pageId = nCLMetaForm?.FormId ?? 0;
			pageName = nCLMetaForm?.Name;
			pageType = nCLMetaForm?.PageType.ToString();
			break;
		}
		}
	}

	private ReadOnlyRecordBuffer CreateEntryWithoutFieldValues(NavText currentFormId, int fieldNo, int pageNo, string pageName, string pageType, NavText dataSourceType, int sourceObjectId, string sourceObjectName)
	{
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, currentFormId, NavInteger.Create(fieldNo), NavInteger.Create(pageNo), NavText.Create(pageName), NavText.Create(pageType), dataSourceType, NavInteger.Create(sourceObjectId), NavText.Create(sourceObjectName), emptyNavText, emptyNavText, emptyNavText, NavBoolean.Create(value: false), emptyNavText, emptyNavText, emptyNavText, emptyNavText);
	}

	private static List<NCLMetaField> GetAccessibleTableFieldsInRange(NCLMetaTable table, SortOrder sortOrder, int low, int high)
	{
		IEnumerable<MetaField> source = ((table.TableType != TableType.Query) ? NavGlobal.MetadataProvider.GetTableMetadata(table.TableId) : NavGlobal.MetadataProvider.GetQueryTableMetadata(table.TableId))?.Fields.Where((MetaField metaField2) => metaField2.AccessByObjectType != AccessByObjectType.None && metaField2.AccessByObjectID != 0);
		List<NCLMetaField> list = new List<NCLMetaField>();
		IEnumerable<NCLMetaField> source2 = table?.AllFields;
		IEnumerable<NCLMetaField> enumerable = ((sortOrder == SortOrder.Ascending) ? source2.OrderBy((NCLMetaField f) => f.FieldNo) : source2.OrderByDescending((NCLMetaField f) => f.FieldNo));
		foreach (NCLMetaField field in enumerable)
		{
			if (field.FieldNo > high || field.FieldNo < low)
			{
				if ((sortOrder == SortOrder.Ascending && field.FieldNo > high) || (sortOrder == SortOrder.Descending && field.FieldNo < low))
				{
					break;
				}
				continue;
			}
			bool flag = true;
			MetaField metaField = source.SingleOrDefault((MetaField f) => f.Id == field.FieldNo);
			if (metaField != null)
			{
				flag = MetadataProvider.IsTableFieldAccessibleBasedOnLicenseAndUserPermissions(metaField);
			}
			if (flag)
			{
				list.Add(field);
			}
		}
		return list;
	}

	private static List<KeyValuePair<NavFilter, string>> GetCurrentTableFilters(NavRecord rec)
	{
		List<KeyValuePair<NavFilter, string>> list = new List<KeyValuePair<NavFilter, string>>();
		NavFilterGroup[] currentFilters = rec.GetCurrentFilters();
		foreach (NavFilterGroup navFilterGroup in currentFilters)
		{
			NavFilter[] filters = navFilterGroup.Filters;
			foreach (NavFilter key in filters)
			{
				if (navFilterGroup.FilterGroupNo != 6 && navFilterGroup.FilterGroupNo != 16)
				{
					list.Add(new KeyValuePair<NavFilter, string>(key, ((FilterGroupType)navFilterGroup.FilterGroupNo/*cast due to constrained. prefix*/).ToString()));
				}
			}
		}
		return list;
	}

	private static NavText GetFieldValue(NavRecord rec, NCLMetaField field)
	{
		NavText result = emptyNavText;
		_ = string.Empty;
		if (!rec.IsTemporary)
		{
			switch (field.FieldNavType)
			{
			case NavType.BLOB:
				result = BLOBValue;
				break;
			case NavType.Byte:
				result = ByteValue;
				break;
			case NavType.GUID:
				result = NavText.Create(rec.GetFieldValue(field.FieldNo).ToGuid().ToString("D"));
				break;
			default:
			{
				string value = rec.GetFieldValue(field.FieldNo).ToString();
				if (string.IsNullOrEmpty(value))
				{
					result = BlankValue;
				}
				else
				{
					result = ((!string.IsNullOrWhiteSpace(value)) ? NavText.Create(value) : SpaceValue);
				}
				break;
			}
			}
		}
		return result;
	}

	private static string GetFieldInfo(NCLMetaField field)
	{
		StringBuilder stringBuilder = new StringBuilder(field.FieldName);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ({0}, {1}", field.FieldNo, field.FieldNavType);
		if (field.FieldIsPartOfPrimaryKey)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ", {0}", Lang.InspectorPrimaryKeyValue);
		}
		stringBuilder.Append(')');
		return stringBuilder.ToString();
	}
}
