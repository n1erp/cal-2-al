using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract]
public class MetaQueryBuild : MetaQuery
{
	public MetaQueryBuild()
	{
	}

	public MetaQueryBuild(XmlNode root, int metadataAppGroupId, int languageAppGroupId)
		: base(root, metadataAppGroupId, languageAppGroupId)
	{
	}

	public MetaQueryBuild(MetaQuery constructFrom)
		: base(constructFrom)
	{
	}

	public static MetaQueryBuild ConstructBuildQuery(MetaQuery constructFrom)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(MetaQuery));
		using MemoryStream memoryStream = MemoryStreamManager.GetStream();
		xmlSerializer.Serialize(memoryStream, constructFrom);
		memoryStream.Position = 0L;
		using XmlReader xmlReader = XmlHelper.SafeXmlReader(memoryStream);
		return new MetaQueryBuild((MetaQuery)xmlSerializer.Deserialize(xmlReader));
	}

	public static MetaQuery ConstructMetaQuery(MetaQueryBuild constructFrom)
	{
		constructFrom.CalculateColumnIndexes();
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(MetaQueryBuild));
		using MemoryStream memoryStream = MemoryStreamManager.GetStream();
		xmlSerializer.Serialize(memoryStream, constructFrom);
		memoryStream.Position = 0L;
		using XmlReader xmlReader = XmlHelper.SafeXmlReader(memoryStream);
		return new MetaQuery((MetaQueryBuild)xmlSerializer.Deserialize(xmlReader));
	}

	public void ChangeDataColumnToFilterColumn(MetaQueryColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		column.FilterOnly = true;
		column.FieldTotalingMethod = AggregationType.None;
		MetaQueryOrderBy metaQueryOrderBy = OrderBys.Find((MetaQueryOrderBy o) => o.QueryColumnId == column.Id);
		if (metaQueryOrderBy != null)
		{
			OrderBys.Remove(metaQueryOrderBy);
		}
		CalculateColumnIndexes();
	}

	public MetaQueryColumn InsertColumn(int dataItemId, int fieldNo, string name, string caption, int columnId, NavType type, int position)
	{
		MetaQueryDataItem metaQueryDataItem = DataItems.Find((MetaQueryDataItem p) => p.Id == dataItemId) ?? throw new ArgumentException("DataItem not found");
		MetaQueryColumn metaQueryColumn = new MetaQueryColumn
		{
			FieldNo = fieldNo,
			Name = name,
			Caption = caption,
			ColumnType = type,
			Id = columnId,
			QueryColumnIndex = 0
		};
		metaQueryDataItem.QueryColumns.Insert(position, metaQueryColumn);
		CalculateColumnIndexes();
		return metaQueryColumn;
	}

	public void RemoveColumn(string name)
	{
		foreach (MetaQueryDataItem dataItem in DataItems)
		{
			MetaQueryColumn metaQueryColumn = dataItem.QueryColumns.Find((MetaQueryColumn prop) => prop.Name == name);
			if (metaQueryColumn != null)
			{
				dataItem.QueryColumns.Remove(metaQueryColumn);
				CalculateColumnIndexes();
				break;
			}
		}
	}

	public void RemoveColumn(int dataItemId, string name)
	{
		MetaQueryDataItem metaQueryDataItem = DataItems.Find((MetaQueryDataItem p) => p.Id == dataItemId);
		if (metaQueryDataItem == null)
		{
			throw new ArgumentException("DataItem not found");
		}
		MetaQueryColumn metaQueryColumn = metaQueryDataItem.QueryColumns.Find((MetaQueryColumn prop) => prop.Name == name);
		if (metaQueryColumn != null)
		{
			metaQueryDataItem.QueryColumns.Remove(metaQueryColumn);
			CalculateColumnIndexes();
		}
	}

	public void RemoveColumn(int dataItemId, int position)
	{
		MetaQueryDataItem metaQueryDataItem = DataItems.Find((MetaQueryDataItem p) => p.Id == dataItemId);
		if (metaQueryDataItem == null)
		{
			throw new ArgumentException("DataItem not found");
		}
		MetaQueryColumn metaQueryColumn = metaQueryDataItem.QueryColumns[position];
		if (metaQueryColumn != null)
		{
			metaQueryDataItem.QueryColumns.Remove(metaQueryColumn);
			CalculateColumnIndexes();
		}
	}

	private void CalculateColumnIndexes()
	{
		int num = 0;
		foreach (MetaQueryDataItem dataItem in DataItems)
		{
			foreach (MetaQueryColumn queryColumn in dataItem.QueryColumns)
			{
				if (queryColumn.FilterOnly)
				{
					queryColumn.QueryColumnIndex = -1;
				}
				else
				{
					queryColumn.QueryColumnIndex = num++;
				}
			}
		}
	}
}
