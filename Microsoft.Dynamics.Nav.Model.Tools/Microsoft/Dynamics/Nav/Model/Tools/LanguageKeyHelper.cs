using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal static class LanguageKeyHelper
{
	internal const char LanguageStringKeyValueSeparatorChar = ':';

	internal const char LanguageKeySeparatorChar = '-';

	internal static readonly string LanguageKeySeparatorString = '-'.ToString(CultureInfo.InvariantCulture);

	private static readonly Lazy<Dictionary<int, PropertyTypeInfo>> multiLanguageProperties = new Lazy<Dictionary<int, PropertyTypeInfo>>(() => PropertyTypeInfo.Definitions.Where((PropertyTypeInfo d) => d.DataType == DataTypes.MLText && d.AttributeId != 0).ToDictionary((PropertyTypeInfo d) => d.AttributeId), isThreadSafe: true);

	internal static IElement FindElementFromLanguageKey(IElement element, string key)
	{
		return FindElementFromLanguageKey(element, key.Split('-'));
	}

	internal static IElement FindElementFromLanguageKey(IElement element, string[] segments)
	{
		for (int i = 1; i < segments.Length; i++)
		{
			if (element == null)
			{
				break;
			}
			string idString = segments[i].Substring(1);
			if (!int.TryParse(idString, out var id))
			{
				id = 0;
			}
			Guid result;
			switch (segments[i][0])
			{
			case 'P':
			{
				if (i == segments.Length - 3)
				{
					return element;
				}
				ElementRelation elementRelation = element.ElementTypeInfo.Relations.Values.SingleOrDefault((ElementRelation er) => er.AttributeId == id);
				if (elementRelation != null && elementRelation.MultiplicityIsSingle)
				{
					element = element.GetFirstSubElement(elementRelation.ChildType);
				}
				break;
			}
			case 'C':
				switch (element.ElementTypeInfo.ElementType)
				{
				case ElementType.Page:
				case ElementType.RequestPage:
					element = element.GetSubElements(ElementType.Control).SingleOrDefault((IElement e) => e.Id == id);
					break;
				case ElementType.Report:
					element = element.GetSubElements(ElementType.DataItem).SingleOrDefault((IElement e) => e.Id == id);
					break;
				}
				break;
			case 'D':
				element = element.GetSubElements(ElementType.DataItem).SingleOrDefault((IElement e) => e.Id == id);
				break;
			case 'F':
				element = element.GetSubElements(ElementType.Field).SingleOrDefault((IElement e) => e.Id == id);
				break;
			case 'G':
				element = element.GetSubElements(ElementType.Action).SingleOrDefault((IElement e) => e.Id == id);
				break;
			case 'I':
				if (!Guid.TryParse(idString, out result))
				{
					throw new ModelToolException(NonLocalizable.InvalidLanguageKeyFormat, string.Join(LanguageKeySeparatorString, segments));
				}
				idString = result.ToString("B").ToUpperInvariant();
				element = element.GetSubElements(ElementType.MenuNode).SingleOrDefault((IElement e) => e.GetStringProperty(PropertyType.MenuNodeId).ToUpperInvariant() == idString);
				break;
			case 'J':
				if (!Guid.TryParse(idString, out result))
				{
					throw new ModelToolException(NonLocalizable.InvalidLanguageKeyFormat, string.Join(LanguageKeySeparatorString, segments));
				}
				idString = result.ToString("B").ToUpperInvariant();
				element = element.GetSubElements(ElementType.XmlPortElement).SingleOrDefault((IElement e) => e.GetStringProperty(PropertyType.ElementId).ToUpperInvariant() == idString);
				break;
			case 'N':
				element = element.GetSubElements(ElementType.RequestPage).SingleOrDefault();
				break;
			case 'Q':
				element = element.GetSubElements(ElementType.Variable).SingleOrDefault((IElement e) => e.Id == id);
				break;
			case 'T':
				element = element.GetSubElements(ElementType.Label).SingleOrDefault((IElement e) => e.Id == id);
				break;
			case 'U':
				element = element.GetSubElements(ElementType.Procedure).SingleOrDefault((IElement e) => e.Id == id);
				break;
			case 'E':
				element = element.GetSubElements(ElementType.Event).SingleOrDefault((IElement e) => e.Id == id);
				break;
			default:
				throw new ModelToolException(NonLocalizable.InvalidLanguageKeyFormat, string.Join(LanguageKeySeparatorString, segments));
			}
		}
		return null;
	}

	internal static bool ValidateKeySegments(string[] segments, out PropertyType propertyType, out int lcid, out int length)
	{
		propertyType = PropertyType.Unknown;
		lcid = 0;
		length = 0;
		if (segments.Length < 4 || segments[segments.Length - 1][0] != 'L' || segments[segments.Length - 2][0] != 'A' || segments[segments.Length - 3][0] != 'P')
		{
			return false;
		}
		if (int.TryParse(segments[segments.Length - 3].Substring(1), out var result) && multiLanguageProperties.Value.TryGetValue(result, out var value))
		{
			propertyType = value.PropertyType;
		}
		lcid = int.Parse(segments[segments.Length - 2].Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture);
		length = int.Parse(segments[segments.Length - 1].Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture);
		return true;
	}

	internal static ElementType GetObjectElementTypeFromChar(char c)
	{
		return c switch
		{
			'C' => ElementType.Codeunit, 
			'R' => ElementType.Report, 
			'T' => ElementType.Table, 
			'X' => ElementType.XmlPort, 
			'M' => ElementType.MenuSuite, 
			'Q' => ElementType.Query, 
			'N' => ElementType.Page, 
			_ => ElementType.Unknown, 
		};
	}

	internal static string CreateLanguageString(IElement element, PropertyType propertyType, CultureInfo culture, int length, string value)
	{
		return CreateLanguageString(element, propertyType, ModelTextHelper.CultureToLcid(culture), length, value);
	}

	internal static string CreateLanguageString(IElement element, PropertyType propertyType, int lcid, int length, string value)
	{
		StringBuilder stringBuilder = CreateLanguageKey(element, propertyType, lcid, length);
		stringBuilder.Append(':');
		stringBuilder.Append(value);
		return stringBuilder.ToString();
	}

	internal static string CreateLanguageString(string key, string value)
	{
		return key + ":" + value;
	}

	internal static StringBuilder CreateLanguageKey(IElement element, PropertyType propertyType, CultureInfo culture, int length)
	{
		return CreateLanguageKey(element, propertyType, ModelTextHelper.CultureToLcid(culture), length);
	}

	internal static StringBuilder CreateLanguageKey(IElement element, PropertyType propertyType, int lcid, int length)
	{
		StringBuilder languageElementPath = GetLanguageElementPath(element);
		languageElementPath.Append('P');
		languageElementPath.Append(MapPropertyTypeToAttributeId(propertyType));
		languageElementPath.Append("-A");
		languageElementPath.Append(lcid);
		languageElementPath.Append("-L");
		languageElementPath.Append(length);
		return languageElementPath;
	}

	internal static int MapPropertyTypeToAttributeId(PropertyType propertyType)
	{
		return multiLanguageProperties.Value.Values.FirstOrDefault((PropertyTypeInfo pi) => pi.PropertyType == propertyType)?.AttributeId ?? 0;
	}

	internal static void SplitLanguageStringIntoKeyValue(string translate, out string key, out string value)
	{
		int num = translate.IndexOf(':');
		if (num < 0)
		{
			throw new ModelToolException(NonLocalizable.InvalidLanguageKeyFormat, translate);
		}
		key = translate.Substring(0, num);
		value = translate.Substring(num + 1);
	}

	internal static string[] SplitKeyIntoSegments(string key)
	{
		return key.Split('-');
	}

	internal static void GetObjectTypeAndIdFromKey(string key, out ElementType objectType, out int id)
	{
		objectType = GetObjectElementTypeFromChar(key[0]);
		int num = key.IndexOf('-');
		key = ((num > 0) ? key.Substring(1, num - 1) : key.Substring(1));
		if (objectType == ElementType.Unknown || !int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
		{
			throw new ModelToolException(NonLocalizable.InvalidLanguageKeyFormat, key);
		}
	}

	internal static bool GetInfoFromLanguageKey(string key, out ElementType objectType, out int id, out int language)
	{
		objectType = ElementType.Unknown;
		language = (id = 0);
		string[] array = SplitKeyIntoSegments(key);
		if (array.Length < 4)
		{
			throw new ModelToolException(NonLocalizable.InvalidLanguageKeyFormat, key);
		}
		if (array[array.Length - 2][0] != 'A')
		{
			return false;
		}
		if (!int.TryParse(array[array.Length - 2].Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out language))
		{
			return false;
		}
		GetObjectTypeAndIdFromKey(key, out objectType, out id);
		return true;
	}

	private static StringBuilder GetLanguageElementPath(IElement element, StringBuilder sb = null)
	{
		if (sb == null)
		{
			sb = new StringBuilder();
		}
		if (element.Parent != null)
		{
			sb = GetLanguageElementPath(element.Parent, sb);
		}
		int attributeId;
		if (element.Parent != null && (attributeId = element.Parent.ElementTypeInfo.Relations.GetValue(element.ElementTypeInfo.ElementType).AttributeId) != 0)
		{
			sb.Append('P');
			sb.Append(attributeId);
			sb.Append('-');
		}
		if (element.ElementTypeInfo.Category != CategoryType.Trigger)
		{
			string value = element.Id.ToString(CultureInfo.InvariantCulture);
			switch (element.ElementTypeInfo.ElementType)
			{
			case ElementType.Table:
				sb.Append('T');
				break;
			case ElementType.Field:
				sb.Append('F');
				break;
			case ElementType.RequestPage:
				sb.Append('N');
				if (element.Parent == null)
				{
					throw new InvalidOperationException(element.ElementKey + " " + element.ElementTypeInfo.ElementType);
				}
				value = ((element.Parent.ElementTypeInfo.ElementType == ElementType.Report) ? "2" : "1");
				break;
			case ElementType.Page:
				sb.Append('N');
				break;
			case ElementType.Codeunit:
			case ElementType.Control:
			case ElementType.QueryElement:
				sb.Append('C');
				break;
			case ElementType.Action:
				sb.Append('G');
				break;
			case ElementType.Variable:
			case ElementType.Query:
				sb.Append('Q');
				break;
			case ElementType.XmlPortElement:
				value = Guid.Parse(element.GetStringProperty(PropertyType.ElementId)).ToString("N").ToUpper(CultureInfo.InvariantCulture);
				sb.Append('J');
				break;
			case ElementType.Procedure:
				sb.Append('U');
				break;
			case ElementType.Report:
				sb.Append('R');
				break;
			case ElementType.DataItem:
				sb.Append((element.GetStringProperty(PropertyType.ReportDataItemDataType) == "Column") ? 'C' : 'D');
				break;
			case ElementType.Label:
				sb.Append('T');
				break;
			case ElementType.XmlPort:
				sb.Append('X');
				break;
			case ElementType.MenuSuite:
				sb.Append('M');
				break;
			case ElementType.MenuNode:
				value = Guid.Parse(element.GetStringProperty(PropertyType.MenuNodeId)).ToString("N").ToUpper(CultureInfo.InvariantCulture);
				sb.Append('I');
				break;
			case ElementType.Event:
				sb.Append('E');
				break;
			default:
				throw new InvalidOperationException(element.ElementKey + " " + element.ElementTypeInfo.ElementType);
			}
			sb.Append(value);
			sb.Append(LanguageKeySeparatorString);
		}
		return sb;
	}
}
