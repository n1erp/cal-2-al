using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class PageMetadataEmitHelper
{
	internal static void WriteTriggers(XmlWriter xmlWriter, ImmutableArray<MethodSymbol> triggers)
	{
		if (triggers.Length != 0)
		{
			xmlWriter.WriteStartElement("Triggers", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			foreach (MethodSymbol item in triggers)
			{
				SourceTriggerSymbol trigger = item as SourceTriggerSymbol;
				WriteTrigger(xmlWriter, trigger);
			}
			xmlWriter.WriteEndElement();
		}
	}

	internal static void WriteControlTriggers(XmlWriter xmlWriter, ImmutableArray<MethodSymbol> triggers)
	{
		foreach (MethodSymbol item in triggers)
		{
			SourceTriggerSymbol trigger = item as SourceTriggerSymbol;
			WriteTrigger(xmlWriter, trigger, isControlTrigger: true);
		}
	}

	internal static bool TryUseSpecialPropertyValueMetadataEmitter(XmlWriter xmlWriter, PropertySymbol property)
	{
		PropertyKind propertyKind = property.PropertyKind;
		if (propertyKind == PropertyKind.Visible)
		{
			WritePropertyModifiedInUserPersonalization(xmlWriter, property);
		}
		switch (property.Property.PropertyValue.Kind)
		{
		case BoundKind.TableFilterStatementPropertyValue:
			WriteTableFilterStatementPropertyValue(xmlWriter, property);
			return true;
		case BoundKind.TableViewPropertyValue:
			WriteTableViewMetadataPropertyValue(xmlWriter, property);
			return true;
		case BoundKind.TableRelationPropertyValue:
			PropertyExpressionEmitterHelper.WriteTableRelationPropertyValue(xmlWriter, property);
			return true;
		case BoundKind.BadPropertyValue:
			if (property.DeclaringCompilation.Options.Target.IsPersonalizationTarget())
			{
				return true;
			}
			DebugAssertHelper.Fail("Bad properties should not be emitted.");
			return true;
		default:
			switch (property.PropertyKind)
			{
			case PropertyKind.DataCaptionExpression:
				WriteDataCaptionExpressionPropertyValue(xmlWriter, property);
				return true;
			case PropertyKind.IndentationColumn:
				WriteIndentationColumnPropertyValue(xmlWriter, property);
				return true;
			case PropertyKind.RunObject:
				WriteRunObject(xmlWriter, property);
				return true;
			case PropertyKind.CaptionClass:
				WriteCaptionClassPropertyValue(xmlWriter, property);
				return true;
			case PropertyKind.Permissions:
				WriteIndirectPermissionsPropertyValue(xmlWriter, property);
				return true;
			default:
				return false;
			}
		}
	}

	internal static void WriteContextSpecificHelpUrlPropertyIfNeeded(XmlWriter xmlWriter, Symbol objectSymbol, PropertySymbol property)
	{
		CompilationOptions options = objectSymbol.DeclaringCompilation.Options;
		string contextSensitiveHelpUrl = GetContextSensitiveHelpUrl(property, options);
		if (!string.IsNullOrEmpty(contextSensitiveHelpUrl))
		{
			xmlWriter.WriteAttributeString("HelpLink", contextSensitiveHelpUrl);
		}
	}

	internal static string GetContextSensitiveHelpUrl(PropertySymbol property, CompilationOptions options)
	{
		if (string.IsNullOrEmpty(options.ContextSensitiveHelpUrl))
		{
			return null;
		}
		if (property == null)
		{
			return options.ContextSensitiveHelpUrl;
		}
		return options.ContextSensitiveHelpUrl + property.ValueText;
	}

	internal static void WriteSupportedLocales(XmlWriter xmlWriter, Symbol symbol)
	{
		NavAppSupportedLocales supportedLocales = symbol.DeclaringCompilation.Options.SupportedLocales;
		if (!(supportedLocales == null) && !supportedLocales.IsEmpty)
		{
			xmlWriter.WriteAttributeString("SupportedLocales", supportedLocales.ToString());
		}
	}

	private static void WriteCaptionClassPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		xmlWriter.WriteAttributeString(property.MetadataName, "SourceExpr");
	}

	private static void WriteIndirectPermissionsPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		if (property.GetContainingPage().RelatedTable != null)
		{
			BoundPermissionPropertyValue boundPermissionPropertyValue = (BoundPermissionPropertyValue)property.Property.PropertyValue;
			string value = string.Join(", ", boundPermissionPropertyValue.IndirectPermissionValues);
			xmlWriter.WriteAttributeString("IndirectPermissions", value);
		}
	}

	internal static void WriteTrigger(XmlWriter xmlWriter, ImmutableArray<MethodSymbol> triggers)
	{
		if (triggers.Any())
		{
			DebugAssertHelper.Assert(triggers.Length == 1);
			SourceTriggerSymbol trigger = triggers[0] as SourceTriggerSymbol;
			WriteTrigger(xmlWriter, trigger);
		}
	}

	internal static void WriteTrigger(XmlWriter xmlWriter, SourceTriggerSymbol trigger, bool isControlTrigger = false)
	{
		string localName = (isControlTrigger ? "Triggers" : "Trigger");
		xmlWriter.WriteStartElement(localName);
		xmlWriter.WriteAttributeString("TriggerType", trigger.TriggerInfo.Name);
		xmlWriter.WriteAttributeString("Name", trigger.MetadataName);
		xmlWriter.WriteEndElement();
	}

	internal static void WriteRunObject(XmlWriter xmlWriter, PropertySymbol property)
	{
		BoundApplicationObjectReferencePropertyValue boundApplicationObjectReferencePropertyValue = (BoundApplicationObjectReferencePropertyValue)property.Property.PropertyValue;
		TableTypeSymbol runObjectSourceTable = GetRunObjectSourceTable(xmlWriter, property.GetContainingPage(), boundApplicationObjectReferencePropertyValue);
		WriteRunObjectAttributes(xmlWriter, boundApplicationObjectReferencePropertyValue.Value.Id, boundApplicationObjectReferencePropertyValue.Value.Kind, runObjectSourceTable?.Id);
	}

	internal static void WriteRunObjectAttributes(XmlWriter xmlWriter, int targetId, SymbolKind runObjectKind, int? sourceTableId, string runObjectView = null)
	{
		xmlWriter.WriteAttributeString("PushAction", "RunObject");
		xmlWriter.WriteAttributeString("RunObjectType", MetadataEmitterHelper.GetRunObjectTypeMetadataValue(runObjectKind));
		xmlWriter.WriteAttributeString("TargetID", targetId.ToString(CultureInfo.InvariantCulture));
		if (sourceTableId.HasValue)
		{
			xmlWriter.WriteAttributeString("RunObjectSrcTable", sourceTableId.Value.ToString(CultureInfo.InvariantCulture));
		}
		if (runObjectView != null)
		{
			xmlWriter.WriteAttributeString("RunObjectView", runObjectView);
		}
	}

	internal static void WritePropertyModifiedInUserPersonalization(XmlWriter xmlWriter, PropertySymbol property)
	{
		if (property.DeclaringCompilation.Options.Target.IsPersonalizationTarget() && property.GetContainingApplicationObjectTypeSymbol().Kind == SymbolKind.PageCustomization)
		{
			xmlWriter.WriteAttributeString("PersonalizedProperties", property.MetadataName);
		}
	}

	internal static void WriteControlExpressions(XmlWriter xmlWriter, SourceControlSymbol control, HashSet<string> alreadyAddedSourceExpressions)
	{
		switch (control.ControlKind)
		{
		case ControlKind.Field:
			WriteFieldControlExpressions(xmlWriter, control, alreadyAddedSourceExpressions);
			break;
		case ControlKind.Group:
		case ControlKind.CueGroup:
		case ControlKind.Repeater:
		case ControlKind.Fixed:
		case ControlKind.Grid:
			WriteGroupControlExpressions(xmlWriter, control);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(control.ControlKind);
		case ControlKind.Area:
		case ControlKind.Part:
		case ControlKind.SystemPart:
		case ControlKind.Label:
		case ControlKind.UserControl:
		case ControlKind.ChartPart:
			break;
		}
	}

	internal static void WriteGroupControlExpressions(XmlWriter xmlWriter, SourceControlSymbol control)
	{
		switch (control.ControlKind)
		{
		case ControlKind.Repeater:
			WriteRepeaterControlExpressions(xmlWriter, control);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(control.ControlKind);
		case ControlKind.Group:
		case ControlKind.CueGroup:
		case ControlKind.Fixed:
		case ControlKind.Grid:
			break;
		}
	}

	internal static void WriteRepeaterControlExpressions(XmlWriter xmlWriter, SourceControlSymbol control)
	{
		DebugAssertHelper.Assert(control.ControlKind == ControlKind.Repeater);
		WriteIndentationColumnExpression(xmlWriter, control);
	}

	internal static void WriteFieldControlExpressions(XmlWriter xmlWriter, SourceControlSymbol control, HashSet<string> alreadyAddedSourceExpressions)
	{
		DebugAssertHelper.Assert(control.ControlKind == ControlKind.Field);
		WriteTypeSpecificFieldExpressions(xmlWriter, control);
		string item = control.SourceExpression?.Syntax.ToString();
		if (!control.IsSourceRecordField && (!alreadyAddedSourceExpressions.Contains(item) || ObjectMetadataEmitHelper.AllowControlSourceExpressionDuplicate(control.SourceExpression)))
		{
			alreadyAddedSourceExpressions.Add(item);
			WriteControlSourceExpression(xmlWriter, control);
		}
	}

	internal static bool IsDataCaptionExpressionRecordAccess(BoundExpression dataCaptionExpression)
	{
		if (dataCaptionExpression.Kind != BoundKind.FieldAccess)
		{
			return false;
		}
		BoundFieldAccess boundFieldAccess = (BoundFieldAccess)dataCaptionExpression;
		if (boundFieldAccess.ReceiverOpt == null || boundFieldAccess.ReceiverOpt.Kind != BoundKind.Global)
		{
			return false;
		}
		return true;
	}

	internal static void WriteIndentationColumnExpression(XmlWriter xmlWriter, ContainerSymbol controlOrModification)
	{
		controlOrModification.GetExtendedPageControlOrSelf();
		BoundCodeExpressionPropertyValue boundPropertyValue = controlOrModification.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.IndentationColumn);
		if (boundPropertyValue != null && MetadataEmitterHelper.IsIndentationColumnExpression(boundPropertyValue.Expression))
		{
			xmlWriter.WriteStartElement("Expression");
			string indentExpressionName = GetIndentExpressionName(controlOrModification);
			xmlWriter.WriteAttributeString("Name", indentExpressionName);
			xmlWriter.WriteAttributeString("Datatype", boundPropertyValue.Expression.Type.GetNavTypeKindSafe().ToString());
			xmlWriter.WriteAttributeString("ExpressionType", "Property");
			xmlWriter.WriteAttributeString("SourceExpression", indentExpressionName);
			xmlWriter.WriteEndElement();
		}
	}

	internal static void WriteTypeSpecificFieldExpressions(XmlWriter xmlWriter, SourceControlSymbol control)
	{
		NavTypeKind navTypeKind = control.Type.NavTypeKind;
		if (navTypeKind == NavTypeKind.Decimal)
		{
			xmlWriter.WriteStartElement("Expression");
			string formatExpressionName = MetadataEmitterHelper.GetFormatExpressionName(control.Id);
			xmlWriter.WriteAttributeString("Name", formatExpressionName);
			xmlWriter.WriteAttributeString("Datatype", "OemText");
			xmlWriter.WriteAttributeString("ExpressionType", "Property");
			xmlWriter.WriteAttributeString("SourceExpression", formatExpressionName);
			xmlWriter.WriteEndElement();
		}
		if (control.IsCueControl() && control.IsSourceRecordField)
		{
			xmlWriter.WriteStartElement("Expression");
			string dynamicCueStyleExpressionName = GetDynamicCueStyleExpressionName(control);
			xmlWriter.WriteAttributeString("Name", dynamicCueStyleExpressionName);
			xmlWriter.WriteAttributeString("Datatype", "Text");
			xmlWriter.WriteAttributeString("ExpressionType", "DynamicCueStyle");
			xmlWriter.WriteAttributeString("IsDataSetVar", "1");
			xmlWriter.WriteAttributeString("SourceExpression", dynamicCueStyleExpressionName);
			xmlWriter.WriteEndElement();
		}
	}

	internal static void WriteTypeSpecificControlSourceExpressionAttributes(XmlWriter xmlWriter, SourceControlSymbol control)
	{
		switch (control.SourceExpression.Type.NavTypeKind)
		{
		case NavTypeKind.Option:
		{
			OptionTypeSymbol optionTypeSymbol = (OptionTypeSymbol)control.SourceExpression.Type;
			xmlWriter.WriteAttributeString("OptionString", optionTypeSymbol.OptionMembers);
			break;
		}
		case NavTypeKind.Enum:
		{
			EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)control.SourceExpression.Type;
			xmlWriter.WriteAttributeString("EnumId", enumTypeSymbol.Id.ToString(CultureInfo.InvariantCulture));
			break;
		}
		}
	}

	internal static void WriteControlSourceExpression(XmlWriter xmlWriter, SourceControlSymbol control)
	{
		xmlWriter.WriteStartElement("Expression");
		xmlWriter.WriteAttributeString("Name", "Control" + control.Id.ToString(CultureInfo.InvariantCulture));
		WriteControlSourceExpression(xmlWriter, control.SourceExpression);
		if (control.Type.HasLength)
		{
			xmlWriter.WriteAttributeString("DataLength", control.Type.Length.ToString(CultureInfo.InvariantCulture));
		}
		if (control.ContainingSymbol.IsRepeaterGroup())
		{
			xmlWriter.WriteAttributeString("InRepeaterGroup", "1");
		}
		xmlWriter.WriteAttributeString("ExpressionType", "SourceExpression");
		xmlWriter.WriteAttributeString("SourceExpression", control.SourceExpression.Syntax.ToString());
		if (control.SourceExpression.IsAssignable)
		{
			xmlWriter.WriteAttributeString("ExpressionIsAssignable", "1");
		}
		WriteTypeSpecificControlSourceExpressionAttributes(xmlWriter, control);
		xmlWriter.WriteEndElement();
	}

	internal static void WriteControlSourceExpression(XmlWriter xmlWriter, BoundExpression sourceExpression)
	{
		BoundKind kind = sourceExpression.Kind;
		if (kind == BoundKind.FieldAccess)
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)sourceExpression;
			DebugAssertHelper.Assert(boundFieldAccess.FieldSymbol.ContainingType.NavTypeKind == NavTypeKind.Record || boundFieldAccess.FieldSymbol.ContainingType.NavTypeKind == NavTypeKind.TableExtension);
			TableTypeSymbol relatedTableSymbol = boundFieldAccess.FieldSymbol.ContainingType.GetRelatedTableSymbol();
			xmlWriter.WriteAttributeString("TableNo", relatedTableSymbol.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("FieldNo", boundFieldAccess.FieldSymbol.Id.ToString(CultureInfo.InvariantCulture));
		}
		else
		{
			xmlWriter.WriteAttributeString("Datatype", GetControlSourceExpressionDataType(sourceExpression));
		}
	}

	private static string GetControlSourceExpressionDataType(BoundExpression sourceExpression)
	{
		if (sourceExpression.Type.NavTypeKind == NavTypeKind.String)
		{
			return "OemText";
		}
		return sourceExpression.Type.NavTypeKind.GetNavTypeEnumNameString();
	}

	internal static void WriteVariableInDataSetExpression(XmlWriter xmlWriter, VariableSymbol global)
	{
		xmlWriter.WriteStartElement("Expression");
		xmlWriter.WriteAttributeString("Name", GetPrefixedExpressionName(global.Name, global));
		string value = global.Type.Name;
		switch (global.Type.Kind)
		{
		case SymbolKind.Enum:
		{
			EnumTypeSymbol enumTypeSymbol = global.Type as EnumTypeSymbol;
			value = "Option";
			xmlWriter.WriteAttributeString("EnumId", enumTypeSymbol.Id.ToString(CultureInfo.InvariantCulture));
			break;
		}
		case SymbolKind.OptionType:
		{
			OptionTypeSymbol optionTypeSymbol = global.Type as OptionTypeSymbol;
			xmlWriter.WriteAttributeString("OptionString", optionTypeSymbol.OptionMembers);
			break;
		}
		}
		xmlWriter.WriteAttributeString("Datatype", value);
		xmlWriter.WriteAttributeString("IsDataSetVar", "1");
		if (global.Type.HasLength)
		{
			xmlWriter.WriteAttributeString("DataLength", global.Type.Length.ToString(CultureInfo.InvariantCulture));
		}
		xmlWriter.WriteAttributeString("ExpressionType", "VariableIncludedInDataSet");
		xmlWriter.WriteAttributeString("SourceExpression", global.Name);
		if (global.IsWritable)
		{
			xmlWriter.WriteAttributeString("ExpressionIsAssignable", "1");
		}
		xmlWriter.WriteEndElement();
	}

	internal static string GetPrefixedExpressionName(string expressionName, Symbol applicationId)
	{
		IApplicationObjectTypeSymbol containingSymbolOfType = applicationId.GetContainingSymbolOfType<IApplicationObjectTypeSymbol>();
		if (containingSymbolOfType != null)
		{
			SymbolKind kind = containingSymbolOfType.Kind;
			string text = ((kind != SymbolKind.PageExtension) ? "p" : "px");
			return text + containingSymbolOfType.Id.ToString(CultureInfo.InvariantCulture) + expressionName;
		}
		return expressionName;
	}

	internal static string GetIndentExpressionName(Symbol controlOrModification)
	{
		ControlSymbol extendedPageControlOrSelf = controlOrModification.GetExtendedPageControlOrSelf();
		return GetPrefixedExpressionName(string.Format(CultureInfo.InvariantCulture, "Control{0}_Indent", extendedPageControlOrSelf.Id), controlOrModification);
	}

	internal static string GetDynamicCueStyleExpressionName(SourceControlSymbol control)
	{
		return string.Format(CultureInfo.InvariantCulture, "Control{0}_DynamicCueStyle", control.Id);
	}

	internal static void WriteTypeSpecificFieldAttributes(XmlWriter xmlWriter, SourceControlSymbol control)
	{
		DebugAssertHelper.Assert(control.ControlKind == ControlKind.Field);
		NavTypeKind navTypeKind = control.Type.NavTypeKind;
		if (navTypeKind == NavTypeKind.Decimal)
		{
			xmlWriter.WriteAttributeString("FormatColumnName", MetadataEmitterHelper.GetFormatExpressionName(control.Id));
		}
	}

	private static TableTypeSymbol GetRunObjectSourceTable(XmlWriter xmlWriter, IPageBaseTypeSymbol containingPage, BoundApplicationObjectReferencePropertyValue objectReference)
	{
		TableTypeSymbol result = null;
		switch (objectReference.Value.Kind)
		{
		case SymbolKind.Page:
			result = (objectReference.Value as PageTypeSymbol).GetRelatedTableSymbol();
			break;
		case SymbolKind.Report:
		case SymbolKind.XmlPort:
			result = containingPage.GetRelatedTableSymbol();
			break;
		}
		return result;
	}

	internal static void WriteIndentationColumnPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		BoundExpression boundExpression = (BoundExpression)property.Value;
		property.ContainingSymbol.GetExtendedPageControlOrSelf();
		xmlWriter.WriteAttributeString(value: (!MetadataEmitterHelper.IsIndentationColumnExpression(boundExpression)) ? ((BoundFieldAccess)boundExpression).FieldSymbol.Id.ToString(CultureInfo.InvariantCulture) : GetIndentExpressionName(property.ContainingSymbol), localName: property.MetadataName);
	}

	internal static void WriteDataCaptionExpressionPropertyValue(XmlWriter xmlWriter, PropertySymbol dataCaptionExprProperty)
	{
		DebugAssertHelper.Assert(dataCaptionExprProperty != null);
		xmlWriter.WriteAttributeString(dataCaptionExprProperty.MetadataName, "DataCaptionExprCode");
	}

	internal static void WriteTableViewMetadataPropertyValue(XmlWriter xmlWriter, PropertySymbol propertySymbol)
	{
		BoundTableViewPropertyValue boundTableViewPropertyValue = (BoundTableViewPropertyValue)propertySymbol.Property.PropertyValue;
		PropertyTypeInfo propertyTypeInfo = boundTableViewPropertyValue.GetPropertyTypeInfo();
		DebugAssertHelper.Assert(propertyTypeInfo != null);
		string localName;
		string filterGroup;
		switch (propertyTypeInfo.Kind)
		{
		default:
			localName = string.Empty;
			filterGroup = string.Empty;
			break;
		case PropertyKind.SourceTableView:
			localName = "SourceTableView";
			filterGroup = "2";
			break;
		case PropertyKind.SubPageView:
			localName = "SubFormView";
			filterGroup = "4";
			break;
		case PropertyKind.RunPageView:
		case PropertyKind.Filters:
			localName = "RunFormView";
			filterGroup = "3";
			break;
		case PropertyKind.DataItemTableView:
			localName = "DataItemTableView";
			filterGroup = "2";
			break;
		}
		xmlWriter.WriteStartElement(localName);
		WriteSortingElement(xmlWriter, boundTableViewPropertyValue);
		if (boundTableViewPropertyValue.TableFilter != null)
		{
			PropertyExpressionEmitterHelper.WriteTableFilters(xmlWriter, isCondition: false, "TableFilters", boundTableViewPropertyValue.TableFilter, filterGroup);
		}
		xmlWriter.WriteEndElement();
	}

	internal static void WriteSortingElement(XmlWriter xmlWriter, BoundTableViewPropertyValue tableView)
	{
		xmlWriter.WriteStartElement("Sorting");
		string value = "";
		string value2 = "0";
		string value3 = "0";
		string value4 = "1";
		if (tableView.Sorting != null && tableView.Sorting.Length > 0)
		{
			value = MetadataEmitterHelper.GetFieldPrefixedAttributeValues(tableView.Sorting.Select((BoundExpression v) => v.ExpressionSymbol).Cast<FieldSymbol>());
			value2 = "1";
		}
		if (tableView.IsAscending.HasValue)
		{
			value3 = "1";
			if (!tableView.IsAscending.Value)
			{
				value4 = "0";
			}
		}
		xmlWriter.WriteAttributeString("KeyFields", value);
		xmlWriter.WriteAttributeString("KeyFieldsSetByView", value2);
		xmlWriter.WriteAttributeString("AscendingSetByView", value3);
		xmlWriter.WriteAttributeString("Ascending", value4);
		xmlWriter.WriteEndElement();
	}

	internal static void WriteTableFilterStatementPropertyValue(XmlWriter xmlWriter, PropertySymbol propertySymbol)
	{
		BoundTableFilterStatementPropertyValue value = (BoundTableFilterStatementPropertyValue)propertySymbol.Property.PropertyValue;
		PropertyTypeInfo propertyTypeInfo = value.GetPropertyTypeInfo();
		DebugAssertHelper.Assert(propertyTypeInfo != null);
		string filterGroup;
		switch (propertyTypeInfo.Kind)
		{
		default:
		{
			string elementName = string.Empty;
			filterGroup = string.Empty;
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, elementName, () =>
			{
				PropertyExpressionEmitterHelper.WriteTableFilters(xmlWriter, isCondition: false, string.Empty, value.TableFilter, filterGroup);
			});
			break;
		}
		case PropertyKind.SubPageLink:
		{
			string elementName = "SubFormLink";
			filterGroup = "4";
			PropertyExpressionEmitterHelper.WriteTableFilters(xmlWriter, isCondition: false, elementName, value.TableFilter, filterGroup);
			break;
		}
		case PropertyKind.RunPageLink:
		{
			string elementName = "RunFormLink";
			filterGroup = "0";
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, elementName, () =>
			{
				PropertyExpressionEmitterHelper.WriteTableFilters(xmlWriter, isCondition: false, "TableFilters", value.TableFilter, filterGroup);
			});
			break;
		}
		}
	}
}
