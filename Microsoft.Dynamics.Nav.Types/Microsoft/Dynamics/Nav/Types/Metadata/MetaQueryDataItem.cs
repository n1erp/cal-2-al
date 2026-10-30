using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryDataItem
{
	[DataMember]
	private List<MetaQueryColumn> queryColumns = new List<MetaQueryColumn>();

	[DataMember]
	private List<MetaQueryDataItemLink> dataItemLinks = new List<MetaQueryDataItemLink>();

	[DataMember]
	private List<MetaQueryFieldFilter> fieldFilters = new List<MetaQueryFieldFilter>();

	[DataMember]
	public string DataItemName { get; set; }

	[DataMember]
	public int TableNo { get; set; }

	[DataMember]
	public int Id { get; set; }

	public List<MetaQueryColumn> QueryColumns => queryColumns;

	public List<MetaQueryDataItemLink> DataItemLinks => dataItemLinks;

	public List<MetaQueryFieldFilter> FieldFilters => fieldFilters;

	[DataMember]
	public DataItemLinkType DataItemLinkType { get; set; }

	public MetaQueryDataItem()
	{
	}

	public MetaQueryDataItem(XmlNode root)
	{
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "NAME":
				DataItemName = childNode.InnerText;
				break;
			case "DATAITEMTABLE":
				TableNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "DATAITEMTABLEFILTER":
				fieldFilters.Add(new MetaQueryFieldFilter(childNode));
				break;
			case "DATAITEMLINKTYPE":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "INNER JOIN":
					DataItemLinkType = DataItemLinkType.InnerJoin;
					break;
				case "LEFT OUTER JOIN":
					DataItemLinkType = DataItemLinkType.LeftOuterJoin;
					break;
				case "RIGHT OUTER JOIN":
					DataItemLinkType = DataItemLinkType.RightOuterJoin;
					break;
				case "FULL OUTER JOIN":
					DataItemLinkType = DataItemLinkType.FullOuterJoin;
					break;
				case "CROSS JOIN":
					DataItemLinkType = DataItemLinkType.CrossJoin;
					break;
				default:
					throw new ArgumentException("DataItemLinkType");
				}
				break;
			case "DATAITEMLINK":
				dataItemLinks.Add(new MetaQueryDataItemLink(childNode));
				break;
			case "QUERYCOLUMN":
				queryColumns.Add(new MetaQueryColumn(childNode));
				break;
			}
		}
	}

	public void ResolveColumnTypes(Func<int, NavType> fieldTypeFromFieldNoResolver)
	{
		foreach (MetaQueryColumn queryColumn in QueryColumns)
		{
			if (queryColumn.ColumnType == NavType.None)
			{
				NavType navType = fieldTypeFromFieldNoResolver(queryColumn.FieldNo);
				if (navType == NavType.None)
				{
					throw new NavMetadataNotFoundException(ObjectType.FieldNumber, queryColumn.FieldNo);
				}
				queryColumn.ColumnType = navType;
			}
		}
	}

	public void ApplyLanguage(string language, string primaryLanguage, Func<int, MetaField> fieldFromFieldNoResolver)
	{
		foreach (MetaQueryColumn queryColumn in QueryColumns)
		{
			if (queryColumn.CaptionML != null)
			{
				queryColumn.Caption = MultiLanguage.GetLanguageValue(language, primaryLanguage, queryColumn.CaptionML);
			}
			else if (queryColumn.FieldTotalingMethod == AggregationType.None)
			{
				MetaField metaField = fieldFromFieldNoResolver(queryColumn.FieldNo);
				if (metaField == null)
				{
					throw new NavMetadataNotFoundException(ObjectType.FieldNumber, queryColumn.FieldNo);
				}
				queryColumn.Caption = metaField.Caption;
			}
		}
	}

	public void MergeMultiLanguageTexts(MetaQuery parent)
	{
		foreach (MetaQueryColumn queryColumn in QueryColumns)
		{
			queryColumn.CaptionML = MetaTable.MultiLanguageTextMerge(queryColumn.CaptionML, 0, ObjectType.Query, parent.Id, queryColumn.Id, parent.LanguageAppGroupId);
		}
	}

	internal void ValidateColumns(Action<int> fieldObsolescenceFromFieldNoValidator)
	{
		foreach (MetaQueryColumn queryColumn in QueryColumns)
		{
			fieldObsolescenceFromFieldNoValidator(queryColumn.FieldNo);
		}
	}
}
