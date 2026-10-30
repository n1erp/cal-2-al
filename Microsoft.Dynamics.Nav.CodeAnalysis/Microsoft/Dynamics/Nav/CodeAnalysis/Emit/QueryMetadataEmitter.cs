using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class QueryMetadataEmitter : ObjectMetadataEmitter<QueryTypeSymbol>
{
	private int queryColumnIndex;

	public QueryMetadataEmitter(QueryTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Query", () =>
			{
				WriteQueryBaseProperties(xmlWriter);
				WriteQueryProperties(xmlWriter);
				WriteDataItems(xmlWriter, Symbol.FlattenedDataItems);
			});
		});
	}

	protected override bool TryUseSpecialPropertyValueMetadataEmitter(XmlWriter xmlWriter, PropertySymbol property)
	{
		BoundPropertyValue propertyValue = property.Property.PropertyValue;
		switch (propertyValue.Kind)
		{
		case BoundKind.OrderByPropertyValue:
			xmlWriter.WriteOrderByPropertyValue((BoundOrderByPropertyValue)propertyValue);
			return true;
		case BoundKind.DataItemLinkPropertyValue:
			xmlWriter.WriteQueryDataItemLinkPropertyValue((BoundDataItemLinkPropertyValue)propertyValue);
			return true;
		case BoundKind.TableFilterStatementPropertyValue:
			xmlWriter.WriteQueryFilterPropertyValue((BoundTableFilterStatementPropertyValue)propertyValue, "DataItemTableFilter");
			return true;
		default:
			return false;
		}
	}

	private void WriteDataItems(XmlWriter xmlWriter, ImmutableArray<QueryDataItemSymbol> flattenedDataItems)
	{
		List<BoundTableFilterStatementPropertyValue> columnFilters = new List<BoundTableFilterStatementPropertyValue>();
		foreach (QueryDataItemSymbol item in flattenedDataItems)
		{
			WriteDataItems(xmlWriter, item, columnFilters);
		}
		WriteColumnFilters(xmlWriter, columnFilters);
	}

	private static void WriteColumnFilters(XmlWriter xmlWriter, List<BoundTableFilterStatementPropertyValue> columnFilters)
	{
		foreach (BoundTableFilterStatementPropertyValue columnFilter in columnFilters)
		{
			xmlWriter.WriteQueryFilterPropertyValue(columnFilter, "ColumnFilter");
		}
	}

	private void WriteDataItems(XmlWriter xmlWriter, QueryDataItemSymbol dataItem, List<BoundTableFilterStatementPropertyValue> columnFilters)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "DataItem", () =>
		{
			WriteProperties(xmlWriter, dataItem, dataItem.Properties, shouldOutputDefaultValues: true);
			WriteSqlJoinType(xmlWriter, dataItem);
			xmlWriter.WriteElementString("DataItemTable", dataItem.RelatedTable.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteElementString("ID", dataItem.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteElementString("Name", dataItem.Name);
			IEnumerable<QueryElementSymbol> dataItemColumns = (from m in dataItem.GetMembers()
				where m.Kind == SymbolKind.QueryColumn || m.Kind == SymbolKind.QueryFilter
				select m).Cast<QueryElementSymbol>();
			WriteDataItemColumnsAndFilters(xmlWriter, dataItemColumns, columnFilters);
		});
	}

	private void WriteQueryElementContent(XmlWriter xmlWriter, QueryElementSymbol element, List<BoundTableFilterStatementPropertyValue> columnFilters)
	{
		WriteProperties(xmlWriter, element, element.Properties, shouldOutputDefaultValues: true);
		xmlWriter.WriteElementString("ID", element.Id.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteElementString("Name", element.Name);
		xmlWriter.WriteElementString("FieldNo", (element.RelatedField?.Id ?? 0).ToString(CultureInfo.InvariantCulture));
		BoundTableFilterStatementPropertyValue boundPropertyValue = element.GetBoundPropertyValue<BoundTableFilterStatementPropertyValue>(PropertyKind.ColumnFilter);
		if (boundPropertyValue != null)
		{
			columnFilters.Add(boundPropertyValue);
		}
		WriteColumnTypeAndMethod(xmlWriter, element);
	}

	private void WriteDataItemColumnsAndFilters(XmlWriter xmlWriter, IEnumerable<QueryElementSymbol> dataItemColumns, List<BoundTableFilterStatementPropertyValue> columnFilters)
	{
		foreach (QueryElementSymbol column in dataItemColumns)
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "QueryColumn", () =>
			{
				WriteQueryElementContent(xmlWriter, column, columnFilters);
				if (column.Kind == SymbolKind.QueryColumn)
				{
					xmlWriter.WriteElementString("FilterOnly", "0");
					xmlWriter.WriteElementString("QueryColumnIndex", queryColumnIndex.ToString(CultureInfo.InvariantCulture));
					queryColumnIndex++;
				}
				else
				{
					xmlWriter.WriteElementString("FilterOnly", "1");
					xmlWriter.WriteElementString("QueryColumnIndex", "-1");
				}
			});
		}
	}

	private static void WriteSqlJoinType(XmlWriter xmlWriter, QueryDataItemSymbol dataItem)
	{
		SqlJoinTypeKind joinType = dataItem.GetEnumPropertyValue<SqlJoinTypeKind>(PropertyKind.SqlJoinType) ?? SqlJoinTypeKind.LeftOuterJoin;
		xmlWriter.WriteElementString("DataItemLinkType", ConvertSqlJoinTypeToString(joinType));
	}

	private static string ConvertSqlJoinTypeToString(SqlJoinTypeKind joinType)
	{
		return joinType switch
		{
			SqlJoinTypeKind.LeftOuterJoin => "Left Outer Join", 
			SqlJoinTypeKind.InnerJoin => "Inner Join", 
			SqlJoinTypeKind.RightOuterJoin => "Right Outer Join", 
			SqlJoinTypeKind.FullOuterJoin => "Full Outer Join", 
			SqlJoinTypeKind.CrossJoin => "Cross Join", 
			_ => throw ExceptionUtilities.UnexpectedValue(joinType), 
		};
	}

	private static void WriteColumnTypeAndMethod(XmlWriter xmlWriter, QueryElementSymbol element)
	{
		QueryColumnMethodKind? enumPropertyValue = element.GetEnumPropertyValue<QueryColumnMethodKind>(PropertyKind.Method);
		if (enumPropertyValue.HasValue)
		{
			switch (enumPropertyValue.Value)
			{
			case QueryColumnMethodKind.Day:
			case QueryColumnMethodKind.Month:
			case QueryColumnMethodKind.Year:
				xmlWriter.WriteElementString("MethodType", "Date");
				xmlWriter.WriteElementString("ColumnType", NavTypeKind.Integer.ToString());
				xmlWriter.WriteElementString("DateMethod", enumPropertyValue.Value.ToString());
				break;
			case QueryColumnMethodKind.Count:
				xmlWriter.WriteElementString("MethodType", "Totals");
				xmlWriter.WriteElementString("ColumnType", NavTypeKind.Integer.ToString());
				xmlWriter.WriteElementString("TotalingMethod", enumPropertyValue.Value.ToString());
				break;
			case QueryColumnMethodKind.Sum:
			case QueryColumnMethodKind.Average:
			case QueryColumnMethodKind.Min:
			case QueryColumnMethodKind.Max:
				xmlWriter.WriteElementString("MethodType", "Totals");
				xmlWriter.WriteElementString("ColumnType", NavTypeKind.Decimal.ToString());
				xmlWriter.WriteElementString("TotalingMethod", enumPropertyValue.Value.ToString());
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(enumPropertyValue.Value);
			}
		}
		else
		{
			xmlWriter.WriteElementString("ColumnType", "UNDEF");
			xmlWriter.WriteElementString("MethodType", "None");
		}
	}

	private void WriteQueryProperties(XmlWriter xmlWriter)
	{
		WriteProperties(xmlWriter, Symbol, Symbol.Properties, shouldOutputDefaultValues: true);
	}

	private void WriteQueryBaseProperties(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
		ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
		ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "Name", Symbol.Name);
	}
}
