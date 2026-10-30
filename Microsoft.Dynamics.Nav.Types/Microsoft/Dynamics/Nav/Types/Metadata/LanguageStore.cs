#define TRACE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class LanguageStore
{
	private const string CommentStart = "#";

	private static readonly char[] ElementSeparator = new char[3] { ';', ',', '\t' };

	private static readonly char[] Digits = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

	private const char TextSeparator = ':';

	private const int PropertyTextConst = 1000;

	private List<string> importedData;

	private static readonly List<Tuple<string, ObjectType>> parseObjectTypeMappings = new List<Tuple<string, ObjectType>>
	{
		Tuple.Create("Tab", ObjectType.Table),
		Tuple.Create("Rep", ObjectType.Report),
		Tuple.Create("CU", ObjectType.CodeUnit),
		Tuple.Create("XmlP", ObjectType.XmlPort),
		Tuple.Create("MenuSuite", ObjectType.MenuSuite),
		Tuple.Create("Page", ObjectType.Page),
		Tuple.Create("Query", ObjectType.Query)
	};

	private readonly ConcurrentDictionary<int, LanguageObjectContainer> tableContainer;

	private readonly ConcurrentDictionary<int, LanguageObjectContainer> codeunitContainer;

	private readonly ConcurrentDictionary<int, LanguageObjectContainer> pageContainer;

	private readonly ConcurrentDictionary<int, LanguageObjectContainer> reportContainer;

	private readonly ConcurrentDictionary<int, LanguageObjectContainer> xmlPortContainer;

	private readonly ConcurrentDictionary<int, LanguageObjectContainer> queryContainer;

	private readonly ConcurrentDictionary<Guid, MenuItemObject> menuSuiteContainer;

	private readonly ConcurrentDictionary<int, Collection<LanguageObject>> tableCache;

	private readonly ConcurrentDictionary<int, Collection<LanguageObject>> codeunitCache;

	private readonly ConcurrentDictionary<int, Collection<LanguageObject>> pageCache;

	private readonly ConcurrentDictionary<int, Collection<LanguageObject>> reportCache;

	private readonly ConcurrentDictionary<int, Collection<LanguageObject>> xmlPortCache;

	private readonly ConcurrentDictionary<int, Collection<LanguageObject>> queryCache;

	public IEnumerable<string> ImportedData => importedData;

	public bool HasData { get; private set; }

	public event EventHandler<EventArgs<int>> ItemAccessed;

	public LanguageStore(IList<string> languageLines)
	{
		tableContainer = new ConcurrentDictionary<int, LanguageObjectContainer>();
		codeunitContainer = new ConcurrentDictionary<int, LanguageObjectContainer>();
		pageContainer = new ConcurrentDictionary<int, LanguageObjectContainer>();
		reportContainer = new ConcurrentDictionary<int, LanguageObjectContainer>();
		xmlPortContainer = new ConcurrentDictionary<int, LanguageObjectContainer>();
		queryContainer = new ConcurrentDictionary<int, LanguageObjectContainer>();
		menuSuiteContainer = new ConcurrentDictionary<Guid, MenuItemObject>();
		tableCache = new ConcurrentDictionary<int, Collection<LanguageObject>>();
		codeunitCache = new ConcurrentDictionary<int, Collection<LanguageObject>>();
		pageCache = new ConcurrentDictionary<int, Collection<LanguageObject>>();
		reportCache = new ConcurrentDictionary<int, Collection<LanguageObject>>();
		xmlPortCache = new ConcurrentDictionary<int, Collection<LanguageObject>>();
		queryCache = new ConcurrentDictionary<int, Collection<LanguageObject>>();
		ProcessLanguageData(languageLines);
	}

	private void ProcessLanguageData(IList<string> languageLines)
	{
		if (languageLines == null)
		{
			return;
		}
		if (LanguageFormatConverter.IsTextFormat(languageLines))
		{
			languageLines = new LanguageFormatConverter().ConvertToLangFormat(languageLines);
		}
		if (!languageLines.Any())
		{
			return;
		}
		languageLines = languageLines.OrderBy((string ll) => ll).ToList();
		List<int> list = new List<int>();
		int num = 0;
		ObjectType objectType = ObjectType.TableData;
		int num2 = -1;
		int num3 = -1;
		importedData = new List<string>(languageLines.Count);
		foreach (string languageLine in languageLines)
		{
			if (string.IsNullOrEmpty(languageLine) || languageLine.StartsWith("#", StringComparison.Ordinal))
			{
				continue;
			}
			int num4 = languageLine.IndexOfAny(ElementSeparator);
			if (num4 < 0)
			{
				continue;
			}
			string text = languageLine.Substring(0, num4);
			int objectId;
			if ((objectId = IntegerValueFromString(text, 0, out var numberIndex)) < 0)
			{
				continue;
			}
			ObjectType objectType2 = ParseObjectType(text.Substring(0, numberIndex));
			if (objectType2 == ObjectType.MenuSuite)
			{
				MenuItemElement menuItemElement = ParseMenuSuiteData(objectId, languageLine.Substring(num4 + 1));
				if (menuItemElement != null)
				{
					try
					{
						menuSuiteContainer.AddOrUpdate(menuItemElement.MenuId, (Guid guid) => new MenuItemObject(objectId, menuItemElement), (Guid guidKey, MenuItemObject menuElement) => UpdateMenuItem(menuElement, menuItemElement));
					}
					catch
					{
						Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Could not add menuitem {0}", languageLine));
					}
				}
			}
			int startOffset;
			if ((startOffset = languageLine.IndexOf("Lang", StringComparison.Ordinal)) >= 0)
			{
				int item = IntegerValueFromString(languageLine, startOffset);
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
			importedData.Add(languageLine.Substring(num4 + 1));
			if (num3 == -1)
			{
				num2 = objectId;
				objectType = objectType2;
				num3 = 0;
			}
			else if (objectType2 != objectType || objectId != num2)
			{
				SaveElement(objectType, num2, num3, num);
				num2 = objectId;
				objectType = objectType2;
				num3 = num;
			}
			num++;
		}
		if (num2 != -1)
		{
			SaveElement(objectType, num2, num3, num);
		}
		HasData = num > 0;
	}

	private static MenuItemObject UpdateMenuItem(MenuItemObject item, MenuItemElement element)
	{
		item.Translations.TryAdd(element.LanguageCode, element.ItemValue);
		return item;
	}

	public bool HasDataFor(ObjectType objectType, int objectId)
	{
		ConcurrentDictionary<int, Collection<LanguageObject>> languageObjectCache = GetLanguageObjectCache(objectType);
		if (languageObjectCache == null || !languageObjectCache.ContainsKey(objectId))
		{
			return GetLanguageObjectContainer(objectType)?.ContainsKey(objectId) ?? false;
		}
		return true;
	}

	public LanguageElements GetMenuItemElements(string menuId)
	{
		if (menuSuiteContainer == null || menuSuiteContainer.Count < 1 || string.IsNullOrEmpty(menuId))
		{
			return null;
		}
		Guid key;
		try
		{
			key = new Guid(menuId);
		}
		catch (FormatException)
		{
			return null;
		}
		if (!menuSuiteContainer.TryGetValue(key, out var value))
		{
			return null;
		}
		menuSuiteContainer.AddOrUpdate(key, value, (Guid updateGuid, MenuItemObject updateItem) =>
		{
			updateItem.ReferenceCount++;
			return updateItem;
		});
		if (value.Translations.Count == 0)
		{
			return null;
		}
		List<int> list = new List<int>(value.Translations.Count);
		List<string> list2 = new List<string>(value.Translations.Count);
		foreach (KeyValuePair<int, string> translation in value.Translations)
		{
			list.Add(translation.Key);
			list2.Add(translation.Value);
		}
		return new LanguageElements(new ReadOnlyCollection<int>(list), new ReadOnlyCollection<string>(list2));
	}

	public LanguageElements GetLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage)
	{
		Collection<LanguageObject> languageObjects = GetLanguageObjects(objectType, objectId);
		if (languageObjects == null || languageObjects.Count < 1)
		{
			return null;
		}
		if (controlId == -1)
		{
			controlType = ControlType.None;
		}
		List<int> list = null;
		List<string> list2 = null;
		foreach (LanguageObject item in languageObjects.Where((LanguageObject languageObject) => controlType == languageObject.ControlType && controlId == languageObject.ControlId && requestPage == languageObject.RequestPage && languageObject.PropertyType == propertyType))
		{
			ItemAccessed?.Invoke(this, new EventArgs<int>(item.DataIndex));
			if (list == null)
			{
				list = new List<int>(languageObjects.Count);
			}
			if (list2 == null)
			{
				list2 = new List<string>(languageObjects.Count);
			}
			list.Add(item.LanguageCode);
			list2.Add(item.ItemValue);
		}
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return new LanguageElements(new ReadOnlyCollection<int>(list), new ReadOnlyCollection<string>(list2));
	}

	public LanguageElements GetLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage)
	{
		Collection<LanguageObject> languageObjects = GetLanguageObjects(objectType, objectId);
		if (languageObjects == null || languageObjects.Count < 1)
		{
			return null;
		}
		if (controlId == -1 && (controlType == ControlType.Field || controlType == ControlType.Code))
		{
			controlType = ControlType.None;
		}
		List<int> list = null;
		List<string> list2 = null;
		foreach (LanguageObject item in languageObjects.Where((LanguageObject languageObject) => controlId == languageObject.ControlId && controlType == languageObject.ControlType && 1000 == languageObject.PropertyType && textId == languageObject.PropertyId && codeId == languageObject.CodeId && requestPage == languageObject.RequestPage))
		{
			ItemAccessed?.Invoke(this, new EventArgs<int>(item.DataIndex));
			if (list == null)
			{
				list = new List<int>(languageObjects.Count);
			}
			if (list2 == null)
			{
				list2 = new List<string>(languageObjects.Count);
			}
			list.Add(item.LanguageCode);
			list2.Add(item.ItemValue);
		}
		if (list != null && list.Count != 0)
		{
			return new LanguageElements(new ReadOnlyCollection<int>(list), new ReadOnlyCollection<string>(list2));
		}
		return null;
	}

	private static int IntegerValueFromString(string numberString)
	{
		int numberIndex;
		return IntegerValueFromString(numberString, 0, out numberIndex);
	}

	private static int IntegerValueFromString(string numberString, int startOffset)
	{
		int numberIndex;
		return IntegerValueFromString(numberString, startOffset, out numberIndex);
	}

	private static int IntegerValueFromString(string numberString, int startoffset, out int numberIndex)
	{
		numberIndex = numberString.IndexOfAny(Digits, startoffset);
		if (numberIndex < 0)
		{
			return -1;
		}
		bool flag = false;
		if (numberIndex > 0)
		{
			flag = numberString[numberIndex - 1] == '-';
		}
		int i;
		for (i = 1; numberIndex + i < numberString.Length && char.IsDigit(numberString[numberIndex + i]); i++)
		{
		}
		if (flag)
		{
			numberIndex--;
			i++;
		}
		if (!int.TryParse(numberString.Substring(numberIndex, i), out var result))
		{
			return -1;
		}
		return result;
	}

	private void SaveElement(ObjectType objectType, int objectId, int startIndex, int lineNumber)
	{
		LanguageObjectContainer value = new LanguageObjectContainer(objectType, objectId, startIndex, lineNumber - startIndex);
		ConcurrentDictionary<int, LanguageObjectContainer> languageObjectContainer = GetLanguageObjectContainer(objectType);
		if (languageObjectContainer == null)
		{
			return;
		}
		try
		{
			languageObjectContainer.TryAdd(objectId, value);
		}
		catch
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Unable to add line {0}:{1} to object collection", objectType, objectId));
		}
	}

	private Tuple<string[], int> GetStrings(ObjectType objectType, int id)
	{
		ConcurrentDictionary<int, LanguageObjectContainer> languageObjectContainer = GetLanguageObjectContainer(objectType);
		if (languageObjectContainer == null || !languageObjectContainer.TryGetValue(id, out var value))
		{
			return null;
		}
		return Tuple.Create(importedData.GetRange(value.StartElement, value.ElementCount).ToArray(), value.StartElement);
	}

	private ConcurrentDictionary<int, LanguageObjectContainer> GetLanguageObjectContainer(ObjectType objectType)
	{
		return objectType switch
		{
			ObjectType.Table => tableContainer, 
			ObjectType.Report => reportContainer, 
			ObjectType.CodeUnit => codeunitContainer, 
			ObjectType.XmlPort => xmlPortContainer, 
			ObjectType.Page => pageContainer, 
			ObjectType.Query => queryContainer, 
			_ => null, 
		};
	}

	private ConcurrentDictionary<int, Collection<LanguageObject>> GetLanguageObjectCache(ObjectType objectType)
	{
		return objectType switch
		{
			ObjectType.Table => tableCache, 
			ObjectType.Report => reportCache, 
			ObjectType.CodeUnit => codeunitCache, 
			ObjectType.XmlPort => xmlPortCache, 
			ObjectType.Page => pageCache, 
			ObjectType.Query => queryCache, 
			_ => null, 
		};
	}

	private Collection<LanguageObject> GetLanguageObjectFromCache(ObjectType objectType, int objId)
	{
		ConcurrentDictionary<int, Collection<LanguageObject>> languageObjectCache = GetLanguageObjectCache(objectType);
		if (languageObjectCache == null)
		{
			return null;
		}
		if (languageObjectCache.TryGetValue(objId, out var value))
		{
			return value;
		}
		return null;
	}

	private void SetLanguageObjectCache(ObjectType objectType, int objId, Collection<LanguageObject> current)
	{
		ConcurrentDictionary<int, Collection<LanguageObject>> languageObjectCache = GetLanguageObjectCache(objectType);
		if (languageObjectCache == null)
		{
			return;
		}
		try
		{
			if (!languageObjectCache.ContainsKey(objId))
			{
				languageObjectCache.TryAdd(objId, current);
			}
		}
		catch
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Unable to add object {0}:{1} collection to cache", objectType, objId));
		}
	}

	private static Collection<LanguageObject> ParseLanguageData(Tuple<string[], int> rawData)
	{
		string[] item = rawData.Item1;
		int item2 = rawData.Item2;
		List<LanguageObject> list = new List<LanguageObject>(((ICollection<string>)item).Count);
		foreach (string item4 in (IEnumerable<string>)item)
		{
			int num = item4.IndexOf(':');
			string itemValue = StripTranslationString(item4.Substring(num + 1));
			string[] array = item4.Substring(0, num).Split(ElementSeparator);
			bool isRequestPage = false;
			int controlId = -1;
			int language = -1;
			ControlType controlType = ControlType.None;
			int propertyType = -1;
			if (!string.IsNullOrEmpty(array[0]) && array[0].Equals("yes"))
			{
				isRequestPage = true;
			}
			int codeId = -1;
			if (!string.IsNullOrEmpty(array[1]))
			{
				string text = array[1];
				string text2 = string.Empty;
				int num2;
				if ((num2 = text.IndexOf('!')) >= 0)
				{
					text2 = text.Substring(num2 + 1);
				}
				controlType = ParseControlType(text, out controlId);
				if (controlType == ControlType.Code)
				{
					codeId = controlId;
					controlId = -1;
					controlType = ControlType.None;
				}
				if (!string.IsNullOrEmpty(text2))
				{
					codeId = IntegerValueFromString(text2);
				}
			}
			int propertyId = -1;
			if (!string.IsNullOrEmpty(array[2]))
			{
				propertyType = ParsePropertyType(array[2], out propertyId);
			}
			if (!string.IsNullOrEmpty(array[3]))
			{
				language = IntegerValueFromString(array[3]);
			}
			LanguageObject item3 = new LanguageObject(isRequestPage, language, propertyType, controlType, controlId, codeId, propertyId, itemValue)
			{
				DataIndex = item2++
			};
			list.Add(item3);
		}
		return new Collection<LanguageObject>(list);
	}

	private static MenuItemElement ParseMenuSuiteData(int objectId, string rawData)
	{
		if (string.IsNullOrWhiteSpace(rawData))
		{
			return null;
		}
		int num = rawData.IndexOf(':');
		string itemValue = StripTranslationString(rawData.Substring(num + 1));
		string[] array = rawData.Substring(0, num).Split(ElementSeparator);
		int language = -1;
		if (string.IsNullOrEmpty(array[1]) || !array[1].StartsWith("MenuItem", StringComparison.Ordinal))
		{
			return null;
		}
		string g = array[1].Substring(8);
		Guid menuId = new Guid(g);
		if (!string.IsNullOrEmpty(array[3]))
		{
			language = IntegerValueFromString(array[3]);
		}
		return new MenuItemElement(objectId, language, menuId, itemValue);
	}

	private static string StripTranslationString(string translationString)
	{
		if (string.IsNullOrWhiteSpace(translationString))
		{
			return string.Empty;
		}
		if (!translationString.Contains("\""))
		{
			return translationString;
		}
		char[] trimChars = new char[2] { '\t', '"' };
		return translationString.Trim(trimChars);
	}

	private static ControlType ParseControlType(string control, out int controlId)
	{
		if (string.IsNullOrEmpty(control))
		{
			controlId = -1;
			return ControlType.None;
		}
		controlId = IntegerValueFromString(control, 0, out var numberIndex);
		string text = control;
		if (numberIndex > 0)
		{
			text = control.Substring(0, numberIndex);
		}
		switch (text)
		{
		case "Field":
			return ControlType.Field;
		case "P?":
		case "Code":
		case "Event":
			return ControlType.Code;
		case "Column":
			return ControlType.Column;
		case "Control":
			return ControlType.Control;
		case "Label":
			return ControlType.Label;
		case "DataItem":
			return ControlType.DataItem;
		case "ActionItem":
			return ControlType.ActionItem;
		case "Var":
			return ControlType.Control;
		default:
			return ControlType.None;
		}
	}

	private static int ParsePropertyType(string propertyString, out int propertyId)
	{
		propertyId = -1;
		if (string.IsNullOrEmpty(propertyString))
		{
			return 0;
		}
		switch (propertyString)
		{
		case "CapML":
			return 0;
		case "OptionCapML":
			return 2;
		case "PromotedActionCategoriesML":
			return 5;
		case "ToolTipML":
			return 4;
		case "InstructionalTextML":
			return 1;
		case "ReqFilterHeadingML":
			return 6;
		case "AdditionalSearchTermsML":
			return 7;
		default:
			if (propertyString.StartsWith("Txt", StringComparison.Ordinal))
			{
				propertyId = IntegerValueFromString(propertyString, 0, out var _);
				return 1000;
			}
			return -1;
		}
	}

	private static ObjectType ParseObjectType(string objectAsString)
	{
		return parseObjectTypeMappings.FirstOrDefault((Tuple<string, ObjectType> mapping) => objectAsString.StartsWith(mapping.Item1, StringComparison.Ordinal))?.Item2 ?? ObjectType.TableData;
	}

	private Collection<LanguageObject> GetLanguageObjects(ObjectType objType, int objId)
	{
		Collection<LanguageObject> languageObjectFromCache = GetLanguageObjectFromCache(objType, objId);
		if (languageObjectFromCache != null)
		{
			return languageObjectFromCache;
		}
		Tuple<string[], int> strings = GetStrings(objType, objId);
		if (strings == null)
		{
			return null;
		}
		languageObjectFromCache = ParseLanguageData(strings);
		if (languageObjectFromCache != null)
		{
			SetLanguageObjectCache(objType, objId, languageObjectFromCache);
		}
		return languageObjectFromCache;
	}
}
