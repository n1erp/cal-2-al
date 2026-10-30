using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal static class TranslationUtilities
{
	private const string LegacyIdSeparator = "-";

	internal static void WriteReportLabel(IAlWriter writer, IElement element)
	{
		string stringProperty = element.GetStringProperty(PropertyType.Name);
		if (element.GetStringProperty(PropertyType.CaptionML) != null)
		{
			string text = ProcessAndGetLabelSyntax(writer.State.TranslationExtractor, element, PropertyType.CaptionML);
			writer.WriteLine(stringProperty + " = " + text + ";");
		}
		else
		{
			writer.WriteLine("// The label '" + stringProperty + "' could not be exported.");
		}
	}

	internal static string ExtractTextConst(IElement ident, ITranslationExtractor translationExtractor, string value)
	{
		string text = ProcessAndGetLabelSyntax(translationExtractor, ident, PropertyType.TextConstString);
		return "Label " + text;
	}

	internal static string ProcessAndGetLabelSyntax(ITranslationExtractor translationExtractor, IElement element, PropertyType propertyType)
	{
		string text = ((element.ElementTypeInfo.ElementType != ElementType.Change) ? element.GetStringProperty(propertyType) : element.GetStringProperty(PropertyType.ModifiedValue));
		var (text2, text3) = GetLabelSyntaxFromMultilanguage(text);
		foreach (KeyValuePair<CultureInfo, string> item in ModelTextHelper.MLTextSplitter(text).ToList())
		{
			string text4 = item.Key?.Name;
			if (!string.IsNullOrEmpty(text4))
			{
				string value = item.Value;
				translationExtractor?.StoreLabel(element, propertyType, text2.UnquoteLiteral(), value.UnquoteLiteral(), text4, text3);
			}
		}
		return text2 + text3;
	}

	internal static int GetLegacyPropertyValue(IElement element, PropertyType elementPropertyType)
	{
		return elementPropertyType switch
		{
			PropertyType.CaptionML => 8629, 
			PropertyType.ToolTipML => 8631, 
			PropertyType.OptionCaptionML => 8632, 
			PropertyType.RequestFilterHeadingML => 18490, 
			PropertyType.TextConstString => 26171, 
			PropertyType.AdditionalSearchTermsML => element.ElementTypeInfo.ElementType switch
			{
				ElementType.Report => 18561, 
				ElementType.Page => 55074, 
				_ => throw new ArgumentOutOfRangeException(string.Format("${0} was not expected on an element of type {1}.", "AdditionalSearchTermsML", element.ElementTypeInfo.ElementType)), 
			}, 
			PropertyType.InstructionalTextML => 55119, 
			PropertyType.PromotedActionCategoriesML => 55132, 
			_ => throw new ArgumentOutOfRangeException("elementPropertyType", $"The {elementPropertyType} is not properly handled by the legacy ID generator."), 
		};
	}

	internal static string GetLegacyIdForProperty(IElement element, PropertyType propertyType)
	{
		return "P" + GetLegacyPropertyValue(element, propertyType);
	}

	internal static int GetLegacyValueForTrigger(IElement owningApplicationObject, IElement triggerElement)
	{
		ElementType elementType = triggerElement.ElementTypeInfo.ElementType;
		switch (owningApplicationObject.ElementTypeInfo.ElementType)
		{
		case ElementType.Codeunit:
			return GetLegacyValueForCodeunitTrigger(elementType);
		case ElementType.Table:
			return GetLegacyValueForTableTrigger(elementType);
		case ElementType.Page:
		case ElementType.RequestPage:
			return GetLegacyValueForPageTrigger(elementType);
		case ElementType.Report:
			return GetLegacyValueForReportTrigger(elementType);
		case ElementType.Query:
			return GetLegacyValueForQueryTrigger(elementType);
		case ElementType.XmlPort:
			return GetLegacyValueForXmlPortTrigger(elementType);
		default:
			throw new ArgumentOutOfRangeException("elementType", $"The {elementType} is not a known trigger.");
		}
	}

	private static int GetLegacyValueForXmlPortTrigger(ElementType triggerElementType)
	{
		return triggerElementType switch
		{
			ElementType.OnInitXmlPort => 54116, 
			ElementType.OnPreXmlPort => 54117, 
			ElementType.OnPostXmlPort => 54118, 
			ElementType.ImportOnAfterAssignVariable => 54126, 
			ElementType.ExportOnBeforePassVariable => 54127, 
			ElementType.ImportOnAfterInitRecord => 54136, 
			ElementType.ImportOnBeforeInsertRecord => 54137, 
			ElementType.ExportOnPreXmlItem => 54138, 
			ElementType.ExportOnAfterGetRecord => 54139, 
			ElementType.ImportOnAfterInsertRecord => 54140, 
			ElementType.ImportOnBeforeModifyRecord => 54141, 
			ElementType.ImportOnAfterModifyRecord => 54142, 
			ElementType.ImportOnAfterAssignField => 54146, 
			ElementType.ExportOnBeforePassField => 54147, 
			_ => throw new ArgumentOutOfRangeException("triggerElementType", $"The {triggerElementType} is not a known trigger."), 
		};
	}

	private static int GetLegacyValueForCodeunitTrigger(ElementType triggerElementType)
	{
		if (triggerElementType == ElementType.OnRun)
		{
			return 26369;
		}
		throw new ArgumentOutOfRangeException("triggerElementType", $"The {triggerElementType} is not a known trigger.");
	}

	private static int GetLegacyValueForQueryTrigger(ElementType triggerElementType)
	{
		if (triggerElementType == ElementType.OnBeforeOpen)
		{
			return 57700;
		}
		throw new ArgumentOutOfRangeException("triggerElementType", $"The {triggerElementType} is not a known trigger.");
	}

	private static int GetLegacyValueForReportTrigger(ElementType triggerElementType)
	{
		return triggerElementType switch
		{
			ElementType.OnInitReport => 18512, 
			ElementType.OnPreReport => 18513, 
			ElementType.OnPostReport => 18514, 
			ElementType.OnPreDataItem => 18529, 
			ElementType.OnAfterGetRecord => 18530, 
			ElementType.OnPostDataItem => 18531, 
			_ => throw new ArgumentOutOfRangeException("triggerElementType", $"The {triggerElementType} is not a known trigger."), 
		};
	}

	private static int GetLegacyValueForPageTrigger(ElementType triggerElementType)
	{
		return triggerElementType switch
		{
			ElementType.OnInit => 55140, 
			ElementType.OnOpenPage => 55141, 
			ElementType.OnClosePage => 55142, 
			ElementType.OnFindRecord => 55143, 
			ElementType.OnAfterGetRecord => 55145, 
			ElementType.OnNextRecord => 55144, 
			ElementType.OnNewRecord => 55146, 
			ElementType.OnInsertRecord => 55147, 
			ElementType.OnModifyRecord => 55148, 
			ElementType.OnDeleteRecord => 55149, 
			ElementType.OnQueryClosePage => 55150, 
			ElementType.OnAfterGetCurrRecord => 55151, 
			ElementType.OnAction => 55160, 
			ElementType.OnValidate => 55170, 
			ElementType.OnLookup => 55171, 
			ElementType.OnDrillDown => 55172, 
			ElementType.OnAssistEdit => 55173, 
			ElementType.OnControlAddIn => 55174, 
			_ => throw new ArgumentOutOfRangeException("triggerElementType", $"The {triggerElementType} is not a known trigger."), 
		};
	}

	private static int GetLegacyValueForTableTrigger(ElementType triggerElementType)
	{
		return triggerElementType switch
		{
			ElementType.OnInsert => 4958, 
			ElementType.OnModify => 4959, 
			ElementType.OnDelete => 4960, 
			ElementType.OnRename => 4961, 
			ElementType.OnValidate => 4962, 
			ElementType.OnLookup => 4963, 
			_ => throw new ArgumentOutOfRangeException("triggerElementType", $"The {triggerElementType} is not a known trigger."), 
		};
	}

	private static IElement GetParentForTrigger(IElement root, IElement trigger)
	{
		IElement element = trigger;
		while (element.Parent != null)
		{
			if (element.ElementTypeInfo.ElementType == ElementType.RequestPage)
			{
				return element;
			}
			element = element.Parent;
		}
		return element;
	}

	private static string GetLegacyTranslationId(IElement root, IElement e)
	{
		string text;
		switch (e.ElementTypeInfo.ElementType)
		{
		case ElementType.Codeunit:
			text = "C";
			break;
		case ElementType.Page:
			text = "N";
			break;
		case ElementType.Table:
			text = "T";
			break;
		case ElementType.XmlPort:
			text = "X";
			break;
		case ElementType.Query:
			text = "Q";
			break;
		case ElementType.Report:
			text = "R";
			break;
		case ElementType.Field:
			text = "F";
			break;
		case ElementType.Control:
		case ElementType.QueryElement:
			text = "C";
			break;
		case ElementType.Label:
			text = "T";
			break;
		case ElementType.DataItem:
			text = ((!string.Equals(e.GetStringProperty(PropertyType.ReportDataItemDataType), "column", StringComparison.OrdinalIgnoreCase)) ? "D" : "C");
			break;
		case ElementType.Variable:
			text = "Q";
			break;
		case ElementType.Procedure:
			text = "U";
			break;
		case ElementType.Action:
			text = "P55242-G";
			break;
		case ElementType.RequestPage:
			if (e.Parent.ElementTypeInfo.ElementType == ElementType.Report)
			{
				return GetLegacyTranslationId(root, e.Parent) + "-N2";
			}
			return GetLegacyTranslationId(root, e.Parent) + "-N1";
		case ElementType.XmlPortElement:
			return GetLegacyTranslationId(root, e.Parent);
		case ElementType.Modification:
			return GetModificationLegacyId(root, e);
		case ElementType.Event:
		{
			int? int32Property = e.GetInt32Property(PropertyType.EventSourceId);
			int? num = int32Property;
			text = "V" + num + "-E";
			break;
		}
		default:
			return GetLegacyTranslationId(root, e.Parent) + "-P" + GetLegacyValueForTrigger(GetParentForTrigger(root, e), e);
		}
		if (e.ElementTypeInfo.IdProperty == PropertyType.Unknown)
		{
			throw new ArgumentException($"The element {e.ElementTypeInfo.ElementType} is not correctly handled by the legacy translation generator.");
		}
		string text2 = text + e.GetInt32Property(e.ElementTypeInfo.IdProperty);
		if (e.ElementTypeInfo.ElementType != root.ElementTypeInfo.ElementType)
		{
			return GetLegacyTranslationId(root, e.Parent) + "-" + text2;
		}
		return text2;
	}

	private static string GetModificationLegacyId(IElement root, IElement e)
	{
		ElementType extensionTargetType = ModificationContentExporter.GetExtensionTargetType(e);
		return extensionTargetType switch
		{
			ElementType.Page => "N", 
			ElementType.Table => "T", 
			_ => throw new ArgumentException($"Unexpected extension target : {extensionTargetType}"), 
		};
	}

	internal static string GetTranslationItemLegacyId(IElement root, IElement element, PropertyType propertyType)
	{
		return string.Join("-", GetLegacyTranslationId(root, element), GetLegacyIdForProperty(element, propertyType));
	}

	internal static string GetTranslationItemId(IElement element, PropertyType propertyType, bool shouldHashName, string rootObjectName)
	{
		bool num = propertyType != PropertyType.TextConstString && element.ElementTypeInfo.ElementType != ElementType.Label;
		IElement root = AlUtilities.GetRoot(element);
		List<string> list = new List<string> { GetSingleElementId(root, shouldHashName, rootObjectName) };
		GetParentItemsTranslationIds(root, element, shouldHashName, list);
		list.Add(GetSingleElementId(element, shouldHashName));
		if (num)
		{
			string text = propertyType.ToString();
			text = (text.EndsWith("ML") ? text.Substring(0, text.Length - 2) : text);
			if (shouldHashName)
			{
				text = LanguageFileUtilities.GetNameHash(text);
			}
			list.Add("Property " + text);
		}
		return RemoveDuplicatesAndMakeString(list);
	}

	private static void GetParentItemsTranslationIds(IElement rootObject, IElement element, bool shouldHashName, List<string> listOfIds)
	{
		if (element.Parent == null)
		{
			return;
		}
		IElement parent = element.Parent;
		CategoryType category = parent.ElementTypeInfo.Category;
		if (category != CategoryType.Method && category != CategoryType.Trigger)
		{
			return;
		}
		if (parent.Parent != rootObject && !parent.Parent.IsOfType(ElementType.Change))
		{
			listOfIds.Add(GetSingleElementId(parent.Parent, shouldHashName));
		}
		if (parent.ElementTypeInfo.ElementType == ElementType.Event)
		{
			int? ownerId = AlUtilities.GetEventOwnerId(parent);
			IElement element2 = rootObject.GetSubElements().FirstOrDefault((IElement e) => e.Id == ownerId);
			if (element2 != null)
			{
				listOfIds.Add(GetSingleElementId(element2, shouldHashName));
			}
		}
		listOfIds.Add(GetSingleElementId(element.Parent, shouldHashName));
	}

	private static string RemoveDuplicatesAndMakeString(IList<string> ids)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < ids.Count; i++)
		{
			if (!hashSet.Contains(ids[i]))
			{
				if (instance.Length > 0)
				{
					instance.Builder.Append(" - ");
				}
				instance.Builder.Append(ids[i]);
				hashSet.Add(ids[i]);
			}
		}
		return instance.ToStringAndFree();
	}

	private static string GetSingleElementId(IElement element, bool shouldHashName, string rootObjectName = null)
	{
		string elementType = GetElementType(element, shouldHashName);
		string elementName = GetElementName(element, shouldHashName, rootObjectName, elementType);
		return elementType + " " + elementName;
	}

	private static string GetElementName(IElement element, bool shouldHashName, string rootObjectName, string elementType)
	{
		string arg;
		if (rootObjectName != null)
		{
			arg = rootObjectName;
		}
		else if (IsElementWithoutName(element))
		{
			arg = element.ElementTypeInfo.Name;
		}
		else
		{
			arg = ((element.ElementTypeInfo.ElementType != ElementType.Event) ? AlUtilities.GetElementName(element) : AlUtilities.GetEventSubscriberName(element));
		}
		arg = arg.UnquoteIdentifier();
		if (shouldHashName)
		{
			arg = LanguageFileUtilities.GetNameHash(arg);
		}
		return arg;
	}

	private static bool IsElementWithoutName(IElement element)
	{
		if (element.ElementTypeInfo.Category != CategoryType.Trigger)
		{
			return element.ElementTypeInfo.ElementType == ElementType.RequestPage;
		}
		return true;
	}

	private static string GetElementType(IElement element, bool shouldHashName)
	{
		ElementType elementType = element.ElementTypeInfo.ElementType;
		IElement root = AlUtilities.GetRoot(element);
		if (root != element && !IsAnyParentElementOfType(element, ElementType.Change) && root.ElementTypeInfo.ElementType == ElementType.Modification)
		{
			return "Change";
		}
		if (element.GetPropertySetState(PropertyType.TargetPageType))
		{
			return "PageExtension";
		}
		if (element.GetPropertySetState(PropertyType.Target))
		{
			return "TableExtension";
		}
		if (element.ElementTypeInfo.Category == CategoryType.Method || element.ElementTypeInfo.Category == CategoryType.Trigger)
		{
			return "Method";
		}
		return elementType switch
		{
			ElementType.Variable => "NamedType", 
			ElementType.Label => "ReportLabel", 
			ElementType.DataItem => root.ElementTypeInfo.ElementType.ToString() + "DataItem", 
			_ => elementType.ToString(), 
		};
	}

	private static bool IsAnyParentElementOfType(IElement element, ElementType elementType)
	{
		while (element != null)
		{
			if (element.ElementTypeInfo.ElementType == elementType)
			{
				return true;
			}
			element = element.Parent;
		}
		return false;
	}

	internal static (string, string) GetLabelSyntaxFromMultilanguage(string textconst)
	{
		List<string> list = ModelTextHelper.MLTextFormatter(ModelTextHelper.MLTextSplitter(textconst), 0, forceUseSingleQuotes: true).ToList();
		string comment = list.Find((string v) => v.StartsWith("@@@="))?.Substring("@@@=".Length);
		comment = ProcessCommentString(comment);
		IEnumerable<string> source = list.Where((string v) => !string.IsNullOrEmpty(v) && !v.StartsWith("@@@="));
		return ((source.FirstOrDefault((string l) => l.StartsWith("ENU")) ?? source.FirstOrDefault())?.Substring("@@@=".Length) ?? string.Empty.QuoteLiteral(), comment);
	}

	private static string ProcessCommentString(string comment)
	{
		if (comment == null)
		{
			return string.Empty;
		}
		if (comment == "'{Locked}'")
		{
			return ", Locked=true";
		}
		return ", Comment=" + comment;
	}
}
