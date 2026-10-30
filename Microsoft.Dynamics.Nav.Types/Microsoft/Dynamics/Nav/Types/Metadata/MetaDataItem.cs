using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaDataItem
{
	private List<MetaDataItemColumn> dataItemColumns = new List<MetaDataItemColumn>();

	private List<MetaDataItem> childDataItems;

	private int[] calcFields;

	public int Id { get; private set; }

	public int DataItemIndent { get; private set; }

	public int DataItemTable { get; private set; }

	public string DataItemTableView { get; private set; }

	public string DataItemLinkReference { get; private set; }

	public string DataItemLink { get; private set; }

	public int MaxIteration { get; private set; }

	public string DataItemVarName { get; private set; }

	public string ExternalizedName { get; private set; }

	public bool PrintOnlyIfDetail { get; private set; }

	public IReadOnlyList<MetaDataItemColumn> DataItemColumns => dataItemColumns;

	public IReadOnlyList<MetaDataItem> ChildDataItems => childDataItems;

	public MetaDataItem()
	{
	}

	public MetaDataItem(XmlNode dataItemNode, int parentId, Dictionary<string, string> uniqueDataItemNames, int languageAppGroupId)
	{
		foreach (XmlNode childNode in dataItemNode.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "DATAITEMINDENT":
			case "INDENTATION":
				DataItemIndent = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "DATAITEMTABLE":
				DataItemTable = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "FIELDFILTER":
			case "DATAITEMTABLEVIEW":
				DataItemTableView = childNode.InnerText;
				break;
			case "DATAITEMLINKREFERENCE":
				DataItemLinkReference = childNode.InnerText;
				break;
			case "DATAITEMLINK":
				DataItemLink = childNode.InnerText;
				break;
			case "CALCFIELDS":
				if (!string.IsNullOrEmpty(childNode.InnerText))
				{
					string[] array = childNode.InnerText.Split(',');
					calcFields = new int[array.Length];
					for (int i = 0; i < array.Length; i++)
					{
						int num = int.Parse(array[i].Substring(5, array[i].Length - 5), CultureInfo.InvariantCulture);
						calcFields[i] = num;
					}
				}
				break;
			case "MAXITERATION":
				MaxIteration = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "DATAITEMVARNAME":
			case "NAME":
			{
				DataItemVarName = childNode.InnerText;
				string externalizedName = MetaBase.ExternalizeName(DataItemVarName);
				ExternalizedName = MakeUniqueName(uniqueDataItemNames, externalizedName);
				break;
			}
			case "PRINTONLYIFDETAIL":
				PrintOnlyIfDetail = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "DATAITEMLINKTYPE":
			{
				DataItemLinkType dataItemLinkType = DataItemLinkType.LeftOuterJoin;
				if (string.Compare(childNode.InnerText, "INNER JOIN", StringComparison.OrdinalIgnoreCase) == 0)
				{
					dataItemLinkType = DataItemLinkType.InnerJoin;
				}
				PrintOnlyIfDetail = dataItemLinkType == DataItemLinkType.InnerJoin;
				break;
			}
			case "DATAITEMFIELD":
			{
				MetaDataItemColumn metaDataItemColumn = new MetaDataItemColumn(childNode, parentId, languageAppGroupId);
				if (!string.IsNullOrEmpty(metaDataItemColumn.Name))
				{
					dataItemColumns.Add(metaDataItemColumn);
				}
				break;
			}
			}
		}
	}

	private static string MakeUniqueName(Dictionary<string, string> uniqueDataItemNames, string externalizedName)
	{
		string value = null;
		int num = 1;
		string text = externalizedName;
		while (uniqueDataItemNames.TryGetValue(text, out value))
		{
			text = string.Format(CultureInfo.CurrentCulture, "{0}{1}", externalizedName, num++);
		}
		uniqueDataItemNames.Add(text, text);
		return text;
	}

	internal void BuildDataItemChildren(List<MetaDataItem> dataItems)
	{
		if (childDataItems != null)
		{
			return;
		}
		childDataItems = new List<MetaDataItem>();
		for (int num = dataItems.FindIndex((MetaDataItem dataItemVarName) => dataItemVarName.DataItemVarName == DataItemVarName) + 1; num < dataItems.Count && dataItems[num].DataItemIndent > DataItemIndent; num++)
		{
			if (dataItems[num].DataItemIndent > DataItemIndent && dataItems[num].DataItemIndent == DataItemIndent + 1)
			{
				childDataItems.Add(dataItems[num]);
			}
		}
	}

	public IReadOnlyCollection<int> CalcFields()
	{
		return calcFields;
	}
}
