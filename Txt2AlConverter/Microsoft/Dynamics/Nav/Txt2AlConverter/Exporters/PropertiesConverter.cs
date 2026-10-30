using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

internal class PropertiesConverter : IPropertiesConverter
{
	private readonly HashSet<string> nameSet = new HashSet<string> { "" };

	protected virtual Dictionary<string, PropertyKind> RenamedProperties { get; } = new Dictionary<string, PropertyKind>();

	protected virtual Dictionary<string, string> RenamedPropertyValueEnums { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public virtual void WriteMandatoryProperties(IElement owner, IAlWriter writer, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo[] declaredProperties)
	{
	}

	public string GetPropertyName(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		string name = propertyInfo.Name;
		PropertyKind value;
		if (AlUtilities.IsMultilanguageProperty(owner, propertyInfo.PropertyType))
		{
			name = name.Substring(0, name.Length - 2);
			if (RenamedProperties.TryGetValue(name, out value))
			{
				name = value.ToString();
			}
			return name;
		}
		if (string.Equals(propertyInfo.Name, "Layout", StringComparison.OrdinalIgnoreCase) && owner.TryGetStringProperty(PropertyType.GroupType, out var value2) && string.Equals(value2, "CueGroup", StringComparison.OrdinalIgnoreCase))
		{
			return "CueGroupLayout";
		}
		if (RenamedProperties.TryGetValue(name, out value))
		{
			return value.ToString();
		}
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		foreach (char item in name.Where(char.IsLetter))
		{
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}

	public virtual string GetPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		if (AlUtilities.IsMultilanguageProperty(owner, propertyInfo.PropertyType))
		{
			var (text, text2) = TranslationUtilities.GetLabelSyntaxFromMultilanguage(owner.GetStringProperty(propertyType));
			return text + text2;
		}
		switch (propertyType)
		{
		case PropertyType.ControlAddIn:
		{
			owner.TryGetStringProperty(PropertyType.ControlAddIn, out var value);
			if (!string.IsNullOrEmpty(value))
			{
				return value.UnquoteLiteral().Split(';')[0].QuoteIdentifier();
			}
			return "\"\"";
		}
		case PropertyType.SourceType:
			return owner.GetStringProperty(PropertyType.SourceType).ToLowerInvariant() + "type";
		case PropertyType.Name:
			return AlUtilities.QuoteIdentifierIfNeeded(AlUtilities.GetElementName(owner, nameSet));
		case PropertyType.ApplicationArea:
			return GetApplicationArea(owner.GetStringProperty(propertyType));
		case PropertyType.APIVersion:
			return ConvertAPIVersion(owner.GetStringProperty(propertyType));
		default:
			return GetEscapedPropertyValue(owner, propertyInfo, state);
		}
	}

	public virtual bool IsPropertyIgnored(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.Id:
		case PropertyType.Name:
		case PropertyType.Level:
		case PropertyType.Indentation:
		case PropertyType.ControlAddIn:
		case PropertyType.SourceType:
		case PropertyType.Title:
		case PropertyType.ImplicitType:
			return true;
		default:
			return false;
		}
	}

	public virtual bool TryCommentOutProperty(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, out string commentPart)
	{
		commentPart = string.Empty;
		if (AlExporterLists.TemporarilyCommentedOutProperties.Contains(propertyInfo.PropertyType))
		{
			commentPart = "//This property is currently not supported";
			return true;
		}
		return false;
	}

	internal static bool TryCommentOutPropertyOnElementOfTypeApi(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, ElementType elementType, out string commentPart)
	{
		commentPart = string.Empty;
		if (owner.ElementTypeInfo.ElementType != elementType)
		{
			if (elementType == ElementType.Page && owner.ElementTypeInfo.ElementType == ElementType.Control && owner.Parent.ElementTypeInfo.ElementType == ElementType.Page && IsElementOfTypeApi(owner.Parent))
			{
				return false;
			}
			commentPart = $"//The property '{propertyInfo.PropertyType}' can only be set on a {elementType} where the property '{elementType}Type' is set to 'API'";
			return true;
		}
		if (!IsElementOfTypeApi(owner))
		{
			commentPart = $"//The property '{propertyInfo.PropertyType}' can only be set if the property '{elementType}Type' is set to 'API'";
			return true;
		}
		return false;
	}

	protected static string GetApplicationArea(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		return (from s in value.Split(',')
			select s.Substring(1)).Aggregate((string agg, string next) => agg + "," + AlUtilities.QuoteIdentifierIfNeeded(next));
	}

	protected static string ConvertValue(string value)
	{
		if (AlUtilities.IsValueBoolean(value))
		{
			return ConvertBooleanValue(value);
		}
		return value;
	}

	protected static string ConvertBooleanValue(string value)
	{
		value = value.ToLowerInvariant();
		switch (value)
		{
		case "yes":
		case "true":
			return "true";
		case "no":
		case "false":
			return "false";
		default:
			return value;
		}
	}

