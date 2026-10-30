using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Dynamics.Nav.MetaModel;

public static class ElementKeyHelper
{
	private static readonly IElementKeyProvider defaultProvider = new CompareElementKeyProvider();

	private static readonly IElementKeyProvider displayProvider = new DisplayElementKeyProvider();

	public static IElementKeyProvider DisplayProvider => displayProvider;

	public static string CreateElementKey(IElement element)
	{
		return defaultProvider.CreateElementKeySegmentString(element);
	}

	public static string CreateElementKey(IEnumerable<IElementKeySegment> segments)
	{
		return CombineElementKeySegmentStrings((from s in segments.Reverse()
			select defaultProvider.ElementKeySegmentToElementKey(s)).ToArray());
	}

	public static IElement FindElement(this IElement root, IEnumerable<IElementKeySegment> segments)
	{
		IElement element = root;
		foreach (IElementKeySegment item in segments.Reverse())
		{
			string key = defaultProvider.ElementKeySegmentToElementKey(item);
			element = element.GetSubElements(item.ElementType).SingleOrDefault((IElement e) => e.ElementKeyNoParent == key);
			if (element == null)
			{
				return null;
			}
		}
		return element;
	}

	public static IEnumerable<IElementKeySegment> GetElementKeySegments(string elementKey)
	{
		return defaultProvider.GetElementKeySegments(elementKey);
	}

	public static string CombineElementKeySegmentStrings(params string[] segments)
	{
		return defaultProvider.CombineElementKeySegmentStrings(segments);
	}

	public static string DisplayElementKeyToElementKey(string elementKey)
	{
		if (elementKey == null)
		{
			return null;
		}
		if (defaultProvider is DisplayElementKeyProvider)
		{
			return elementKey;
		}
		return CombineElementKeySegmentStrings((from s in DisplayProvider.GetElementKeySegments(elementKey).Reverse()
			select defaultProvider.ElementKeySegmentToElementKey(s)).ToArray());
	}

	public static string CreateDisplayElementKey(this IElement element, bool includeParent = true)
	{
		string text = DisplayProvider.CreateElementKeySegmentString(element);
		if (includeParent)
		{
			while (element.Parent != null && element.Parent.Parent != null)
			{
				text = DisplayProvider.CombineElementKeySegmentStrings(DisplayProvider.CreateElementKeySegmentString(element.Parent), text);
				element = element.Parent;
			}
		}
		return text;
	}

	public static ApplicationObjectId GetApplicationObjectIdFromDisplayElementKey(string elementKey)
	{
		IElementKeySegment elementKeySegment = DisplayProvider.GetElementKeySegments(elementKey).Single();
		return new ApplicationObjectId(elementKeySegment.ElementType, int.Parse(elementKeySegment.Id, CultureInfo.InvariantCulture));
	}
}
