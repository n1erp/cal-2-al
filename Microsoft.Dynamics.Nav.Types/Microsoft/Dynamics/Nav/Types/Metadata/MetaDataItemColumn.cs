using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaDataItemColumn
{
	private KeyValuePair<int, string>[] optionCaptionML;

	public bool AutoCalcField { get; private set; }

	public bool IsDecimalFormatter { get; private set; }

	public string Name { get; private set; }

	public string DecimalFormatter { get; private set; }

	public int Id { get; private set; }

	public NavType FieldType { get; private set; }

	public int FieldNo { get; private set; }

	public string SourceExpression { get; private set; }

	public string OptionString { get; private set; }

	[IgnoreDataMember]
	[XmlIgnore]
	public IReadOnlyCollection<KeyValuePair<int, string>> OptionCaptionML => optionCaptionML;

	public int DataSetIndex { get; set; }

	public MetaDataItemColumn()
	{
	}

	public MetaDataItemColumn(XmlNode dataItemFieldNode, int parentId, int languageAppGroupId)
	{
		if (dataItemFieldNode == null)
		{
			throw new ArgumentNullException("dataItemFieldNode");
		}
		List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
		foreach (XmlNode childNode in dataItemFieldNode.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "FRIENDLYFIELDNAME":
			case "NAME":
				Name = childNode.InnerText;
				break;
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "FIELDTYPE":
				if (string.Equals(childNode.InnerText, DataType.Binary.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					FieldType = NavType.ClrByteArray;
				}
				else
				{
					FieldType = (NavType)Enum.Parse(typeof(NavType), childNode.InnerText);
				}
				break;
			case "FIELDNO":
				FieldNo = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "AUTOCALCFIELD":
				AutoCalcField = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "ISDECIMALFORMATTER":
				IsDecimalFormatter = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "DECIMALFORMATTER":
				DecimalFormatter = childNode.InnerText;
				break;
			case "SOURCEEXPR":
				SourceExpression = childNode.InnerText.Replace("\"", "");
				break;
			case "OPTIONSTRING":
				OptionString = childNode.InnerText;
				break;
			case "OPTIONCAPTIONML":
				if (!childNode.HasChildNodes)
				{
					break;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Attributes != null && childNode2.Attributes.Count > 0)
					{
						int key = int.Parse(childNode2.Attributes["Id"].InnerText, CultureInfo.InvariantCulture);
						string innerText = childNode2.InnerText;
						list.Add(new KeyValuePair<int, string>(key, innerText));
					}
				}
				break;
			}
		}
		list = MetaReport.MultiLanguageDictionaryMerge(list, 2, ObjectType.Report, parentId, Id, languageAppGroupId);
		optionCaptionML = list.ToArray();
	}
}
