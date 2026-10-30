using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Compiler;
using Microsoft.Dynamics.Nav.Model.IO.Txt;
using Microsoft.Dynamics.Nav.Model.TypeSystem;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;
using Txt2AlConverter;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public static class AlUtilities
{
	private const string TxtMlCommentFormat = "@@@=";

	private const string AlMlCommentFormat = "Comment=";

	public const string ScopeAttributeName = "Scope";

	public const string MultiLineCommentStart = "/*";

	public const string MultiLineCommentEnd = "*/";

	private static readonly Regex TypeWithLengthRegex = new Regex("(Code|Text)([0-9]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly Regex PageOrTableAndIdRegex = new Regex("^(Page|Table)([0-9]+)$", RegexOptions.IgnoreCase);

	internal static void WriteElementHeaderContent(IAlWriter writer, IElement element, PropertyType[] template, IPropertiesConverter propertiesExporter)
	{
		for (int i = 0; i < template.Length; i++)
		{
			if (i > 0)
			{
				writer.Write(';');
			}
			if (element.ElementTypeInfo.Properties.TryGetValue(template[i], out var value))
			{
				string propertyValue = propertiesExporter.GetPropertyValue(element, value.Property, writer.State);
				if (propertyValue != null)
				{
					writer.Write(propertyValue);
				}
				else if (template[i] != PropertyType.Name || !TryCorrectMissingName(element, writer))
				{
					Console.Error.WriteLine($"The value of property {template[i]} of element with ID {element.Id.ToString(CultureInfo.InvariantCulture)} has not been correctly exported by C\\SIDE.");
				}
			}
		}
	}

	internal static string GetCompilationScopeAttribute(Version runtimeVersion, string calAttrbuteValue = null)
	{
		Version version = new Version(4, 0, 0, 0);
		string text = "Extension";
		if (runtimeVersion >= version)
		{
			switch (calAttrbuteValue?.ToLowerInvariant())
			{
			case "[external]":
				text = "Cloud";
				break;
			case null:
			case "[internal]":
				text = "OnPrem";
				break;
			}
		}
		else
		{
			switch (calAttrbuteValue?.ToLowerInvariant())
			{
			case "[external]":
				text = SymbolCompilationScope.Extension.ToString();
				break;
			case null:
			case "[internal]":
				text = SymbolCompilationScope.Internal.ToString();
				break;
			}
		}
		return FormattableString.Invariant($"[Scope('{text.ToString()}')]");
	}

	private static bool TryCorrectMissingName(IElement element, IAlWriter writer)
	{
		element.TryGetStringProperty(PropertyType.SourceExpression, out var value);
		if (value != null)
		{
			writer.Write(value);
			return true;
		}
		element.TryGetStringProperty(PropertyType.CaptionML, out value);
		if (value != null)
		{
			value = (from pair in ModelTextHelper.MLTextSplitter(value)
				select ModelTextHelper.MLTextFormatter(new KeyValuePair<CultureInfo, string>[1] { pair }, int.MaxValue, forceUseSingleQuotes: true).FirstOrDefault()).First().Split('=')[1].UnquoteLiteral().QuoteIdentifierIfNeeded();
			writer.Write(value);
			return true;
		}
		return false;
	}

	public static string QuoteIdentifierIfNeeded(string value)
	{
		if (!NeedsIdentifierQuotes(value))
		{
			return value;
		}
		return "\"" + value.Replace("\"", "\"\"") + "\"";
	}

	private static bool NeedsIdentifierQuotes(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (IsIdentifierQuoted(value))
		{
			return false;
		}
		if (IsKeyword(value))
		{
			return true;
		}
		if (value[0] != '_' && (value[0] < 'a' || value[0] > 'z') && (value[0] < 'A' || value[0] > 'Z'))
		{
			return true;
		}
		for (int num = value.Length - 1; num > 0; num--)
		{
			if (value[num] != '_' && (value[num] < 'a' || value[num] > 'z') && (value[num] < 'A' || value[num] > 'Z') && !char.IsDigit(value[num]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsIdentifierQuoted(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (value[0] == '"' && value[value.Length - 1] == '"')
		{
			return !DoesIdentifierContainUnescapedDoubleQuotes(value);
		}
		return false;
	}

	private static bool DoesIdentifierContainUnescapedDoubleQuotes(string value)
	{
		if (value.Length == 1)
		{
			return value[0] == '"';
		}
		int num = 1;
		while (num < value.Length - 2)
		{
			if (value[num] == '"')
			{
				if (value[num + 1] != '"')
				{
					return true;
				}
				num += 2;
			}
			else
			{
				num++;
			}
		}
		return false;
	}

	public static string QuoteStringLiteralIfNeeded(string value, bool needsSingleQuoting)
	{
		if (!needsSingleQuoting)
		{
			return value;
		}
		return SingleQuote(value);
	}

	public static ControlKind ControlTypeToControlKind(IElement control)
	{
		string stringProperty = control.GetStringProperty(PropertyType.ControlType);
		stringProperty = stringProperty.ToUpperInvariant();
		if (stringProperty == "GROUP")
		{
			stringProperty = control.GetStringProperty(PropertyType.GroupType)?.ToUpperInvariant() ?? "GROUP";
		}
		switch (stringProperty)
		{
		case "FIELD":
		{
			if (control.IsUserControl())
			{
				return ControlKind.UserControl;
			}
			if (control.TryGetStringProperty(PropertyType.SourceExpression, out var value) && value == null)
			{
				return ControlKind.Label;
			}
			return ControlKind.Field;
		}
		case "GROUP":
			return ControlKind.Group;
		case "PART":
			return (control.GetStringProperty(PropertyType.PartType)?.ToUpperInvariant() ?? "PAGE") switch
			{
				"SYSTEM" => ControlKind.SystemPart, 
				"CHART" => ControlKind.ChartPart, 
				_ => ControlKind.Part, 
			};
		case "GRIDLAYOUT":
			return ControlKind.Grid;
		case "FIXEDLAYOUT":
			return ControlKind.Fixed;
		case "CUEGROUP":
			return ControlKind.CueGroup;
		case "REPEATER":
			return ControlKind.Repeater;
		case "CONTAINER":
			return ControlKind.Area;
		default:
			return ControlKind.Field;
		}
	}

	public static ActionKind ActionTypeToActionKind(string actionType)
	{
		return actionType.ToUpperInvariant() switch
		{
			"ACTION" => ActionKind.Action, 
			"ACTIONGROUP" => ActionKind.Group, 
			"ACTIONCONTAINER" => ActionKind.Area, 
			"SEPARATOR" => ActionKind.Separator, 
			_ => ActionKind.Action, 
		};
	}

	public static string DataitemTypeToSectionName(string dataitemType)
	{
		return dataitemType.ToUpperInvariant() switch
		{
			"DATAITEM" => "DataItem", 
			"COLUMN" => "Column", 
			"FILTER" => "Filter", 
			_ => "DataItem", 
		};
	}

	public static string GetExtensionObjectName(IElement rootElement, int id)
	{
		return ModificationContentExporter.GetExtensionObjectName(rootElement, id, ModificationContentExporter.GetExtensionTypeDisplayName(rootElement));
	}

	public static string GetModificationTargetName(string elementIdentifier)
	{
		return ModificationContentExporter.GetALNameFromElementIdentifier(elementIdentifier);
	}

	public static QueryElementKind QueryTypeToQueryKind(IElement element)
	{
		return element.GetStringProperty(PropertyType.QueryElementType).ToUpperInvariant() switch
		{
			"DATAITEM" => QueryElementKind.DataItem, 
			"COLUMN" => QueryElementKind.Column, 
			"FILTER" => QueryElementKind.Filter, 
			_ => QueryElementKind.DataItem, 
		};
	}

	public static string GetControlNameFromControlContainerType(string controlContainerType)
	{
		return controlContainerType switch
		{
			"ContentArea" => AreaKind.Content.ToString(), 
			"FactBoxArea" => AreaKind.FactBoxes.ToString(), 
			"RoleCenterArea" => AreaKind.RoleCenter.ToString(), 
			_ => null, 
		};
	}

	public static string GetActionNameFromActionContainerType(string actionContainerType)
	{
		return actionContainerType switch
		{
			"NewDocumentItems" => ActionAreaKind.Creation.ToString(), 
			"HomeItems" => ActionAreaKind.Embedding.ToString(), 
			"RelatedInformation" => ActionAreaKind.Navigation.ToString(), 
			"ActionItems" => ActionAreaKind.Processing.ToString(), 
			"Reports" => ActionAreaKind.Reporting.ToString(), 
			"ActivityButtons" => ActionAreaKind.Sections.ToString(), 
			_ => null, 
		};
	}

	internal static void WriteVariableDeclaration(IElement ident, IAlWriter writer, bool prependNameWithSpace = false, bool commentedOut = false, bool isParameter = false)
	{
		if (ident.IsTrue(PropertyType.InDataSet))
		{
			writer.WriteLine("[InDataSet]");
		}
		if (ident.TryGetStringProperty(PropertyType.SecurityFiltering, out var value) && !string.IsNullOrEmpty(value))
		{
			writer.Write(string.Format(CultureInfo.InvariantCulture, "[SecurityFiltering(SecurityFilter::{0})]", value));
		}
		if (ident.IsTrue(PropertyType.RunOnClient) && !isParameter)
		{
			writer.WriteLine("[RunOnClient]");
		}
		if (ident.IsTrue(PropertyType.WithEvents) && !isParameter)
		{
			writer.WriteLine("[WithEvents]");
		}
		if (ident.IsTrue(PropertyType.SuppressDispose) && !isParameter)
		{
			writer.WriteLine("[SuppressDispose]");
		}
		if (ident.IsTrue(PropertyType.NonDebuggable) && !isParameter)
		{
			writer.WriteLine("[NonDebuggable]");
		}
		if (commentedOut)
		{
			writer.Write("//");
		}
		StringBuilder stringBuilder = new StringBuilder();
		FormatIdentifierName(stringBuilder, ident, prependNameWithSpace);
		writer.Write(stringBuilder.ToString());
		writer.Write(": ");
		writer.Write(writer.State.TypeResolver.ResolveType(ident));
		if (ident.IsTrue(PropertyType.Temporary))
		{
			writer.Write(" temporary");
		}
	}

	public static string ConvertTypeString(string typeString, int? length)
	{
		if (typeString == null)
		{
			return null;
		}
		if (typeString.Equals("Text", StringComparison.OrdinalIgnoreCase) || typeString.Equals("Code", StringComparison.OrdinalIgnoreCase))
		{
			if (length.HasValue && length.Value > 0)
			{
				return typeString + "[" + length.Value + "]";
			}
		}
		else
		{
			MatchCollection matchCollection = TypeWithLengthRegex.Matches(typeString);
			if (matchCollection.Count > 0)
			{
				return matchCollection[0].Groups[1]?.ToString() + "[" + matchCollection[0].Groups[2]?.ToString() + "]";
			}
		}
		if (typeString == "GUID")
		{
			return "Guid";
		}
		return typeString;
	}

	public static string RemoveIdentifierQuotes(string value)
	{
		if (string.IsNullOrEmpty(value) || value.Length < 2)
		{
			return value;
		}
		if (value[0] == '"' && value[value.Length - 1] == '"')
		{
			return value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
		}
		return value;
	}

	public static int GetIndentation(IElement elem)
	{
		switch (elem.ElementTypeInfo.ElementType)
		{
		case ElementType.Control:
		case ElementType.Action:
		case ElementType.QueryElement:
			return elem.GetInt32Property(PropertyType.Level).GetValueOrDefault();
		case ElementType.XmlPortElement:
		case ElementType.DataItem:
			return elem.GetInt32Property(PropertyType.Indentation).GetValueOrDefault();
		default:
			return 0;
		}
	}

	public static string GetSectionTypeName(IElement child)
	{
		ControlKind controlKind;
		switch (child.ElementTypeInfo.ElementType)
		{
		case ElementType.Control:
			controlKind = ControlTypeToControlKind(child);
			if (controlKind == ControlKind.Part)
			{
				if (child.GetStringProperty(PropertyType.SystemPartId) == null)
				{
					string stringProperty2 = child.GetStringProperty(PropertyType.PartType);
					if (stringProperty2 == null || !stringProperty2.Equals("System", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0077;
					}
				}
				return "systempart";
			}
			goto IL_0077;
		case ElementType.Action:
			return ActionTypeToActionKind(child.GetStringProperty(PropertyType.ActionType)).ToString();
		case ElementType.XmlPortElement:
		{
			string stringProperty = child.GetStringProperty(PropertyType.SourceType);
			return string.Concat(str1: child.GetStringProperty(PropertyType.NodeType).ToLowerInvariant(), str0: stringProperty.ToLowerInvariant());
		}
		case ElementType.DataItem:
			return DataitemTypeToSectionName(child.GetStringProperty(PropertyType.ReportDataItemDataType));
		case ElementType.QueryElement:
			return DataitemTypeToSectionName(child.GetStringProperty(PropertyType.QueryElementType));
		default:
			{
				return child.ElementTypeInfo.Name.ToLowerInvariant();
			}
			IL_0077:
			if (child.IsLabelControl())
			{
				return "label";
			}
			return controlKind.ToString();
		}
	}

	public static void FormatIdentifierName(StringBuilder sb, IElement ident, bool prependNameWithSpace)
	{
		if (ident.GetPropertySetState(PropertyType.Name))
		{
			if (prependNameWithSpace)
			{
				sb.Append(' ');
			}
			string stringProperty = ident.GetStringProperty(PropertyType.Name);
			sb.Append(QuoteIdentifierIfNeeded(stringProperty));
		}
	}

	public static string ConvertMlCommentFormat(string value)
	{
		return value.Replace("@@@=", "Comment=");
	}

	public static bool IsKeyword(string value)
	{
		return AlExporterLists.Keywords.Contains(value);
	}

	public static string SingleQuote(string value)
	{
		return "'" + value.Replace("'", "''") + "'";
	}

	public static string GetEventSubscriberName(IElement e)
	{
		return e.GetStringProperty(PropertyType.EventSource) + "::" + GetElementName(e);
	}

	internal static int? GetEventOwnerId(IElement element)
	{
		if (element == null || element.ElementTypeInfo.ElementType != ElementType.Event)
		{
			return null;
		}
		return -element.GetInt32Property(PropertyType.EventSourceId);
	}

	public static string GetElementName(IElement child, HashSet<string> nameSet = null)
	{
		string text = child.GetStringProperty(PropertyType.Name);
		if (child.ElementTypeInfo.ElementType == ElementType.Control && !string.IsNullOrEmpty(text))
		{
			text = text.UnquoteIdentifier();
		}
		if (string.IsNullOrEmpty(text) && child.GetPropertySetState(PropertyType.SourceExpression))
		{
			string stringProperty = child.GetStringProperty(PropertyType.SourceExpression);
			if (!string.IsNullOrEmpty(stringProperty))
			{
				text = RemoveIdentifierQuotes(stringProperty);
			}
		}
		if (IsNameInvalid(text, nameSet) && child.TryGetStringProperty(PropertyType.CaptionML, out var value) && value != null)
		{
			text = ModelTextHelper.MLTextSplitter(value).SingleOrDefault((KeyValuePair<CultureInfo, string> ml) =>
			{
				CultureInfo key = ml.Key;
				return key != null && key.LCID == 1033;
			}).Value;
		}
		if (string.IsNullOrEmpty(text) && IsQueryElement(child))
		{
			text = DefaultNameForQueryElements(child);
		}
		if (IsNameInvalid(text, nameSet) || (string.IsNullOrWhiteSpace(text) && child.ElementTypeInfo.ElementType == ElementType.Control))
		{
			text = child.ElementTypeInfo.ElementType.ToString() + child.Id;
		}
		nameSet?.Add(text);
		return text;
	}

	private static bool IsNameInvalid(string name, HashSet<string> nameSet)
	{
		if (!string.IsNullOrEmpty(name) && (nameSet == null || !nameSet.Contains(name)))
		{
			return name.Length > 120;
		}
		return true;
	}

	private static bool IsQueryElement(IElement element)
	{
		if (!element.GetPropertySetState(PropertyType.DataSource))
		{
			return element.GetPropertySetState(PropertyType.MethodType);
		}
		return true;
	}

	private static string DefaultNameForQueryElements(IElement element)
	{
		string text = null;
		string stringProperty = element.GetStringProperty(PropertyType.DataSource);
		string stringProperty2 = element.GetStringProperty(PropertyType.Method);
		if (!string.IsNullOrEmpty(stringProperty))
		{
			text = string.Concat((from c in RemoveIdentifierQuotes(stringProperty)
				select (!char.IsLetterOrDigit(c)) ? '_' : c).Reverse().SkipWhile((char c) => c == '_').Reverse());
			text = Regex.Replace(text, "_{2,}", "_");
		}
		if (!string.IsNullOrEmpty(stringProperty2))
		{
			text = stringProperty2 + "_" + text;
		}
		return text;
	}

	public static IEnumerable<IElement> QueryFullElementTree(IElement anchor, ElementType elementType, Func<IElement, bool> predicate)
	{
		return QueryElementRecursive(GetRoot(anchor), elementType, predicate);
	}

	private static IEnumerable<IElement> QueryElementRecursive(IElement current, ElementType elementType, Func<IElement, bool> predicate)
	{
		if (predicate(current))
		{
			yield return current;
		}
		foreach (IElement subElement in current.GetSubElements(elementType))
		{
			foreach (IElement item in QueryElementRecursive(subElement, elementType, predicate))
			{
				yield return item;
			}
		}
	}

	internal static IElement GetRoot(IElement element)
	{
		IElement element2 = element;
		while (element2.Parent != null)
		{
			element2 = element2.Parent;
		}
		return element2;
	}

	private static IElement GetContainingObject(IElement element)
	{
		if (element == null)
		{
			return null;
		}
		IElement element2 = element;
		while (element2.Parent != null && element2.ElementTypeInfo.ElementType != ElementType.RequestPage)
		{
			element2 = element2.Parent;
		}
		return element2;
	}

	private static ElementType? GetContainingObjectElementType(IElement element)
	{
		return GetContainingObject(element)?.ElementTypeInfo?.ElementType;
	}

	internal static TriggerTypeInfo GetTriggerTypeInfo(IElement element, string elemInfoName)
	{
		ElementType valueOrDefault = GetContainingObjectElementType(element).GetValueOrDefault();
		ElementType extendedElementTypeIfApplicable = GetExtendedElementTypeIfApplicable(element, valueOrDefault);
		if (valueOrDefault == extendedElementTypeIfApplicable)
		{
			return GetTriggerTypeInfoBasic(element, elemInfoName, valueOrDefault);
		}
		return GetTriggerTypeInfoBasic(element, elemInfoName, extendedElementTypeIfApplicable) ?? GetTriggerTypeInfoBasic(element, elemInfoName, valueOrDefault);
	}

	internal static TriggerTypeInfo GetTriggerTypeInfoBasic(IElement element, string elemInfoName, ElementType rootType)
	{
		switch (rootType)
		{
		case ElementType.Page:
			return TriggerDefinitions.GetPageTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetControlTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetActionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetFieldTriggerInfo(elemInfoName);
		case ElementType.Table:
			return TriggerDefinitions.GetTableTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetFieldTriggerInfo(elemInfoName);
		case ElementType.Codeunit:
			return TriggerDefinitions.GetCodeunitTriggerInfo(elemInfoName);
		case ElementType.Field:
			return TriggerDefinitions.GetFieldTriggerInfo(elemInfoName);
		case ElementType.Action:
			return TriggerDefinitions.GetActionTriggerInfo(elemInfoName);
		case ElementType.Control:
			return TriggerDefinitions.GetControlTriggerInfo(elemInfoName);
		case ElementType.Query:
			return TriggerDefinitions.GetQueryTriggerInfo(elemInfoName);
		case ElementType.XmlPort:
		{
			int num = elemInfoName.IndexOf(':');
			if (num > 0)
			{
				elemInfoName = elemInfoName.Substring(num + 2);
			}
			return TriggerDefinitions.GetXmlPortTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetXmlPortTableElementTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetXmlPortFieldElementTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetXmlPortTextElementTriggerInfo(elemInfoName);
		}
		case ElementType.Report:
			return TriggerDefinitions.GetReportTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetReportDataItemTriggerInfo(elemInfoName);
		case ElementType.RequestPage:
			return TriggerDefinitions.GetPageTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetControlTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetActionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetFieldTriggerInfo(elemInfoName);
		case ElementType.Modification:
			return TriggerDefinitions.GetTableExtensionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetPageExtensionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetFieldExtensionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetActionExtensionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetControlExtensionTriggerInfo(elemInfoName);
		default:
			return TriggerDefinitions.GetTableExtensionTriggerInfo(elemInfoName) ?? TriggerDefinitions.GetFieldExtensionTriggerInfo(elemInfoName);
		}
	}

	private static ElementType GetExtendedElementTypeIfApplicable(IElement element, ElementType rootType)
	{
		if (rootType != ElementType.Modification)
		{
			return rootType;
		}
		if (ChangeContentExporter.GetChangeType(element.Parent) == ChangeType.InsertAfter)
		{
			return element.ElementTypeInfo.ElementType;
		}
		return ElementType.Modification;
	}

	internal static string GetSourceId(string value)
	{
		if (int.TryParse(value, out var _))
		{
			return value;
		}
		MatchCollection matchCollection = PageOrTableAndIdRegex.Matches(value);
		if (matchCollection.Count == 1 && matchCollection[0].Groups.Count == 3)
		{
			return matchCollection[0].Groups[2].Value;
		}
		return null;
	}

	internal static bool IsValueBoolean(string value)
	{
		switch (value.ToLowerInvariant())
		{
		case "true":
		case "yes":
		case "false":
		case "no":
			return true;
		default:
			return false;
		}
	}

	internal static bool? ToBooleanValue(string value)
	{
		switch (value?.ToLowerInvariant())
		{
		case "true":
		case "yes":
			return true;
		case "false":
		case "no":
			return false;
		default:
			return null;
		}
	}

	internal static List<string> SplitCommaSeperatedIdentifiers(string value)
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(value))
		{
			return list;
		}
		bool flag = false;
		int num = 0;
		for (int i = 0; i < value.Length; i++)
		{
			if (!flag && value[i] == ',')
			{
				list.Add(value.Substring(num, i - num));
				num = i + 1;
			}
			else if (!flag && value[i] == '"')
			{
				flag = true;
			}
			else if (flag && value[i] == '"' && (i + 1 >= value.Length || value[i + 1] != '"'))
			{
				flag = false;
			}
		}
		if (num < value.Length)
		{
			list.Add(value.Substring(num));
		}
		return list;
	}

	internal static IEnumerable<TokenInfo> GetTokens(string text, bool enablePartialScan = false)
	{
		if (string.IsNullOrEmpty(text))
		{
			yield break;
		}
		Encoding unicode = Encoding.Unicode;
		using MemoryStream stream = new MemoryStream(unicode.GetBytes(text));
		using Scanner scanner = new Scanner(stream, unicode, skipWhitespace: false, text.Length, enablePartialScan);
		Microsoft.Dynamics.Nav.Model.Compiler.Token tokenparameter;
		while ((tokenparameter = scanner.NextToken()) != Microsoft.Dynamics.Nav.Model.Compiler.Token.t_endofinput)
		{
			yield return new TokenInfo(tokenparameter, scanner.StartIndex, scanner.Length);
		}
	}

	public static bool HasRdlc(this IElement element)
	{
		if (element.ElementTypeInfo.ElementType == ElementType.Report)
		{
			return element.GetRdlDataLinesCount() != 0;
		}
		return false;
	}

	public static bool HasWordLayout(this IElement element)
	{
		if (element.ElementTypeInfo.ElementType == ElementType.Report)
		{
			return element.GetWordLayoutLinesCount() != 0;
		}
		return false;
	}

	public static bool IsEventDefinedByElement(IElement eventElement, IElement ownerElement)
	{
		if (eventElement.GetInt32Property(PropertyType.EventSourceId) != -ownerElement.Id)
		{
			return SemanticFacts.IsSameName(eventElement.GetStringProperty(PropertyType.EventSource), ownerElement.Name);
		}
		return true;
	}

	public static bool IsMultilanguageProperty(IElement owner, PropertyType propertyType)
	{
		return propertyType.ToString().EndsWith("ML", StringComparison.Ordinal);
	}

	internal static bool IsTriggerElementType(ElementType elementType)
	{
		switch (elementType)
		{
		case ElementType.OnPostSection:
		case ElementType.OnInitReport:
		case ElementType.OnPostReport:
		case ElementType.OnPostDataItem:
		case ElementType.OnDelete:
		case ElementType.OnInsert:
		case ElementType.OnModify:
		case ElementType.OnRename:
		case ElementType.OnValidate:
		case ElementType.OnLookup:
		case ElementType.OnRun:
		case ElementType.OnAction:
		case ElementType.OnInit:
		case ElementType.OnOpenPage:
		case ElementType.OnDrillDown:
		case ElementType.OnAfterGetRecord:
		case ElementType.OnNewRecord:
		case ElementType.OnFindRecord:
		case ElementType.OnAssistEdit:
		case ElementType.OnModifyRecord:
		case ElementType.OnInsertRecord:
		case ElementType.OnDeleteRecord:
		case ElementType.OnClosePage:
		case ElementType.OnQueryClosePage:
		case ElementType.OnNextRecord:
		case ElementType.OnOpenForm:
		case ElementType.OnPush:
		case ElementType.OnFormat:
		case ElementType.OnAfterGetCurrRecord:
		case ElementType.OnAfterValidate:
		case ElementType.OnDeactivate:
		case ElementType.OnBeforePutRecord:
		case ElementType.OnAfterInput:
		case ElementType.OnActivate:
		case ElementType.OnCloseForm:
		case ElementType.OnQueryCloseForm:
		case ElementType.OnBeforeInput:
		case ElementType.OnTimer:
		case ElementType.OnDeactivateForm:
		case ElementType.OnBeforeExportRecord:
		case ElementType.OnPreDataItem:
		case ElementType.OnBeforeImportRecord:
		case ElementType.OnAfterImportRecord:
		case ElementType.OnPreReport:
		case ElementType.OnPreSection:
		case ElementType.OnPreDataport:
		case ElementType.OnControlAddIn:
		case ElementType.OnDataPointClicked:
		case ElementType.OnDataPointDoubleClicked:
		case ElementType.OnSetBreakpoint:
		case ElementType.OnGetVariableValue:
		case ElementType.OnAddWatch:
		case ElementType.OnBeforeOpen:
		case ElementType.OnCreateHyperlink:
		case ElementType.OnHyperlink:
			return true;
		default:
			return false;
		}
	}
}