	protected static bool IsEmptyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		string stringProperty = owner.GetStringProperty(propertyInfo.PropertyType);
		if (string.IsNullOrEmpty(stringProperty) || string.IsNullOrWhiteSpace(stringProperty))
		{
			return true;
		}
		return false;
	}

	protected static bool IsElementOfTypeApi(IElement owner)
	{
		string value = string.Empty;
		switch (owner.ElementTypeInfo.ElementType)
		{
		case ElementType.Page:
			owner.TryGetStringProperty(PropertyType.PageType, out value);
			break;
		case ElementType.Query:
			owner.TryGetStringProperty(PropertyType.QueryType, out value);
			break;
		}
		return string.Equals(value, "API", StringComparison.OrdinalIgnoreCase);
	}

	protected static string FormatMultilinePropertyValue(string value, bool needsSingleQuoting, IAlWriterState state)
	{
		int num = value.IndexOf('\n');
		if (num >= 0)
		{
			return GetMultilineEmbracedProperty(num, value, needsSingleQuoting, state);
		}
		return value;
	}

	protected virtual string GetEscapedPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		return GetEscapedPropertyValue(owner, propertyInfo, state, RenamedPropertyValueEnums);
	}

	internal static string GetEscapedPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state, Dictionary<string, string> renamedPropeties)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string text = owner.GetStringProperty(propertyType);
		bool flag = propertyInfo.DataType == DataTypes.Boolean;
		bool needsSingleQuoting = false;
		if (string.IsNullOrEmpty(text))
		{
			if (propertyInfo.DataType == DataTypes.Text && propertyType != PropertyType.Permissions)
			{
				return AlUtilities.SingleQuote(string.Empty);
			}
			return string.Empty;
		}
		if (IsPropertyTemporarilyComentedOut(propertyType))
		{
			text = text.Replace(Environment.NewLine, "");
		}
		if (propertyInfo.DataType == DataTypes.Text)
		{
			needsSingleQuoting = !AlExporterLists.IdentifierTextProperties.Contains(propertyType);
		}
		if (propertyInfo.DataType == DataTypes.Option && renamedPropeties.ContainsKey(text.UnquoteIdentifier()))
		{
			return renamedPropeties[text.UnquoteIdentifier()];
		}
		if (AlExporterLists.CommaSeperatedProperties.Contains(propertyType))
		{
			return string.Join(",", AlUtilities.SplitCommaSeperatedIdentifiers(text).Select(AlUtilities.QuoteIdentifierIfNeeded));
		}
		if (propertyInfo.DataType == DataTypes.MLText)
		{
			return FormatMultiLanguageText(text, state);
		}
		int num = text.IndexOf('\n');
		if (num >= 0)
		{
			return GetMultilineEmbracedProperty(num, text, needsSingleQuoting, state);
		}
		if (flag)
		{
			return ConvertBooleanValue(text);
		}
		return AlUtilities.QuoteStringLiteralIfNeeded(text, needsSingleQuoting);
	}

	private static bool IsPropertyTemporarilyComentedOut(PropertyType propertyType)
	{
		if (AlExporterLists.TemporarilyCommentedOutProperties.Contains(propertyType))
		{
			return true;
		}
		return false;
	}

	private static string ConvertAPIVersion(string value)
	{
		return "'" + value.Split(',').Join("', '") + "'";
	}

	private static string GetMultilineEmbracedProperty(int indexOfNewLine, string value, bool needsSingleQuoting, IAlWriterState state)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		string value2 = CreateIndentForMultilinePropertyValue(state);
		while (indexOfNewLine >= 0)
		{
			string text = value.Substring(num, indexOfNewLine - num - ((num + indexOfNewLine > 0 && value[indexOfNewLine - 1] == '\r') ? 1 : 0));
			if (needsSingleQuoting)
			{
				text += "\\";
			}
			stringBuilder.AppendLine(AlUtilities.QuoteStringLiteralIfNeeded(text, needsSingleQuoting));
			stringBuilder.Append(value2);
			num = indexOfNewLine + 1;
			indexOfNewLine = value.IndexOf('\n', num);
		}
		stringBuilder.Append(AlUtilities.QuoteStringLiteralIfNeeded(value.Substring(num), needsSingleQuoting));
		return stringBuilder.ToString();
	}

	private static string CreateIndentForMultilinePropertyValue(IAlWriterState state)
	{
		return new string(' ', state.LinePosition);
	}

	private static string FormatMultiLanguageText(string value, IAlWriterState state)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value2 = CreateIndentForMultilinePropertyValue(state);
		value = value.UnquoteIdentifier();
		bool flag = true;
		foreach (string item in ModelTextHelper.MLTextFormatter(ModelTextHelper.MLTextSplitter(value), 0, forceUseSingleQuotes: true))
		{
			if (!flag)
			{
				stringBuilder.AppendLine(",");
				stringBuilder.Append(value2);
			}
			stringBuilder.Append(AlUtilities.ConvertMlCommentFormat(item));
			flag = false;
		}
		return stringBuilder.ToString();
	}

	internal static string FormatMultiLineProperty(string value, IAlWriterState state)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string value2 = CreateIndentForMultilinePropertyValue(state);
		bool flag = true;
		string newLine = Environment.NewLine;
		string[] array = value.Split(new string[1] { newLine }, StringSplitOptions.None);
		foreach (string value3 in array)
		{
			if (!flag)
			{
				stringBuilder.Append(value2);
			}
			stringBuilder.AppendLine(value3);
			flag = false;
		}
		stringBuilder.Length -= newLine.Length;
		return stringBuilder.ToString();
	}
}
