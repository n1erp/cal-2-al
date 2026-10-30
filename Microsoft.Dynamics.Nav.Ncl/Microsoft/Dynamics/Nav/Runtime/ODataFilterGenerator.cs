using System;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ODataFilterGenerator
{
	private Func<NCLMetaField, string> nameTransform;

	private static Func<NCLMetaField, string> GetDefaultFieldName = (NCLMetaField metaField) => MetaBase.ExternalizeName(metaField.FieldName);

	public ODataFilterGenerator()
	{
		nameTransform = GetDefaultFieldName;
	}

	public ODataFilterGenerator(Func<NCLMetaField, string> fieldNameTransform)
	{
		nameTransform = fieldNameTransform;
	}

	public static ODataFilterGenerator CreateDefault()
	{
		return new ODataFilterGenerator();
	}

	internal string ConvertFromNavToODatav3(NCLMetaField fieldMetadata, FilterExpression filter)
	{
		ODataV3FilterConversionVisitor oDataV3FilterConversionVisitor = new ODataV3FilterConversionVisitor(nameTransform);
		new StringBuilder();
		StringBuilder stringBuilder = oDataV3FilterConversionVisitor.Visit(filter, fieldMetadata);
		return stringBuilder.ToString();
	}

	internal string ConvertFromNavToODatav4(NCLMetaField fieldMetadata, FilterExpression filter)
	{
		ODataV4FilterConversionVisitor oDataV4FilterConversionVisitor = new ODataV4FilterConversionVisitor(nameTransform);
		new StringBuilder();
		StringBuilder stringBuilder = oDataV4FilterConversionVisitor.Visit(filter, fieldMetadata);
		return stringBuilder.ToString();
	}

	internal string ConvertToODatav3(TableState tableState)
	{
		FilterFieldDictionary allFilters = tableState.CombineFiltersFromAllGroups(FilterExpression.And);
		return ConvertToODatav3(allFilters);
	}

	internal string ConvertToODatav4(TableState tableState)
	{
		FilterFieldDictionary allFilters = tableState.CombineFiltersFromAllGroups(FilterExpression.And);
		return ConvertToODatav4(allFilters);
	}

	internal string ConvertToODatav3(FilterFieldDictionary allFilters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Tuple<INavFieldMetadata, FilterExpression> item in allFilters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> x) => x.Item1 is NCLMetaField))
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(" and ");
			}
			stringBuilder.Append(ConvertFromNavToODatav3((NCLMetaField)item.Item1, item.Item2));
		}
		return stringBuilder.ToString();
	}

	internal string ConvertToODatav4(FilterFieldDictionary allFilters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Tuple<INavFieldMetadata, FilterExpression> item in allFilters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> x) => x.Item1 is NCLMetaField))
		{
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(" and ");
			}
			stringBuilder.Append(ConvertFromNavToODatav4((NCLMetaField)item.Item1, item.Item2));
		}
		return stringBuilder.ToString();
	}

	public string ConvertRecordToODataV3Filter(object record)
	{
		NavRecord navRecord = record as NavRecord;
		if (record == null)
		{
			throw new NavNCLArgumentException("record");
		}
		return ConvertToODatav3(navRecord.RecordImplementation.TableState);
	}

	public string ConvertRecordToODataV4Filter(object record)
	{
		NavRecord navRecord = record as NavRecord;
		if (record == null)
		{
			throw new NavNCLArgumentException("record");
		}
		return ConvertToODatav4(navRecord.RecordImplementation.TableState);
	}

	public static string CreateODataV3Filter(int tableId, string recordView, int queryId = 0)
	{
		if (queryId != 0)
		{
			return CreateODataFilterForQuery(tableId, queryId, recordView, NavClientType.OData);
		}
		return CreateODataFilter(tableId, recordView, NavClientType.OData);
	}

	public static string CreateODataV4Filter(int tableId, string recordView, int queryId = 0)
	{
		if (queryId != 0)
		{
			return CreateODataFilterForQuery(tableId, queryId, recordView, NavClientType.ODataV4);
		}
		return CreateODataFilter(tableId, recordView, NavClientType.ODataV4);
	}

	internal static string CreateODataFilter(int tableId, string recordView, NavClientType clientType)
	{
		NavRecordRef navRecordRef = new NavRecordRef(NavCurrentThread.Session, tableId);
		navRecordRef.ALSetView(recordView);
		return clientType switch
		{
			NavClientType.OData => CreateDefault().ConvertRecordToODataV3Filter(navRecordRef.Record), 
			NavClientType.ODataV4 => CreateDefault().ConvertRecordToODataV4Filter(navRecordRef.Record), 
			_ => string.Empty, 
		};
	}

	internal static string CreateODataFilterForQuery(int tableId, int queryId, string recordView, NavClientType clientType)
	{
		NavRecordRef navRecordRef = new NavRecordRef(NavCurrentThread.Session, tableId);
		navRecordRef.ALSetView(recordView);
		ODataFilterGenerator oDataFilterGenerator = new ODataFilterGenerator(GetFieldNameMappedToQueryId(tableId, queryId));
		return clientType switch
		{
			NavClientType.OData => oDataFilterGenerator.ConvertRecordToODataV3Filter(navRecordRef.Record), 
			NavClientType.ODataV4 => oDataFilterGenerator.ConvertRecordToODataV4Filter(navRecordRef.Record), 
			_ => string.Empty, 
		};
	}

	public static Func<NCLMetaField, string> GetFieldNameMappedToQueryId(int dataTableId, int queryId)
	{
		return (NCLMetaField metaField) =>
		{
			NCLMetaQuery metaQueryById = NavCurrentThread.Session.NCLMetadata.GetMetaQueryById(queryId);
			MetaQueryDataItem metaQueryDataItem = metaQueryById.MetaQuery.DataItems.Find((MetaQueryDataItem x) => x.TableNo == dataTableId);
			MetaQueryColumn metaQueryColumn = metaQueryDataItem.QueryColumns.Find((MetaQueryColumn x) => x.FieldNo == metaField.FieldNo);
			return MetaBase.ExternalizeName(metaQueryColumn.Name);
		};
	}
}
