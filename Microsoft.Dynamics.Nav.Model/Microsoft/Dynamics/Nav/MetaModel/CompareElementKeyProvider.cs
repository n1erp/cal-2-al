using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class CompareElementKeyProvider : IElementKeyProvider
{
	private const char SegmentSeparatorChar = '/';

	private const char TypeAndIdSeparatorChar = '=';

	private static readonly string SegmentSeparatorString = '/'.ToString(CultureInfo.InvariantCulture);

	private static readonly char[] equalsOrSlash = new char[2] { '=', '/' };

	private static readonly char[] charsRequiringQuotes = new char[3] { '"', '/', '=' };

	public string CreateElementKeySegmentString(IElement element)
	{
		string text = null;
		string name = element.ElementTypeInfo.Name;
		if (element.Id != 0)
		{
			text = element.Id.ToString(CultureInfo.InvariantCulture);
		}
		else if (element.ElementTypeInfo.IdProperty != PropertyType.Id)
		{
			switch (element.ElementTypeInfo.ElementType)
			{
			case ElementType.MenuNode:
				text = element.GetStringProperty(PropertyType.MenuNodeId);
				break;
			case ElementType.XmlPortElement:
				text = element.GetStringProperty(PropertyType.ElementId);
				break;
			default:
				if (element.ElementTypeInfo.NameProperty != PropertyType.Unknown && element.GetPropertySetState(element.ElementTypeInfo.NameProperty))
				{
					text = element.GetStringProperty(element.ElementTypeInfo.NameProperty);
				}
				break;
			}
		}
		if (element.ElementTypeInfo.ElementType == ElementType.Event)
		{
			text = text + "," + element.GetStringProperty(PropertyType.EventSource) + "," + element.GetStringProperty(PropertyType.EventSourceId);
		}
		if (IsIdentifiedByName(element.ElementTypeInfo.ElementType))
		{
			text = element.Name;
		}
		if (!string.IsNullOrEmpty(text))
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}={1}", name, QuoteIfNeeded(text));
		}
		return name;
	}

	public IEnumerable<IElementKeySegment> GetElementKeySegments(string elementKey)
	{
		int idx = elementKey.Length - 1;
		while (idx > 0)
		{
			string id = null;
			string key;
			if (elementKey[idx] == '"')
			{
				int startIndex = idx - 1;
				while ((startIndex = elementKey.LastIndexOf('"', startIndex)) > 0 && elementKey[startIndex - 1] == '"')
				{
					startIndex -= 2;
				}
				id = elementKey.Substring(startIndex + 1, idx - startIndex - 1);
				idx = startIndex - 1;
				startIndex = elementKey.LastIndexOf('/', idx);
				key = elementKey.Substring(startIndex + 1, idx - startIndex - 1);
				idx = startIndex - 1;
			}
			else
			{
				int num = elementKey.LastIndexOfAny(equalsOrSlash, idx);
				if (num < 0 || elementKey[num] == '/')
				{
					key = elementKey.Substring(num + 1, idx - num);
				}
				else
				{
					id = elementKey.Substring(num + 1, idx - num);
					idx = num - 1;
					num = elementKey.LastIndexOf('/', idx);
					key = elementKey.Substring(num + 1, idx - num);
				}
				idx = num - 1;
			}
			yield return new CompareElementKeySegment(id, string.Empty, EnumNameLookup<ElementType>.StringToEnum(key));
		}
	}

	public string CombineElementKeySegmentStrings(params string[] segments)
	{
		return string.Join(SegmentSeparatorString, segments);
	}

	public string ElementKeySegmentToElementKey(IElementKeySegment segment)
	{
		string text = segment.Id;
		if (segment is DisplayElementKeySegment)
		{
			if (string.IsNullOrEmpty(text) && ElementTypeInfo.Lookup.GetValue(segment.ElementType).IdProperty != PropertyType.Id)
			{
				text = segment.Name;
			}
			if (segment.ElementType == ElementType.Event)
			{
				text = text + "," + segment.Name.Substring(0, segment.Name.IndexOf("::", StringComparison.Ordinal)).Replace('@', ',');
			}
			if (IsIdentifiedByName(segment.ElementType))
			{
				text = segment.Name;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}={1}", EnumNameLookup<ElementType>.EnumToString(segment.ElementType), QuoteIfNeeded(text));
		}
		return EnumNameLookup<ElementType>.EnumToString(segment.ElementType);
	}

	public IEnumerable<string> SplitElementKeyList(string elementKeys)
	{
		throw new NotSupportedException();
	}

	private static string QuoteIfNeeded(string value)
	{
		if (value.IndexOfAny(charsRequiringQuotes) < 0)
		{
			return value;
		}
		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}

	private static bool IsIdentifiedByName(ElementType elementType)
	{
		if (elementType == ElementType.Variable)
		{
			return true;
		}
		return false;
	}
}
