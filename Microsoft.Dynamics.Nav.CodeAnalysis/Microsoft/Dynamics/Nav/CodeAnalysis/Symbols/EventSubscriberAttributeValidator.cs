using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class EventSubscriberAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new EventSubscriberAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			CheckAttributeLocation(symbol, diagnostics);
			SourceMethodOrTriggerSymbol containingSymbolOfType = symbol.GetContainingSymbolOfType<SourceMethodOrTriggerSymbol>();
			CheckEventAttribute(symbol, containingSymbolOfType, diagnostics);
		}
	}

	private void CheckAttributeLocation(AttributeSymbol attribute, DiagnosticBag diagnostics)
	{
		CodeunitTypeSymbol containingSymbolOfKind = attribute.GetContainingSymbolOfKind<CodeunitTypeSymbol>(SymbolKind.Codeunit);
		if ((object)containingSymbolOfKind != null && containingSymbolOfKind.Subtype == CodeunitSubtypeKind.Test && containingSymbolOfKind.GetEnumPropertyValue<EventSubscriberInstanceKind>(PropertyKind.EventSubscriberInstance) != EventSubscriberInstanceKind.Manual)
		{
			diagnostics.Add(ErrorCode.ERR_TestCodeunitsEventSubscribersMustHaveManualBinding, attribute.GetLocation());
		}
	}

	private void CheckEventAttribute(AttributeSymbol attribute, SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		AttributeValidatorHelper.CheckEventReturn(methodSymbol, diagnostics);
		SourceAttributeSymbol sourceAttributeSymbol = (SourceAttributeSymbol)attribute;
		ImmutableArray<AttributeArgumentSymbol> arguments = attribute.Arguments;
		AttributeArgumentSymbol attributeArgumentSymbol = arguments[0];
		SourceAttributeArgumentSymbol symbol = (SourceAttributeArgumentSymbol)arguments[1];
		AttributeArgumentSymbol attributeArgumentSymbol2 = arguments[2];
		AttributeArgumentSymbol elementNameArg = arguments[3];
		if (!Enum.TryParse<SymbolKind>(attributeArgumentSymbol.ValueText, ignoreCase: true, out var result))
		{
			return;
		}
		IApplicationObjectTypeSymbol eventApplicationObject = sourceAttributeSymbol.GetEventApplicationObject();
		if (eventApplicationObject == null)
		{
			return;
		}
		string valueText = attributeArgumentSymbol2.ValueText;
		if ((result == SymbolKind.Table && TriggerEventDefinitions.IsValidTableTriggerEvent(valueText)) || (result == SymbolKind.Page && TriggerEventDefinitions.IsValidPageTriggerEvent(valueText)))
		{
			TriggerEventTypeInfo triggerEvent = ((result == SymbolKind.Table) ? TriggerEventDefinitions.GetTableTriggerEventInfo(valueText) : TriggerEventDefinitions.GetPageTriggerEventInfo(valueText));
			InternalCheckTriggerEvent(methodSymbol, triggerEvent, attributeArgumentSymbol2, elementNameArg, methodSymbol.Parameters, eventApplicationObject, diagnostics);
			return;
		}
		CheckElementStringIsEmpty(elementNameArg, valueText, diagnostics);
		MethodSymbol eventMethod = sourceAttributeSymbol.GetEventMethod();
		IApplicationObjectTypeSymbol containingSymbolOfType = eventMethod.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		if (eventMethod != null)
		{
			if (eventMethod.IsEvent)
			{
				AttributeSymbol attributeSymbol = eventMethod.Attributes.FirstOrDefault((AttributeSymbol a) =>
				{
					AttributeTypeInfo attributeInfo = a.AttributeInfo;
					if (attributeInfo == null || attributeInfo.Category != AttributeCategory.Integration)
					{
						AttributeTypeInfo attributeInfo2 = a.AttributeInfo;
						if (attributeInfo2 == null)
						{
							return false;
						}
						return attributeInfo2.Category == AttributeCategory.Business;
					}
					return true;
				});
				AttributeArgumentSymbol attributeArgumentSymbol3 = attributeSymbol.Arguments.ElementAtOrDefault(0);
				bool includeSender = (object)attributeArgumentSymbol3 != null && attributeArgumentSymbol3.ValueAsBoolean == true;
				int num;
				if (attributeSymbol.AttributeInfo.Category == AttributeCategory.Integration)
				{
					AttributeArgumentSymbol attributeArgumentSymbol4 = attributeSymbol.Arguments.ElementAtOrDefault(1);
					num = (((object)attributeArgumentSymbol4 != null && attributeArgumentSymbol4.ValueAsBoolean == true) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
				bool flag = (byte)num != 0;
				CheckEventSubscriberParameters(methodSymbol, methodSymbol.Parameters, eventMethod.Parameters, containingSymbolOfType, includeSender, flag, diagnostics);
				if (flag && !((Symbol)eventApplicationObject).CanCompileWithTarget(methodSymbol.DeclaringCompilation.Options.Target))
				{
					diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, symbol.GetLocation(), eventApplicationObject.Name, methodSymbol.DeclaringCompilation.Options.Target.ToString());
				}
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_MemberNotAnEvent, attributeArgumentSymbol2.Locations.FirstOrDefault(), valueText);
			}
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_EventNotFound, attributeArgumentSymbol2.Locations.FirstOrDefault(), valueText);
		}
	}

	private void CheckEventSubscriberParameters(SourceMethodOrTriggerSymbol methodSymbol, ImmutableArray<ParameterSymbol> subscriberParameters, ImmutableArray<ParameterSymbol> publisherParameters, IApplicationObjectTypeSymbol publisherApplicationObject, bool includeSender, bool globalVariableAccess, DiagnosticBag diagnostics)
	{
		foreach (ParameterSymbol item in subscriberParameters)
		{
			if (includeSender && SemanticFacts.IsSameName(item.Name, "sender"))
			{
				IApplicationObjectTypeSymbol applicationObjectTypeSymbol = publisherApplicationObject;
				if (applicationObjectTypeSymbol.Kind.IsExtensionObject())
				{
					applicationObjectTypeSymbol = ((IApplicationObjectExtensionTypeSymbol)applicationObjectTypeSymbol).Target;
				}
				if (!applicationObjectTypeSymbol.Equals(item.Type.OriginalDefinition))
				{
					string text = string.Concat(applicationObjectTypeSymbol.Kind, " ", applicationObjectTypeSymbol.Id);
					diagnostics.Add(ErrorCode.ERR_EventSubscriberParameterTypeMismatch, item.Locations.FirstOrDefault(), methodSymbol.Name, item.Name, text);
				}
				continue;
			}
			bool flag = false;
			foreach (ParameterSymbol item2 in publisherParameters)
			{
				if (SemanticFacts.IsSameName(item.Name, item2.Name))
				{
					flag = true;
					InternalCheckEventParameter(methodSymbol, item, item2.Type, item2.IsVar, diagnostics);
					break;
				}
			}
			if (!flag)
			{
				if (!globalVariableAccess)
				{
					diagnostics.Add(ErrorCode.ERR_EventUnmatchedParameter, item.Locations.FirstOrDefault(), methodSymbol.Name, item.Name);
					break;
				}
				ImmutableArray<ISymbol> members = publisherApplicationObject.GetMembers(item.Name.UnquoteIdentifier());
				if (members.Length != 1 || members[0].Kind != SymbolKind.GlobalVariable)
				{
					diagnostics.Add(ErrorCode.ERR_EventUnmatchedParameter, item.Locations.FirstOrDefault(), methodSymbol.Name, item.Name);
					break;
				}
				VariableSymbol variableSymbol = (VariableSymbol)members[0];
				if (!AreEventParameterTypesEqual(item.Type, variableSymbol.Type))
				{
					diagnostics.Add(ErrorCode.ERR_EventSubscriberParameterTypeMismatch, item.Locations.FirstOrDefault(), methodSymbol.Name, item.Name, variableSymbol.Type);
					break;
				}
			}
		}
	}

	private void InternalCheckTriggerEvent(SourceMethodOrTriggerSymbol triggerSymbol, TriggerEventTypeInfo triggerEvent, AttributeArgumentSymbol eventNameArg, AttributeArgumentSymbol elementNameArg, ImmutableArray<ParameterSymbol> subscriberParameters, IApplicationObjectTypeSymbol publisherApplicationObject, DiagnosticBag diagnostics)
	{
		if (!InternalCheckTriggerEventElementArgument(triggerSymbol, triggerEvent, eventNameArg, elementNameArg, publisherApplicationObject, diagnostics))
		{
			return;
		}
		foreach (ParameterSymbol item in subscriberParameters)
		{
			bool flag = false;
			foreach (TriggerParameter parameter in triggerEvent.Parameters)
			{
				if (SemanticFacts.IsSameName(item.Name, parameter.Name))
				{
					InternalCheckEventTriggerParameter(triggerSymbol, publisherApplicationObject, item, parameter.Type, parameter.IsVar, diagnostics);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				diagnostics.Add(ErrorCode.ERR_EventUnmatchedParameter, item.Locations.FirstOrDefault(), triggerSymbol.Name, item.Name);
			}
		}
	}

	private void InternalCheckEventTriggerParameter(SourceMethodOrTriggerSymbol triggerSymbol, IApplicationObjectTypeSymbol publisherApplicationObject, ParameterSymbol parameter, TypeSymbol publisherParameterType, bool publisherParameterIsVar, DiagnosticBag diagnostics)
	{
		if (publisherApplicationObject.Kind == SymbolKind.Page && parameter.Type.NavTypeKind == NavTypeKind.Record && !TryGetTable(publisherApplicationObject, out var _))
		{
			AddInvalidEventParameterMissingSourceTableDiagnostic(publisherApplicationObject, parameter.Name, parameter.GetLocation(), diagnostics);
		}
		if (parameter.Type.NavTypeKind == NavTypeKind.Record && TryGetTable(publisherApplicationObject, out var owningTable2) && !parameter.Type.OriginalDefinition.Equals(owningTable2))
		{
			diagnostics.Add(ErrorCode.ERR_EventTriggerEventSubscriberWrongRecord, parameter.GetLocation(), parameter.Type.Name, owningTable2.Name);
		}
		InternalCheckEventParameter(triggerSymbol, parameter, publisherParameterType, publisherParameterIsVar, diagnostics);
	}

	private void InternalCheckEventParameter(SourceMethodOrTriggerSymbol methodSymbol, ParameterSymbol parameter, TypeSymbol publisherParameterType, bool publisherParameterIsVar, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(parameter != null, methodSymbol);
		DebugAssertHelper.Assert(parameter?.Locations != null, methodSymbol);
		DebugAssertHelper.Assert(publisherParameterType != null, methodSymbol);
		if (parameter.IsVar && !publisherParameterIsVar)
		{
			diagnostics.Add(ErrorCode.ERR_EventParameterByVarIsNotAllowed, parameter.Locations.FirstOrDefault(), parameter.Name);
		}
		if (!parameter.Type.NavTypeKind.Equals(publisherParameterType.NavTypeKind))
		{
			diagnostics.Add(ErrorCode.ERR_EventSubscriberParameterTypeMismatch, parameter.Locations.FirstOrDefault(), methodSymbol.Name, parameter.Name, publisherParameterType);
		}
	}

	private bool InternalCheckTriggerEventElementArgument(SourceMethodOrTriggerSymbol methodSymbol, TriggerEventTypeInfo triggerEvent, AttributeArgumentSymbol eventNameArg, AttributeArgumentSymbol elementNameArg, IApplicationObjectTypeSymbol publisherApplicationObject, DiagnosticBag diagnostics)
	{
		string valueText = elementNameArg.ValueText;
		switch (triggerEvent.ElementType)
		{
		case SymbolKind.Field:
			if (((TableTypeSymbol)publisherApplicationObject).QueryAllFields(valueText, methodSymbol.DeclaringCompilation).Count((FieldSymbol x) => x.Kind == SymbolKind.Field) != 1)
			{
				diagnostics.Add(ErrorCode.ERR_FieldNotFound, elementNameArg.Locations.FirstOrDefault(), valueText);
				return false;
			}
			break;
		case SymbolKind.Action:
			if (((PageTypeSymbol)publisherApplicationObject).QueryAllActions(valueText, methodSymbol.DeclaringCompilation).Count((ActionSymbol x) => x.ActionKind == ActionKind.Action) != 1)
			{
				diagnostics.Add(ErrorCode.ERR_ActionNotFound, elementNameArg.Locations.FirstOrDefault(), valueText);
				return false;
			}
			break;
		case SymbolKind.Control:
		{
			PageTypeSymbol page = (PageTypeSymbol)publisherApplicationObject;
			if (TryGetTable(publisherApplicationObject, out var _))
			{
				if (page.QueryAllControls(valueText, methodSymbol.DeclaringCompilation).Count((ControlSymbol c) => c.ControlKind == ControlKind.Field) != 1)
				{
					diagnostics.Add(ErrorCode.ERR_ControlNotFound, elementNameArg.Locations.FirstOrDefault(), valueText);
					return false;
				}
				break;
			}
			AddInvalideEventMissingSourceTableDiagnostic(publisherApplicationObject, eventNameArg.ValueText, eventNameArg.GetLocation(), diagnostics);
			return false;
		}
		case SymbolKind.Undefined:
			return CheckElementStringIsEmpty(elementNameArg, triggerEvent.Name, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(triggerEvent.ElementType);
		}
		return true;
	}

	private static bool TryGetTable(IApplicationObjectTypeSymbol publisherApplicationObject, out ApplicationObjectTypeSymbol owningTable)
	{
		owningTable = publisherApplicationObject.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		if (publisherApplicationObject.Kind == SymbolKind.Page)
		{
			owningTable = ((PageTypeSymbol)publisherApplicationObject).RelatedTable;
		}
		return owningTable != null;
	}

	private static void AddInvalideEventMissingSourceTableDiagnostic(IApplicationObjectTypeSymbol publisherApplicationObject, string triggerEventName, Location location, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = publisherApplicationObject.GetProperty(PropertyKind.SourceTable) as PropertySymbol;
		if (propertySymbol != null)
		{
			diagnostics.Add(ErrorCode.ERR_EventTriggerEventPageMissingSourceTableButFilled, location, triggerEventName, propertySymbol.ValueText);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_EventTriggerEventPageMissingSourceTable, location, triggerEventName);
		}
	}

	private static void AddInvalidEventParameterMissingSourceTableDiagnostic(IApplicationObjectTypeSymbol publisherApplicationObject, string parameterName, Location location, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = publisherApplicationObject.GetProperty(PropertyKind.SourceTable) as PropertySymbol;
		if (propertySymbol != null)
		{
			diagnostics.Add(ErrorCode.ERR_EventTriggerPageParameterMissingSourceTableButFilled, location, parameterName, propertySymbol.ValueText);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_EventTriggerPageParameterMissingSourceTable, location, parameterName);
		}
	}

	private static bool CheckElementStringIsEmpty(AttributeArgumentSymbol elementNameArg, string triggerName, DiagnosticBag diagnostics)
	{
		if (!string.IsNullOrEmpty(elementNameArg.ValueText))
		{
			diagnostics.Add(ErrorCode.ERR_EventElementNameShouldBeEmpty, elementNameArg.Locations.FirstOrDefault(), triggerName);
			return false;
		}
		return true;
	}

	private static bool AreEventParameterTypesEqual(TypeSymbol x, TypeSymbol y)
	{
		if (x == null || y == null)
		{
			return false;
		}
		if (x.Kind == y.Kind && x.Kind == SymbolKind.Record)
		{
			RecordTypeSymbol recordTypeSymbol = (RecordTypeSymbol)x;
			return SemanticFacts.IsSameName(nameB: ((RecordTypeSymbol)y).Name, nameA: recordTypeSymbol.Name);
		}
		return x.Equals(y);
	}
}
