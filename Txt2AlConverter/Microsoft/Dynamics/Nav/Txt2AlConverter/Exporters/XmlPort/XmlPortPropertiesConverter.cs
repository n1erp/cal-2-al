using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.XmlPort;

internal class XmlPortPropertiesConverter : PropertiesConverter
{
	protected override Dictionary<string, string> RenamedPropertyValueEnums { get; } = new Dictionary<string, string>
	{
		{
			"C/SIDE Format/Evaluate",
			FormatEvaluateKind.Legacy.ToString()
		},
		{
			"XML Format/Evaluate",
			FormatEvaluateKind.Xml.ToString()
		},
		{
			"UTF-8",
			EncodingKind.UTF8.ToString()
		},
		{
			"UTF-16",
			EncodingKind.UTF16.ToString()
		},
		{
			"Variable Text",
			FormatKind.VariableText.ToString()
		},
		{
			"\"Variable Text\"",
			FormatKind.VariableText.ToString()
		},
		{
			"Fixed Text",
			FormatKind.FixedText.ToString()
		}
	};

	protected override Dictionary<string, PropertyKind> RenamedProperties { get; } = new Dictionary<string, PropertyKind>(StringComparer.OrdinalIgnoreCase)
	{
		{
			"Temporary",
			PropertyKind.UseTemporary
		},
		{
			"ReqFilterFields",
			PropertyKind.RequestFilterFields
		},
		{
			"ReqFilterHeading",
			PropertyKind.RequestFilterHeading
		},
		{
			"ReqFilterHeadingML",
			PropertyKind.RequestFilterHeadingML
		},
		{
			"VariableName",
			PropertyKind.XmlName
		},
		{
			"SourceTable",
			PropertyKind.XmlName
		}
	};

	public override string GetPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string stringProperty = owner.GetStringProperty(propertyType);
		switch (propertyType)
		{
		case PropertyType.DataItemTable:
		case PropertyType.SourceTable:
			return GetSourceTable(owner);
		case PropertyType.CalcFields:
		case PropertyType.LinkFields:
		case PropertyType.RequestFilterFields:
		case PropertyType.SourceTableView:
			return PropertiesConverter.FormatMultilinePropertyValue(stringProperty, needsSingleQuoting: false, state);
		case PropertyType.VariableName:
			return owner.GetStringProperty(PropertyType.Name).QuoteLiteral();
		case PropertyType.LinkTable:
			return AlUtilities.QuoteIdentifierIfNeeded(stringProperty);
		case PropertyType.FieldValidate:
			return GetFieldValidate(stringProperty);
		case PropertyType.Namespaces:
			return GetNamespaces(stringProperty);
		case PropertyType.Format:
			return GetFormat(stringProperty);
		default:
			return base.GetPropertyValue(owner, propertyInfo, state);
		}
	}

	public override bool IsPropertyIgnored(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.ElementId:
		case PropertyType.SourceField:
		case PropertyType.XmlPortElementDataType:
		case PropertyType.NodeType:
		case PropertyType.SourceType:
			return true;
		case PropertyType.VariableName:
			return IsSourceTableUsedForXmlName(owner);
		default:
			return base.IsPropertyIgnored(owner, propertyInfo);
		}
	}

	private static bool IsSourceTableUsedForXmlName(IElement owner)
	{
		if (owner.ElementTypeInfo.ElementType == ElementType.XmlPortElement)
		{
			return !string.IsNullOrEmpty(owner.GetStringProperty(PropertyType.SourceTable));
		}
		return false;
	}

	private static string GetNamespaces(string value)
	{
		IEnumerable<string> values = value.Replace("\r\n", "").Split(';').Select((string pair) =>
		{
			Tuple<string, string> tuple = pair.SplitOnFirstOccurrence("=");
			string text = tuple.Item1.QuoteIdentifierIfNeeded();
			if (string.IsNullOrEmpty(text))
			{
				text = "\"\"";
			}
			string text2 = tuple.Item2.QuoteLiteral();
			return text + "=" + text2;
		});
		return string.Join(",", values);
	}

	private static string GetFieldValidate(string value)
	{
		if (string.Equals(value, "true"))
		{
			value = "yes";
		}
		if (string.Equals(value, "false"))
		{
			value = "no";
		}
		return value;
	}

	private static string GetSourceTable(IElement owner)
	{
		return owner.GetStringProperty(PropertyType.Name).QuoteLiteral();
	}

	private string GetFormat(string value)
	{
		if (RenamedPropertyValueEnums.ContainsKey(value))
		{
			return RenamedPropertyValueEnums[value];
		}
		return value;
	}
}
