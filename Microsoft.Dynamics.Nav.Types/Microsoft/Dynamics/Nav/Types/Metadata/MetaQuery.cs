using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQuery
{
	[DataMember]
	private readonly List<MetaQueryDataItem> dataItems = new List<MetaQueryDataItem>();

	[DataMember]
	private List<MetaQueryColumnFilter> columnFilters = new List<MetaQueryColumnFilter>();

	[DataMember]
	private List<MetaQueryOrderBy> orderBys = new List<MetaQueryOrderBy>();

	private string captionML;

	private int metadataAppGroupId;

	private int languageAppGroupId;

	[DataMember]
	public int Id { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public long MetadataToken { get; set; }

	[DataMember]
	public string Caption { get; set; }

	[DataMember]
	public string APIPublisher { get; set; }

	[DataMember]
	public string APIGroup { get; set; }

	[DataMember]
	public string APIVersion { get; set; }

	[DataMember]
	public string EntitySetName { get; set; }

	[DataMember]
	public string EntityName { get; set; }

	[DataMember]
	public QueryReadState ReadState { get; set; }

	[DataMember]
	public QueryType QueryType { get; set; }

	[DataMember]
	public int TopNumberOfRowsToReturn { get; set; }

	[DataMember]
	public string QueryCategory { get; set; }

	public string CaptionML => captionML;

	public List<MetaQueryDataItem> DataItems => dataItems;

	public List<MetaQueryColumnFilter> ColumnFilters => columnFilters;

	public List<MetaQueryOrderBy> OrderBys => orderBys;

	[DataMember]
	public bool CustomQuery { get; set; }

	public int MetadataAppGroupId => metadataAppGroupId;

	public int LanguageAppGroupId => languageAppGroupId;

	public MetaQuery()
	{
	}

	public MetaQuery(XmlNode root, int metadataAppGroupId, int languageAppGroupId)
	{
		this.metadataAppGroupId = metadataAppGroupId;
		this.languageAppGroupId = languageAppGroupId;
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "NAME":
				Name = childNode.InnerText;
				Caption = Name;
				break;
			case "CAPTIONML":
				captionML = childNode.InnerText;
				break;
			case "APIPUBLISHER":
				APIPublisher = childNode.InnerText;
				break;
			case "APIGROUP":
				APIGroup = childNode.InnerText;
				break;
			case "ENTITYSETNAME":
				EntitySetName = childNode.InnerText;
				break;
			case "ENTITYNAME":
				EntityName = childNode.InnerText;
				break;
			case "DATAITEMSTRUCTURE":
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					string text = childNode2.Name.ToUpper(CultureInfo.InvariantCulture);
					if (text != null && text == "DATAITEM")
					{
						DataItems.Add(new MetaQueryDataItem(childNode2));
					}
				}
				break;
			case "DATAITEM":
				DataItems.Add(new MetaQueryDataItem(childNode));
				break;
			case "COLUMNFILTER":
				columnFilters.Add(new MetaQueryColumnFilter(childNode));
				break;
			case "ORDERBY":
				orderBys.Add(new MetaQueryOrderBy(childNode));
				break;
			case "TOPNUMBEROFROWS":
				TopNumberOfRowsToReturn = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "READSTATE":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "READUNCOMMITTED":
					ReadState = QueryReadState.ReadUncommitted;
					break;
				case "READSHARED":
					ReadState = QueryReadState.ReadShared;
					break;
				case "READEXCLUSIVE":
					ReadState = QueryReadState.ReadExclusive;
					break;
				default:
					throw new ArgumentException("ReadState");
				}
				break;
			case "QUERYTYPE":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "NORMAL":
					QueryType = QueryType.Normal;
					break;
				case "API":
					QueryType = QueryType.API;
					break;
				default:
					throw new ArgumentException("QueryType");
				}
				break;
			case "APIVERSION":
				APIVersion = childNode.InnerText ?? "beta";
				break;
			case "QUERYCATEGORY":
				QueryCategory = childNode.InnerText ?? string.Empty;
				break;
			}
		}
		if (APIPublisher == null)
		{
			APIPublisher = string.Empty;
		}
		if (APIGroup == null)
		{
			APIGroup = string.Empty;
		}
		MergeMultiLanguageTexts();
	}

	public MetaQuery(MetaQuery constructFrom)
	{
		Id = constructFrom.Id;
		Name = constructFrom.Name;
		Caption = constructFrom.Caption;
		APIPublisher = constructFrom.APIPublisher;
		APIGroup = constructFrom.APIGroup;
		APIVersion = constructFrom.APIVersion;
		EntitySetName = constructFrom.EntitySetName;
		EntityName = constructFrom.EntityName;
		MetadataToken = constructFrom.MetadataToken;
		ReadState = constructFrom.ReadState;
		QueryType = constructFrom.QueryType;
		TopNumberOfRowsToReturn = constructFrom.TopNumberOfRowsToReturn;
		CustomQuery = true;
		metadataAppGroupId = constructFrom.MetadataAppGroupId;
		languageAppGroupId = constructFrom.LanguageAppGroupId;
		dataItems = new List<MetaQueryDataItem>(constructFrom.DataItems);
		columnFilters = new List<MetaQueryColumnFilter>(constructFrom.columnFilters);
		orderBys = new List<MetaQueryOrderBy>(constructFrom.orderBys);
	}

	public void ResolveColumnTypes(Func<int, int, Func<int, NavType>> fieldTypeFromTableNoResolver)
	{
		foreach (MetaQueryDataItem dataItem in DataItems)
		{
			Func<int, NavType> fieldTypeFromFieldNoResolver = fieldTypeFromTableNoResolver(dataItem.TableNo, metadataAppGroupId);
			dataItem.ResolveColumnTypes(fieldTypeFromFieldNoResolver);
		}
	}

	public void ValidateColumns(Func<int, int, Action<int>> fieldObsolescenceFromTableNoValidator)
	{
		foreach (MetaQueryDataItem dataItem in DataItems)
		{
			Action<int> fieldObsolescenceFromFieldNoValidator = fieldObsolescenceFromTableNoValidator(dataItem.TableNo, metadataAppGroupId);
			dataItem.ValidateColumns(fieldObsolescenceFromFieldNoValidator);
		}
	}

	public void ApplyLanguage(string language, string primaryLanguage, Func<int, int, Func<int, MetaField>> fieldFromTableNoResolver)
	{
		if (captionML != null)
		{
			Caption = MultiLanguage.GetLanguageValue(language, primaryLanguage, captionML);
		}
		foreach (MetaQueryDataItem dataItem in DataItems)
		{
			Func<int, MetaField> fieldFromFieldNoResolver = fieldFromTableNoResolver(dataItem.TableNo, metadataAppGroupId);
			dataItem.ApplyLanguage(language, primaryLanguage, fieldFromFieldNoResolver);
		}
	}

	internal void MergeMultiLanguageTexts()
	{
		if (!LanguageProvider.Provider.HasDataFor(ObjectType.Query, Id, languageAppGroupId))
		{
			return;
		}
		captionML = MetaTable.MultiLanguageTextMerge(captionML, 0, ObjectType.Query, Id, -1, languageAppGroupId);
		foreach (MetaQueryDataItem dataItem in DataItems)
		{
			dataItem.MergeMultiLanguageTexts(this);
		}
	}
}
