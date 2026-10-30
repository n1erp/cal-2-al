using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class DisplayElementKeyProvider : IElementKeyProvider
{
	private const char SegmentSeparatorChar = '.';

	private static readonly string SegmentSeparatorString = '.'.ToString(CultureInfo.InvariantCulture);

	public string CreateElementKeySegmentString(IElement element)
	{
		string value = ((element.ElementTypeInfo.NameProperty != PropertyType.Unknown) ? element.GetStringProperty(element.ElementTypeInfo.NameProperty) : null);
		string text = ((element.ElementTypeInfo.TypeProperty != PropertyType.Unknown) ? element.GetStringProperty(element.ElementTypeInfo.TypeProperty) : null);
		string text2;
		if (element.Id != 0)
		{
			text2 = string.Format(CultureInfo.InvariantCulture, "{0} {1}", element.ElementTypeInfo.Name, element.Id);
		}
		else if (element.ElementTypeInfo.ElementType == ElementType.MenuNode)
		{
			text2 = string.Format(CultureInfo.InvariantCulture, "{0} {1}", element.ElementTypeInfo.Name, element.GetStringProperty(PropertyType.MenuNodeId));
		}
		else if (element.ElementTypeInfo.ElementType == ElementType.XmlPortElement)
		{
			text2 = string.Format(CultureInfo.InvariantCulture, "{0} {1}", element.ElementTypeInfo.Name, element.GetStringProperty(PropertyType.ElementId));
		}
		else
		{
			text2 = (string.IsNullOrEmpty(text) ? element.ElementTypeInfo.Name : text);
		}
		if (element.ElementTypeInfo.ElementType == ElementType.Change)
		{
			if (element.GetPropertySetState(PropertyType.Target))
			{
				text2 = string.Format(CultureInfo.InvariantCulture, "{0}@{1}", text2, element.GetStringProperty(PropertyType.Target));
			}
			if (!string.IsNullOrEmpty(value))
			{
				element.TryGetStringProperty(PropertyType.Property, out value);
			}
		}
		if (element.ElementTypeInfo.ElementType == ElementType.Event)
		{
			value = string.Format(CultureInfo.InvariantCulture, "{0}@{1}::{2}", element.GetStringProperty(PropertyType.EventSource), element.GetStringProperty(PropertyType.EventSourceId), value);
		}
		if (!string.IsNullOrEmpty(value))
		{
			text2 = string.Format(CultureInfo.InvariantCulture, "{0}({1})", ModelTextHelper.QuoteIdentifierIfNeeded(value), text2);
		}
		return text2;
	}

	public IEnumerable<IElementKeySegment> GetElementKeySegments(string elementKey)
	{
		int position = elementKey.Length;
		while (position > 0)
		{
			GetElementIdAndType(elementKey, ref position, out var id, out var type);
			string elementName = GetElementName(elementKey, ref position);
			yield return new DisplayElementKeySegment(id, elementName, type);
		}
	}

	public string CombineElementKeySegmentStrings(params string[] segments)
	{
		return string.Join(SegmentSeparatorString, segments);
	}

	public string ElementKeySegmentToElementKey(IElementKeySegment segment)
	{
		if (!(segment is DisplayElementKeySegment))
		{
			throw new NotSupportedException();
		}
		string text = (string.IsNullOrEmpty(segment.Id) ? EnumNameLookup<ElementType>.EnumToString(segment.ElementType) : string.Format(CultureInfo.InvariantCulture, "{0} {1}", EnumNameLookup<ElementType>.EnumToString(segment.ElementType), segment.Id));
		if (!string.IsNullOrEmpty(segment.Name))
		{
			text = string.Format(CultureInfo.InvariantCulture, "{0}({1})", ModelTextHelper.QuoteIdentifierIfNeeded(segment.Name), text);
		}
		return text;
	}

	public IEnumerable<string> SplitElementKeyList(string elementKeys)
	{
		int pos = 0;
		while (pos < elementKeys.Length)
		{
			for (; pos < elementKeys.Length && !ModelTextHelper.IsNavLetter(elementKeys[pos]) && elementKeys[pos] != '_' && elementKeys[pos] != '"'; pos++)
			{
			}
			int num = pos;
			if (elementKeys[pos] == '"')
			{
				for (pos++; pos < elementKeys.Length && elementKeys[pos] != '"'; pos++)
				{
				}
				pos++;
			}
			for (; pos < elementKeys.Length && elementKeys[pos] != '(' && elementKeys[pos] != ','; pos++)
			{
			}
			if (pos < elementKeys.Length && elementKeys[pos] == '(')
			{
				if (pos == elementKeys.Length)
				{
					throw new GenericElementException(NonLocalizable.ModelChanges_InvalidElementKeyList, elementKeys);
				}
				for (; pos < elementKeys.Length && elementKeys[pos] != ')'; pos++)
				{
				}
				pos++;
			}
			yield return elementKeys.Substring(num, pos - num);
			if (pos < elementKeys.Length && elementKeys[pos] != ',')
			{
				throw new GenericElementException(NonLocalizable.ModelChanges_InvalidElementKeyList, elementKeys);
			}
		}
	}

	private static bool GetElementIdAndType(string elementKey, ref int position, out string id, out ElementType type)
	{
		id = string.Empty;
		type = ElementType.Unknown;
		if (string.IsNullOrEmpty(elementKey) || position == 0)
		{
			return false;
		}
		if (elementKey[position - 1] == ')')
		{
			int num = elementKey.LastIndexOf(' ', position - 1);
			int num2 = elementKey.LastIndexOf('(', position - 1);
			if (num > num2)
			{
				id = elementKey.Substring(num + 1, position - num - 2);
				num2 = elementKey.LastIndexOf('(', num);
				if (num2 > 0)
				{
					EnumNameLookup<ElementType>.TryStringToEnum(elementKey.Substring(num2 + 1, num - num2 - 1), out type);
					position = num2;
					return true;
				}
				return false;
			}
			EnumNameLookup<ElementType>.TryStringToEnum(elementKey.Substring(num2 + 1, position - num2 - 2), out type);
			position = num2;
			return true;
		}
		int num3 = elementKey.LastIndexOf(' ', position - 1);
		int num4 = elementKey.LastIndexOf('.', position - 1);
		if (num4 >= num3)
		{
			EnumNameLookup<ElementType>.TryStringToEnum(elementKey.Substring(num4 + 1, position - num4 - 1), out type);
			position = num4 + 1;
			return true;
		}
		id = elementKey.Substring(num3 + 1, position - num3 - 1);
		EnumNameLookup<ElementType>.TryStringToEnum(elementKey.Substring(num4 + 1, num3 - num4 - 1), out type);
		position = num4 + 1;
		return true;
	}

	private static string GetElementName(string elementKey, ref int position)
	{
		if (string.IsNullOrEmpty(elementKey) || position == 0)
		{
			return string.Empty;
		}
		if (elementKey[position - 1] == '"')
		{
			int num = position - 2;
			bool flag = false;
			while ((num >= 0 && elementKey[num] != '"') || (num > 0 && elementKey[num] == '"' && elementKey[num - 1] == '"'))
			{
				if (elementKey[num] == '"')
				{
					num--;
					flag = true;
				}
				num--;
			}
			if (num < 0)
			{
				throw new GenericElementException();
			}
			string text = elementKey.Substring(num + 1, position - num - 2);
			position = num - 1;
			if (!flag)
			{
				return text;
			}
			return text.Replace("\"\"", "\"");
		}
		int num2 = position;
		int num3 = position - 1;
		while (num3 >= 0 && (elementKey[num3] == '_' || (elementKey[num3] >= 'a' && elementKey[num3] <= 'z') || (elementKey[num3] >= 'A' && elementKey[num3] <= 'Z') || char.IsDigit(elementKey[num3])))
		{
			num3--;
		}
		position = num3;
		return elementKey.Substring(num3 + 1, num2 - (num3 + 1));
	}
}
