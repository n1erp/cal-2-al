using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class PageBaseMetadataEmitter<T> : ObjectMetadataEmitter<T> where T : ObjectTypeSymbol
{
	protected readonly ExtensionEmitValues ExtensionEmitValues;

	protected readonly string SourceAppId;

	private static readonly string AllApplicationArea = "#All";

	internal PageBaseMetadataEmitter(T symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		ExtensionEmitValues = ModuleBuilder.Options.ExtensionEmitValues;
		if (ExtensionEmitValues != null)
		{
			SourceAppId = ExtensionEmitValues.AppId.ToString();
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal void WriteControl(XmlWriter xmlWriter, SourceControlSymbol control, Dictionary<string, string> addedSourceExpressions)
	{
		if (control.ControlKind == ControlKind.Area)
		{
			xmlWriter.WriteStartElement("Containers");
			xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", "ControlContainerDefinition");
			xmlWriter.WriteAttributeString("ContainerType", MetadataEmitterHelper.GetContainerType(SyntaxFacts.GetAreaKind(control.Name)));
		}
		else
		{
			xmlWriter.WriteStartElement("Controls");
			xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", MetadataEmitterHelper.GetControlType(control.ControlKind));
			xmlWriter.WriteAttributeString("ID", control.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("Name", GetControlName(control));
			ApplicationObjectTypeSymbol containingSymbolOfType = control.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
			xmlWriter.WriteAttributeString("ControlGUID", MetadataEmitterHelper.GeneratePageControlGuid(control.Id, containingSymbolOfType.Id, containingSymbolOfType.Kind));
			if (control.ControlKind == ControlKind.Field)
			{
				BoundFieldAccess sourceRecordFieldAccess = control.GetSourceRecordFieldAccess();
				string key = control.SourceExpression.Syntax.ToString();
				string text = sourceRecordFieldAccess?.FieldSymbol.Id.ToString(CultureInfo.InvariantCulture);
				if (text == null)
				{
					if (addedSourceExpressions.ContainsKey(key))
					{
						text = ((!addedSourceExpressions.ContainsKey(key) || !ObjectMetadataEmitHelper.AllowControlSourceExpressionDuplicate(control.SourceExpression)) ? addedSourceExpressions[key] : ("Control" + control.Id.ToString(CultureInfo.InvariantCulture)));
					}
					else
					{
						text = "Control" + control.Id.ToString(CultureInfo.InvariantCulture);
						addedSourceExpressions.Add(key, text);
					}
				}
				xmlWriter.WriteAttributeString("DataColumnName", text);
				ApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbol = sourceRecordFieldAccess?.FieldSymbol.GetContainingSymbolOfType<ApplicationObjectExtensionTypeSymbol>();
				if (applicationObjectExtensionTypeSymbol != null)
				{
					xmlWriter.WriteAttributeString("ExtensionId", applicationObjectExtensionTypeSymbol.Id.ToString(CultureInfo.InvariantCulture));
				}
				if (!control.SourceExpression.IsAssignable)
				{
					xmlWriter.WriteAttributeString("SourceExpressionIsAssignable", "0");
				}
				PageMetadataEmitHelper.WriteTypeSpecificFieldAttributes(xmlWriter, control);
			}
			if (control.ControlKind == ControlKind.Part)
			{
				PageTypeSymbol relatedPartSymbol = control.RelatedPartSymbol;
				string value = relatedPartSymbol.Id.ToString(CultureInfo.InvariantCulture);
				xmlWriter.WriteAttributeString("PagePartID", value);
				TableTypeSymbol relatedTable = relatedPartSymbol.RelatedTable;
				if (relatedTable != null)
				{
					xmlWriter.WriteAttributeString("RunObjectSrcTable", relatedTable.Id.ToString(CultureInfo.InvariantCulture));
				}
			}
			if (control.ControlKind == ControlKind.SystemPart)
			{
				string systemPartType = MetadataEmitterHelper.GetSystemPartType(SyntaxFacts.GetSystemPartKind(((PageSystemPartSyntax)control.ControlBaseSyntax).SystemPartType.Identifier.Text));
				xmlWriter.WriteAttributeString("SystemPartID", systemPartType);
			}
			if (control.ControlKind == ControlKind.ChartPart)
			{
				string identifierOrLiteralValue = ((PageChartPartSyntax)control.ControlBaseSyntax).ChartPartType.GetIdentifierOrLiteralValue();
				xmlWriter.WriteAttributeString("ChartPartID", identifierOrLiteralValue);
			}
			if (control.ControlKind == ControlKind.UserControl)
			{
				xmlWriter.WriteAttributeString("ControlAddIn", control.RelatedControlAddInSymbol.MetadataName + ";PublicKeyToken=" + control.RelatedControlAddInSymbol.PublicKeyToken);
			}
		}
		WriteExtensionSpecificMetadataAttributesIfNeeded(xmlWriter);
		WriteContextSpecificHelpUrlPropertyIfNeeded(xmlWriter, control);
		IEnumerable<PropertySymbol> enumerable = control.Properties;
		if (control.ControlKind == ControlKind.Group && control.GetProperty(PropertyKind.ShowCaption) != null && control.GetBooleanPropertyValue(PropertyKind.ShowCaption) == false)
		{
			ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, "CaptionML", "ENU=", emitAsAttribute: true);
			enumerable = enumerable.Where((PropertySymbol p) => p.PropertyKind != PropertyKind.Caption && p.PropertyKind != PropertyKind.CaptionML);
		}
		WriteProperties(xmlWriter, control, enumerable, shouldOutputDefaultValues: true);
		WriteActions(xmlWriter, control.Actions);
		foreach (SourceControlSymbol item in control.Controls.Cast<SourceControlSymbol>())
		{
			WriteControl(xmlWriter, item, addedSourceExpressions);
		}
		if (control.ControlKind != ControlKind.UserControl)
		{
			PageMetadataEmitHelper.WriteControlTriggers(xmlWriter, control.Triggers);
		}
		xmlWriter.WriteEndElement();
	}

	private static string GetControlName(SourceControlSymbol control)
	{
		if (control.ControlKind == ControlKind.UserControl)
		{
			return control.MetadataName;
		}
		return control.Name;
	}

	internal void WriteActions(XmlWriter xmlWriter, ImmutableArray<ActionSymbol> actions)
	{
		foreach (ActionSymbol item in actions)
		{
			WriteAction(xmlWriter, item);
		}
	}

	internal void WriteAction(XmlWriter xmlWriter, ActionSymbol action)
	{
		if (action.ActionKind == ActionKind.Area)
		{
			xmlWriter.WriteStartElement("ActionContainers", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", "ActionContainerDefinition");
			xmlWriter.WriteAttributeString("ID", action.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("ActionContainerType", MetadataEmitterHelper.GetContainerType(SyntaxFacts.GetActionAreaKind(action.Name)));
		}
		else
		{
			string value = action.ActionKind switch
			{
				ActionKind.Group => "ActionGroupDefinition", 
				ActionKind.Separator => "SeparatorDefinition", 
				_ => "ActionDefinition", 
			};
			xmlWriter.WriteStartElement("Actions");
			xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", value);
			xmlWriter.WriteAttributeString("ID", action.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("Name", action.Name);
			ApplicationObjectTypeSymbol containingSymbolOfType = action.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
			xmlWriter.WriteAttributeString("ControlGUID", MetadataEmitterHelper.GeneratePageControlGuid(action.Id, containingSymbolOfType.Id, containingSymbolOfType.Kind));
		}
		WriteExtensionSpecificMetadataAttributesIfNeeded(xmlWriter);
		WriteContextSpecificHelpUrlPropertyIfNeeded(xmlWriter, action);
		WriteProperties(xmlWriter, action, action.Properties, shouldOutputDefaultValues: true);
		foreach (SourceActionSymbol item in action.Actions.Cast<SourceActionSymbol>())
		{
			WriteAction(xmlWriter, item);
		}
		PageMetadataEmitHelper.WriteTrigger(xmlWriter, action.Triggers);
		xmlWriter.WriteEndElement();
	}

	internal void WriteViewActions(XmlWriter xmlWriter, ImmutableArray<ViewSymbol> views)
	{
		int num = 0;
		xmlWriter.WriteStartElement("ActionContainers", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
		xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", "ActionContainerDefinition");
		xmlWriter.WriteAttributeString("ID", num.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("ActionContainerType", "ViewActions");
		foreach (ViewSymbol item in views)
		{
			WriteViewAction(xmlWriter, item);
		}
		xmlWriter.WriteEndElement();
	}

	internal void WriteViewAction(XmlWriter xmlWriter, ViewSymbol view)
	{
		string text = CreateUniqueViewName(view);
		xmlWriter.WriteStartElement("Actions");
		xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", "ActionDefinition");
		xmlWriter.WriteAttributeString("ID", view.Id.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("Name", text);
		var (targetId, sourceTableId) = GetRunObjectTargetInformation(view);
		PageMetadataEmitHelper.WriteRunObjectAttributes(xmlWriter, targetId, SymbolKind.Page, sourceTableId, text);
		ApplicationObjectTypeSymbol containingSymbolOfType = view.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		xmlWriter.WriteAttributeString("ControlGUID", MetadataEmitterHelper.GeneratePageControlGuid(view.Id, containingSymbolOfType.Id, containingSymbolOfType.Kind));
		WriteExtensionSpecificMetadataAttributesIfNeeded(xmlWriter);
		PropertySymbol filters = null;
		PropertySymbol orderBy = null;
		bool flag = false;
		PooledList<PropertySymbol> instance = PooledList<PropertySymbol>.GetInstance();
		try
		{
			foreach (PropertySymbol property in view.Properties)
			{
				if (IsViewActionProperty(property))
				{
					if (property.PropertyKind == PropertyKind.Filters)
					{
						filters = property;
					}
					else if (property.PropertyKind == PropertyKind.OrderBy)
					{
						orderBy = property;
					}
					else if (property.PropertyKind == PropertyKind.Caption || property.PropertyKind == PropertyKind.CaptionML)
					{
						flag = true;
						instance.Add(property);
					}
					else
					{
						instance.Add(property);
					}
				}
			}
			xmlWriter.WriteAttributeString("ApplicationArea", AllApplicationArea);
			if (!flag)
			{
				xmlWriter.WriteAttributeString("CaptionML", "ENU=" + view.Name);
			}
			WriteProperties(xmlWriter, view, instance, shouldOutputDefaultValues: false);
			WriteViewActionFilters(xmlWriter, filters, orderBy);
		}
		finally
		{
			instance.Free();
		}
		xmlWriter.WriteEndElement();
	}

	private static void WriteViewActionFilters(XmlWriter xmlWriter, PropertySymbol filters, PropertySymbol orderBy)
	{
		bool flag = orderBy != null;
		bool flag2 = filters != null;
		if (flag || flag2)
		{
			if (!flag & flag2)
			{
				PageMetadataEmitHelper.WriteTableViewMetadataPropertyValue(xmlWriter, filters);
				return;
			}
			xmlWriter.WriteStartElement("RunFormView");
			WriteViewSorting(xmlWriter, orderBy);
			WriteViewFilters(xmlWriter, filters);
			xmlWriter.WriteEndElement();
		}
	}

	private static void WriteViewSorting(XmlWriter xmlWriter, PropertySymbol orderBy)
	{
		BoundOrderByPropertyValue boundOrderByPropertyValue = (BoundOrderByPropertyValue)orderBy.Property.PropertyValue;
		xmlWriter.WriteStartElement("Sorting");
		string fieldPrefixedAttributeValues = MetadataEmitterHelper.GetFieldPrefixedAttributeValues(boundOrderByPropertyValue.Elements.Select((BoundOrderByExpression ex) => ((BoundFieldAccess)ex.Value).FieldSymbol).Cast<FieldSymbol>());
		string value = (boundOrderByPropertyValue.Elements[0].IsAscending ? "1" : "0");
		xmlWriter.WriteAttributeString("KeyFields", fieldPrefixedAttributeValues);
		xmlWriter.WriteAttributeString("KeyFieldsSetByView", "1");
		xmlWriter.WriteAttributeString("AscendingSetByView", "1");
		xmlWriter.WriteAttributeString("Ascending", value);
		xmlWriter.WriteEndElement();
	}

	private static void WriteViewFilters(XmlWriter xmlWriter, PropertySymbol filters)
	{
		if (filters != null)
		{
			BoundTableViewPropertyValue boundTableViewPropertyValue = (BoundTableViewPropertyValue)filters.Property.PropertyValue;
			PropertyExpressionEmitterHelper.WriteTableFilters(xmlWriter, isCondition: false, "TableFilters", boundTableViewPropertyValue.TableFilter, "3");
		}
	}

	private static (int runPageId, int? runTableId) GetRunObjectTargetInformation(ViewSymbol view)
	{
		PageTypeSymbol pageTypeSymbol = view.GetContainingPage() as PageTypeSymbol;
		if (pageTypeSymbol != null)
		{
			return (runPageId: pageTypeSymbol.Id, runTableId: pageTypeSymbol.GetRelatedTableSymbol()?.Id);
		}
		PageExtensionBaseTypeSymbol containingSymbolOfKind = view.GetContainingSymbolOfKind<PageExtensionBaseTypeSymbol>(SymbolKind.PageExtension, SymbolKind.PageCustomization);
		if (containingSymbolOfKind != null)
		{
			return (runPageId: containingSymbolOfKind.Target.Id, runTableId: containingSymbolOfKind.Target.GetRelatedTableSymbol()?.Id);
		}
		DebugAssertHelper.Fail("Could not find the page target of the view " + view?.Name);
		return (runPageId: 0, runTableId: null);
	}

	protected void WriteViewDefinitions(XmlWriter xmlWriter, IEnumerable<ViewSymbol> views)
	{
		xmlWriter.WriteStartElement("ViewContainers", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
		xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", "ViewContainerDefinition");
		WriteViewDefinitionsList(xmlWriter, views);
		xmlWriter.WriteEndElement();
	}

	protected void WriteViewDefinitionsList(XmlWriter xmlWriter, IEnumerable<ViewSymbol> views)
	{
		foreach (ViewSymbol view in views)
		{
			WriteViewDefinition(xmlWriter, view);
		}
	}

	private void WriteViewDefinition(XmlWriter xmlWriter, ViewSymbol view)
	{
		string value = CreateUniqueViewName(view);
		xmlWriter.WriteStartElement("Views");
		xmlWriter.WriteAttributeString("type", "http://www.w3.org/2001/XMLSchema-instance", "PageViewDefinition");
		xmlWriter.WriteAttributeString("Name", value);
		WriteExtensionSpecificMetadataAttributesIfNeeded(xmlWriter);
		IEnumerable<PropertySymbol> properties = view.Properties.Where((PropertySymbol x) => !IsViewActionProperty(x));
		WriteProperties(xmlWriter, view, properties, shouldOutputDefaultValues: false);
		WriteViewLayoutChanges(xmlWriter, view);
		xmlWriter.WriteEndElement();
	}

	private static string CreateUniqueViewName(ViewSymbol view)
	{
		SymbolReferenceSpecification moduleInfo = view.DeclaringCompilation.ModuleInfo;
		if (moduleInfo == null)
		{
			DebugAssertHelper.Fail("Module info for view " + view.Name + " could not be found");
			return view.Name;
		}
		return CompilationUtilities.CreateEscapedViewName(moduleInfo.AppId, view.Name);
	}

	private void WriteViewLayoutChanges(XmlWriter xmlWriter, ViewSymbol view)
	{
		xmlWriter.WriteStartElement("ViewLayoutDelta");
		xmlWriter.WriteCData(GetViewLayoutChanges(view));
		xmlWriter.WriteEndElement();
	}

	private string GetViewLayoutChanges(ViewSymbol view)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = true
		};
		using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
		{
			xmlWriter.WriteStartElement("MetadataRuntimeDeltas");
			foreach (SourceChangeModifySymbol modifySymbol in view.ModifySymbols)
			{
				PageExtensionMetadataEmitter.WriteModifySymbol(xmlWriter, modifySymbol, WriteProperties);
			}
			foreach (SourceChangeMoveSymbol moveSymbol in view.MoveSymbols)
			{
				PageExtensionMetadataEmitter.WriteMoveSymbol(xmlWriter, moveSymbol);
			}
			xmlWriter.WriteEndElement();
		}
		return stringWriter.ToString();
	}

	internal static void WriteDataCaptionExpression(XmlWriter xmlWriter, BoundCodeExpressionPropertyValue property)
	{
		if (property == null)
		{
			return;
		}
		xmlWriter.WriteStartElement("Expression");
		BoundExpression expression = property.Expression;
		if (PageMetadataEmitHelper.IsDataCaptionExpressionRecordAccess(expression))
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expression;
			ApplicationObjectTypeSymbol containingSymbolOfType = boundFieldAccess.FieldSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
			DebugAssertHelper.Assert(containingSymbolOfType != null);
			DebugAssertHelper.Assert(containingSymbolOfType.IsRecordType());
			string value = boundFieldAccess.FieldSymbol.Id.ToString(CultureInfo.InvariantCulture);
			xmlWriter.WriteAttributeString("FieldNo", value);
			string value2 = containingSymbolOfType.Id.ToString(CultureInfo.InvariantCulture);
			xmlWriter.WriteAttributeString("TableNo", value2);
		}
		else
		{
			if (expression.Type.HasLength)
			{
				xmlWriter.WriteAttributeString("DataLength", expression.Type.Length.ToString(CultureInfo.InvariantCulture));
			}
			xmlWriter.WriteAttributeString("Datatype", expression.Type.GetNavTypeKindSafe().ToString());
		}
		xmlWriter.WriteAttributeString("Name", "DataCaptionExprCode");
		xmlWriter.WriteAttributeString("ExpressionType", "DataCaption");
		xmlWriter.WriteAttributeString("SourceExpression", "DataCaptionExprCode");
		xmlWriter.WriteEndElement();
	}

	protected override bool TryUseSpecialPropertyValueMetadataEmitter(XmlWriter xmlWriter, PropertySymbol property)
	{
		return PageMetadataEmitHelper.TryUseSpecialPropertyValueMetadataEmitter(xmlWriter, property);
	}

	protected void WriteExtensionSpecificMetadataAttributes(XmlWriter xmlWriter)
	{
		xmlWriter.WriteAttributeString("SourceExtensionType", NavExtensionType.ModernDev.ToString());
		if (!string.IsNullOrEmpty(SourceAppId))
		{
			xmlWriter.WriteAttributeString("SourceAppId", SourceAppId);
		}
	}

	internal void WriteExtensionSpecificMetadataAttributesIfNeeded(XmlWriter xmlWriter)
	{
		if (Symbol.Kind == SymbolKind.PageExtension || Symbol.Kind == SymbolKind.PageCustomization)
		{
			WriteExtensionSpecificMetadataAttributes(xmlWriter);
		}
	}

	private static bool IsViewActionProperty(PropertySymbol property)
	{
		PropertyKind propertyKind = property.PropertyKind;
		if ((uint)(propertyKind - 2) <= 1u || propertyKind == PropertyKind.Visible || (uint)(propertyKind - 148) <= 1u)
		{
			return true;
		}
		throw ExceptionUtilities.UnexpectedValue(property.PropertyKind);
	}

	private static void WriteContextSpecificHelpUrlPropertyIfNeeded(XmlWriter xmlWriter, Symbol symbol)
	{
		TypeSymbol typeSymbol = (TypeSymbol)symbol.GetContainingApplicationObjectTypeSymbol();
		if (typeSymbol.Kind != SymbolKind.PageExtension)
		{
			return;
		}
		PropertySymbol property = typeSymbol.GetProperty(PropertyKind.ContextSensitiveHelpPage);
		CompilationOptions options = symbol.DeclaringCompilation.Options;
		string contextSensitiveHelpUrl = PageMetadataEmitHelper.GetContextSensitiveHelpUrl(property, options);
		if (!string.IsNullOrEmpty(contextSensitiveHelpUrl))
		{
			xmlWriter.WriteAttributeString("HelpLink", contextSensitiveHelpUrl);
			if (contextSensitiveHelpUrl.Contains("{0}"))
			{
				PageMetadataEmitHelper.WriteSupportedLocales(xmlWriter, symbol);
			}
		}
	}
}
