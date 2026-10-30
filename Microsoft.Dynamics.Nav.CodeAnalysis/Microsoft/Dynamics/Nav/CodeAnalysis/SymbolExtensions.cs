using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public static class SymbolExtensions
{
	public static Location GetLocation(this ISymbol symbol)
	{
		ImmutableArray<Location> locations = symbol.Locations;
		DebugAssertHelper.Assert(locations.Length > 0, symbol.Kind.ToString());
		if (locations.Length == 0)
		{
			ISymbol containingSymbol = symbol.ContainingSymbol;
			if (containingSymbol == null || containingSymbol.Locations.Length <= 0)
			{
				return Location.None;
			}
			return symbol.ContainingSymbol?.Locations[0];
		}
		return locations[0];
	}

	public static IApplicationObjectTypeSymbol GetContainingApplicationObjectTypeSymbol(this ISymbol symbol)
	{
		IApplicationObjectTypeSymbol applicationObjectTypeSymbol = symbol as IApplicationObjectTypeSymbol;
		if (applicationObjectTypeSymbol == symbol)
		{
			return applicationObjectTypeSymbol;
		}
		return symbol.ContainingSymbol.GetContainingApplicationObjectTypeSymbol();
	}

	public static T? GetSimplePropertyValue<T>(this ISymbol iSymbol, PropertyKind kind) where T : struct
	{
		return (iSymbol as Symbol)?.GetSimplePropertyValue<T>(kind);
	}

	public static T? GetEnumPropertyValue<T>(this ISymbol iSymbol, PropertyKind kind) where T : struct
	{
		return (iSymbol as Symbol)?.GetEnumPropertyValue<T>(kind);
	}

	public static T GetPropertyValueSyntax<T>(this IPropertySymbol symbol) where T : PropertyValueSyntax
	{
		if (!(((PropertySymbol)symbol).GetFirstSyntax() is PropertySyntax { Value: var value } propertySyntax))
		{
			return null;
		}
		if (propertySyntax.Value is T)
		{
			return (T)value;
		}
		return null;
	}

	public static string GetApplicationObjectSubtype(this IApplicationObjectTypeSymbol symbol)
	{
		if (symbol == null)
		{
			return "Normal";
		}
		switch (symbol.NavTypeKind)
		{
		case NavTypeKind.Table:
			return ((TableTypeSymbol)symbol).TableType.ToString();
		case NavTypeKind.Codeunit:
			return ((ICodeunitTypeSymbol)symbol).Subtype.ToString();
		case NavTypeKind.Page:
			return ((PageTypeSymbol)symbol).PageType.ToString();
		case NavTypeKind.Query:
			return ((QueryTypeSymbol)symbol).QueryType.ToString();
		case NavTypeKind.PageExtension:
		case NavTypeKind.TableExtension:
		case NavTypeKind.PageCustomization:
		case NavTypeKind.EnumExtension:
			return ((IApplicationObjectExtensionTypeSymbol)symbol).Target.Id.ToString(CultureInfo.InvariantCulture);
		default:
			return "Normal";
		}
	}

	internal static ModuleKind GetModuleKind(this IModuleSymbol symbol)
	{
		if (!string.Equals(symbol.PublisherName, "Microsoft", StringComparison.Ordinal))
		{
			return ModuleKind.Extension;
		}
		if (string.Equals(symbol.Name, "Application", StringComparison.Ordinal))
		{
			return ModuleKind.Application;
		}
		if (string.Equals(symbol.Name, "System", StringComparison.Ordinal))
		{
			return ModuleKind.Platform;
		}
		if (string.Equals(symbol.Name, "Test", StringComparison.Ordinal))
		{
			return ModuleKind.Test;
		}
		return ModuleKind.Extension;
	}

	public static T GetContainingSymbolOfKind<T>(this ISymbol symbol, SymbolKind kind, SymbolKind alternateKind = SymbolKind.Undefined) where T : ISymbol
	{
		while (symbol != null)
		{
			if (symbol.Kind == kind)
			{
				return (T)symbol;
			}
			if (alternateKind != SymbolKind.Undefined && symbol.Kind == alternateKind)
			{
				return (T)symbol;
			}
			symbol = symbol.ContainingSymbol;
		}
		return default;
	}

	internal static TypeSymbol GetBaseApplicationObjectSymbol(this Symbol symbol)
	{
		ISymbol symbol2 = symbol;
		if (symbol2 is IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbol)
		{
			symbol2 = applicationObjectExtensionTypeSymbol.Target;
		}
		return symbol2 as TypeSymbol;
	}

	internal static bool IsTrigger(this Symbol symbol)
	{
		if (symbol.Kind != SymbolKind.Method)
		{
			return false;
		}
		return ((MethodSymbol)symbol).MethodKind == MethodKind.Trigger;
	}

	internal static bool IsDotNetEventTrigger(this Symbol symbol)
	{
		if (symbol.Kind != SymbolKind.Method)
		{
			return false;
		}
		return ((MethodSymbol)symbol).MethodKind == MethodKind.EventTrigger;
	}

	internal static TypeSymbol GetAliasedTypeSymbol(this TypeSymbol type)
	{
		switch (type.Kind)
		{
		case SymbolKind.Record:
		case SymbolKind.PageReference:
			return type.OriginalDefinition;
		case SymbolKind.TestPage:
			return ((TestPageTypeSymbol)type).Target;
		case SymbolKind.TestRequestPage:
			return ((TestRequestPageTypeSymbol)type).Target;
		default:
			return type;
		}
	}

	internal static bool CanBeOverloaded(this MethodSymbol method)
	{
		if (method.MethodKind == MethodKind.Method && !method.IsEvent)
		{
			return !method.IsEventSubscriber();
		}
		return false;
	}

	internal static string GetOverloadedMethodMetadataNameSuffix(this MethodSymbol method)
	{
		return "_" + Math.Abs(method.Id).ToString(CultureInfo.InvariantCulture);
	}

	internal static bool IsCodeunitOnRunTrigger(this MethodSymbol method)
	{
		if (method.IsTrigger() && method.ContainingType.NavTypeKind == NavTypeKind.Codeunit)
		{
			return SemanticFacts.IsSameName(method.Name, "OnRun");
		}
		return false;
	}

	internal static TypeSymbol GetAttributeTargetVariableType(this AttributeSymbol symbol)
	{
		TypeSymbol typeSymbol = ((VariableSymbol)symbol.ContainingSymbol).Type;
		if (typeSymbol.IsArray())
		{
			typeSymbol = ((ArrayTypeSymbol)typeSymbol).ElementType;
		}
		return typeSymbol;
	}

	internal static bool HasParameters(this MethodSymbol method)
	{
		return method.ParameterCount > 0;
	}

	internal static bool HasReturnValue(this MethodSymbol method)
	{
		if (method.ReturnValue != null)
		{
			return !method.ReturnType.IsNoneType();
		}
		return false;
	}

	internal static bool IsEventSubscriber(this MethodSymbol method)
	{
		if (method.Attributes != null)
		{
			return method.Attributes.Any((AttributeSymbol x) =>
			{
				AttributeTypeInfo attributeInfo = x.AttributeInfo;
				return attributeInfo != null && attributeInfo.Category == AttributeCategory.EventSubscriber;
			});
		}
		return false;
	}

	internal static bool IsInvalidModifySymbol(this ChangeModifySymbol modifySymbol)
	{
		return modifySymbol.Target.IsKind(SymbolKind.ErrorType);
	}

	internal static T GetContainingSymbolOfType<T>(this ISymbol symbol) where T : ISymbol
	{
		while (symbol != null)
		{
			ISymbol symbol2;
			if ((symbol2 = symbol) is T)
			{
				return (T)symbol2;
			}
			symbol = symbol.ContainingSymbol;
		}
		return default;
	}

	internal static bool HasRelatedTable(this ISymbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.ReportDataItem => true, 
			SymbolKind.XmlPortNode => ((XmlPortNodeSymbol)symbol).SourceTypeKind != XmlPortSourceTypeKind.Text, 
			_ => false, 
		};
	}

	internal static bool IsEmittedAsAttribute(this PropertySymbol property)
	{
		BoundProperty property2 = property.Property;
		if (property2 == null)
		{
			return true;
		}
		return property2.PropertyValue?.GetPropertyTypeInfo()?.EmitAsAttribute != false;
	}

	internal static bool ShouldBeEmitted(this PropertySymbol property)
	{
		return property.GetPropertyTypeInfo().CanGenerateMetadata;
	}

	internal static bool IsSourceSymbol(this ISymbol symbol)
	{
		DebugAssertHelper.Assert(symbol != null);
		if (symbol == null)
		{
			return false;
		}
		return !symbol.DeclaringSyntaxReferences.IsDefaultOrEmpty;
	}

	internal static string ToReportingString(this ISymbol symbol)
	{
		SyntaxNode syntax = symbol.GetSyntax();
		string text = "Symbol Kinds: " + GetSymbolChainStringified(symbol);
		if (syntax == null)
		{
			return text;
		}
		return text + ". Syntax Kinds: " + syntax.ToReportingString();
	}

	internal static SynthesizedGlobalVariableSymbol GetCurrentObjectSymbol(this Symbol symbol, ObjectTypeSymbol containingSymbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.Report => SynthesizedGlobalVariableSymbol.Create(containingSymbol, ReportReferenceTypeSymbol.Create((ReportTypeSymbol)symbol, isCurrReport: true), "CurrReport"), 
			SymbolKind.XmlPort => SynthesizedGlobalVariableSymbol.Create(containingSymbol, XmlPortReferenceTypeSymbol.Create((XmlPortTypeSymbol)symbol, isCurrXmlPort: true), "currXMLport"), 
			SymbolKind.Page => SynthesizedGlobalVariableSymbol.Create(containingSymbol, PageReferenceTypeSymbol.Create((PageTypeSymbol)symbol, isCurrPage: true), "CurrPage"), 
			_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
		};
	}

	internal static bool IsNonInvocableMember(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return IsControlAddinEvent(symbol);
		}
		return true;
	}

	internal static bool IsControlAddinEvent(Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Event)
		{
			return true;
		}
		if (symbol.Kind != SymbolKind.Method)
		{
			return false;
		}
		if (symbol.ContainingSymbol == null || symbol.ContainingSymbol.Kind != SymbolKind.Control)
		{
			return false;
		}
		ControlAddInTypeSymbol relatedControlAddInSymbol = ((ControlSymbol)symbol.ContainingSymbol).RelatedControlAddInSymbol;
		if (relatedControlAddInSymbol == null)
		{
			return false;
		}
		return relatedControlAddInSymbol.GetMembers(symbol.Name).Count((Symbol x) => x.Kind == SymbolKind.Event) == 1;
	}

	internal static bool IsContainedBy(this ISymbol symbol, ISymbol containerSymbol)
	{
		for (ISymbol containingSymbol = symbol.ContainingSymbol; containingSymbol != null; containingSymbol = containingSymbol.ContainingSymbol)
		{
			if (containingSymbol == containerSymbol)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsControl(this ControlSymbol c)
	{
		return c.ControlKind != ControlKind.Area;
	}

	internal static bool IsActionWithId(this ActionSymbol a)
	{
		if (a.ActionKind != ActionKind.Action)
		{
			return a.ActionKind == ActionKind.Group;
		}
		return true;
	}

	internal static bool IsInstance(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return !((MethodSymbol)symbol).IsStatic;
		case SymbolKind.Class:
		case SymbolKind.OptionType:
			return false;
		default:
			return true;
		}
	}

	internal static bool HasAttribute(this MethodSymbol symbol, AttributeTagKind attributeTag)
	{
		return symbol?.Attributes.Any((AttributeSymbol x) => x.AttributeInfo?.ContainsTag(attributeTag) ?? false) ?? false;
	}

	internal static bool HasAttribute(this Symbol symbol, AttributeKind attributeKind)
	{
		return (symbol as ISymbolWithAttributes)?.Attributes.Any((AttributeSymbol x) =>
		{
			AttributeTypeInfo attributeInfo = x.AttributeInfo;
			return attributeInfo != null && attributeInfo.Kind == attributeKind;
		}) ?? false;
	}

	internal static bool HasAttribute(this MethodSymbol symbol, AttributeKind attributeKind)
	{
		return symbol?.Attributes.Any((AttributeSymbol x) =>
		{
			AttributeTypeInfo attributeInfo = x.AttributeInfo;
			return attributeInfo != null && attributeInfo.Kind == attributeKind;
		}) ?? false;
	}

	internal static bool HasRunOnClientAttribute(this VariableSymbol symbol)
	{
		return symbol.Attributes.Any((AttributeSymbol x) =>
		{
			AttributeTypeInfo attributeInfo = x.AttributeInfo;
			return attributeInfo != null && attributeInfo.Kind == AttributeKind.RunOnClient;
		});
	}

	internal static bool HasSuppressDisposeAttribute(this VariableSymbol symbol)
	{
		return symbol.Attributes.Any((AttributeSymbol x) =>
		{
			AttributeTypeInfo attributeInfo = x.AttributeInfo;
			return attributeInfo != null && attributeInfo.Kind == AttributeKind.SuppressDispose;
		});
	}

	internal static bool HasWithEventsAttribute(this VariableSymbol symbol)
	{
		return symbol.Attributes.Any((AttributeSymbol x) =>
		{
			AttributeTypeInfo attributeInfo = x.AttributeInfo;
			return attributeInfo != null && attributeInfo.Kind == AttributeKind.WithEvents;
		});
	}

	internal static bool IsOptionMembers(this Symbol symbol)
	{
		if (symbol.Kind != SymbolKind.Option)
		{
			return symbol.Kind == SymbolKind.EnumValue;
		}
		return true;
	}

	internal static bool MustBeAccessedAsOptionMember(this Symbol symbol)
	{
		if (symbol.IsOptionMembers())
		{
			return symbol.ContainingSymbol.Kind != SymbolKind.DotNet;
		}
		return false;
	}

	internal static bool IsPageElement(this Symbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if ((uint)(kind - 60) <= 1u || kind == SymbolKind.View)
		{
			return true;
		}
		return false;
	}

	internal static T GetBoundPropertyValue<T>(this ContainerSymbol symbol, PropertyKind kind) where T : BoundPropertyValue
	{
		return symbol.GetProperty(kind)?.Property?.PropertyValue as T;
	}

	internal static IPageBaseTypeSymbol GetContainingPage(this ISymbol control)
	{
		for (ISymbol symbol = control; symbol != null; symbol = symbol.ContainingSymbol)
		{
			if (symbol is IPageBaseTypeSymbol result)
			{
				return result;
			}
		}
		return control.GetExtendedPage();
	}

	internal static PageTypeSymbol GetExtendedPage(this ISymbol control)
	{
		for (ISymbol symbol = control; symbol != null; symbol = symbol.ContainingSymbol)
		{
			PageExtensionTypeSymbol pageExtensionTypeSymbol = symbol as PageExtensionTypeSymbol;
			if (pageExtensionTypeSymbol != null)
			{
				return pageExtensionTypeSymbol.Target as PageTypeSymbol;
			}
		}
		return null;
	}

	internal static ControlSymbol GetExtendedPageControlOrSelf(this ISymbol symbol)
	{
		ISymbol symbol2 = symbol;
		if (symbol.Kind == SymbolKind.Change)
		{
			ChangeSymbol changeSymbol = (ChangeSymbol)symbol;
			if (changeSymbol.ChangeKind == ChangeKind.Modify && changeSymbol.ChangeTargetKind == SymbolKind.Control)
			{
				symbol2 = ((ChangeModifySymbol)changeSymbol).Target;
			}
		}
		return symbol2 as ControlSymbol;
	}

	internal static ContainerSymbol GetContainingSymbol(this ISymbol symbol)
	{
		ISymbol symbol2 = symbol.ContainingSymbol;
		if (symbol2.Kind == SymbolKind.Change)
		{
			ChangeSymbol changeSymbol = (ChangeSymbol)symbol2;
			if (changeSymbol.ChangeKind.IsModify())
			{
				symbol2 = ((ChangeModifySymbol)changeSymbol).Target?.GetContainingSymbol();
			}
			else if (changeSymbol.ChangeKind.IsAdd())
			{
				Symbol anchor = ((ChangeAddSymbol)changeSymbol).Anchor;
				symbol2 = ((!changeSymbol.ChangeKind.IsAddToContainer()) ? anchor?.GetContainingSymbol() : anchor);
			}
		}
		return symbol2 as ContainerSymbol;
	}

	internal static IEnumerable<Symbol> GetPageElementSymbols(this Symbol symbol, IdentifierNameSyntax nameSyntax)
	{
		return symbol.DeclaringCompilation.GetBinderFactory(nameSyntax.SyntaxTree).GetBinder(nameSyntax).FindSymbols(nameSyntax.Unquoted(), LookupOptions.MustBePageElement);
	}

	internal static IEnumerable<Symbol> GetTableFields(this Symbol symbol, TableTypeSymbol baseTable, IdentifierNameSyntax nameSyntax)
	{
		return from s in symbol.DeclaringCompilation.GetBinderFactory(nameSyntax.SyntaxTree).GetBinder(nameSyntax).FindSymbols(baseTable, nameSyntax.Unquoted(), LookupOptions.Default)
			where s.Kind == SymbolKind.Field
			select s;
	}

	internal static IEnumerable<Symbol> GetTableFieldgroups(this Symbol symbol, TableTypeSymbol baseTable, IdentifierNameSyntax nameSyntax)
	{
		return from s in symbol.DeclaringCompilation.GetBinderFactory(nameSyntax.SyntaxTree).GetBinder(nameSyntax).FindSymbols(baseTable, nameSyntax.Unquoted(), LookupOptions.Default)
			where s.Kind == SymbolKind.FieldGroup
			select s;
	}

	internal static IEnumerable<Symbol> GetPageElementSymbols(this Symbol symbol, PageTypeSymbol basePage, IdentifierNameSyntax nameSyntax)
	{
		return symbol.DeclaringCompilation.GetBinderFactory(nameSyntax.SyntaxTree).GetBinder(nameSyntax).FindSymbols(basePage, nameSyntax.Unquoted(), LookupOptions.MustBePageElement);
	}

	internal static DefaultProperty GetDefaultProperty(this Symbol symbol, PropertyKind kind)
	{
		PropertyTypeInfo propertyTypeInfo = symbol.GetPropertyTypeInfo(kind.ToString());
		if (propertyTypeInfo == null || propertyTypeInfo.DefaultValue == null)
		{
			return null;
		}
		return new DefaultProperty(propertyTypeInfo);
	}

	internal static int? GetInt32PropertyValue(this Symbol symbol, PropertyKind kind)
	{
		return symbol.GetSimplePropertyValue<int>(kind) ?? symbol.GetDefaultPropertyValue(kind, (DefaultProperty v) => int.TryParse(v.DefaultValue.ToUpperInvariant(), out var result) ? new int?(result) : ((int?)null));
	}

	internal static bool? GetBooleanPropertyValue(this Symbol symbol, PropertyKind kind)
	{
		return symbol.GetSimplePropertyValue<bool>(kind) ?? symbol.GetDefaultPropertyValue(kind, (DefaultProperty defaultValue) =>
		{
			string defaultValue2 = defaultValue.DefaultValue;
			if (defaultValue.IsBoolean)
			{
				if (defaultValue2 == "1")
				{
					return true;
				}
				if (defaultValue2 == "0")
				{
					return false;
				}
			}
			bool result;
			return bool.TryParse(defaultValue2, out result) ? new bool?(result) : ((bool?)null);
		});
	}

	internal static T? GetEnumPropertyValue<T>(this Symbol symbol, PropertyKind kind) where T : struct
	{
		T? simplePropertyValue = symbol.GetSimplePropertyValue<T>(kind);
		if (simplePropertyValue.HasValue)
		{
			return simplePropertyValue.Value;
		}
		return symbol.GetDefaultPropertyValue(kind, (DefaultProperty v) => Enum.TryParse<T>(v.DefaultValue, ignoreCase: true, out var result) ? new T?(result) : ((T?)null));
	}

	internal static T GetEnumPropertyValueOrDefault<T>(this Symbol symbol, PropertyKind kind) where T : struct
	{
		return symbol.GetEnumPropertyValue<T>(kind) ?? default(T);
	}

	internal static T? GetValueAsEnum<T>(this AttributeArgumentSymbol attributeArgument) where T : struct
	{
		T result;
		return Enum.TryParse<T>(attributeArgument.ValueText, ignoreCase: true, out result) ? result : default(T);
	}

	internal static T? GetValueAsEnum<T>(this PropertySymbol property) where T : struct
	{
		if ((object)property == null || property.Value == null)
		{
			return null;
		}
		if (property.Property == null)
		{
			return ParseSimplePropertyValueFromText<T>(property.ValueText);
		}
		return GetSimplePropertyValueFromBoundValue<T>(property);
	}

	internal static bool CheckEnumPropertyValue<T>(this Symbol symbol, PropertyKind kind, T enumValue) where T : struct
	{
		return symbol.GetEnumPropertyValueOrDefault<T>(kind).Equals(enumValue);
	}

	internal static T? GetSimplePropertyValue<T>(this Symbol symbol, PropertyKind kind) where T : struct
	{
		PropertySymbol propertySymbol = symbol.GetPropertySymbol(kind);
		if ((object)propertySymbol == null || propertySymbol.Value == null)
		{
			return null;
		}
		if (propertySymbol.Property == null)
		{
			return ParseSimplePropertyValueFromText<T>(propertySymbol.ValueText);
		}
		return GetSimplePropertyValueFromBoundValue<T>(propertySymbol);
	}

	internal static PropertySymbol GetPropertySymbol(this Symbol symbol, PropertyKind kind)
	{
		DebugAssertHelper.Assert(symbol != null);
		ContainerSymbol containerSymbol = symbol as ContainerSymbol;
		if (containerSymbol != null)
		{
			return containerSymbol.GetProperty(kind);
		}
		if (symbol.Kind == SymbolKind.Key || symbol.Kind == SymbolKind.FieldGroup)
		{
			IEnumerable<PropertySymbol> source = ((FieldNameListSymbol)symbol).Properties.Where((PropertySymbol f) => f.PropertyKind == kind);
			if (source.Count() == 1)
			{
				return source.First();
			}
		}
		return null;
	}

	internal static T? GetDefaultPropertyValue<T>(this Symbol symbol, PropertyKind kind, Func<DefaultProperty, T?> create) where T : struct
	{
		DefaultProperty defaultProperty = symbol.GetDefaultProperty(kind);
		if (defaultProperty == null)
		{
			return null;
		}
		return create(defaultProperty);
	}

	internal static TableTypeSymbol GetRelatedTableSymbol(this ISymbol symbol)
	{
		if (symbol == null)
		{
			return null;
		}
		switch (symbol.Kind)
		{
		case SymbolKind.Table:
			return (TableTypeSymbol)symbol;
		case SymbolKind.TableExtension:
			return ((TableExtensionTypeSymbol)symbol).Target as TableTypeSymbol;
		case SymbolKind.Page:
		case SymbolKind.RequestPage:
			return (TableTypeSymbol)((IPageBaseTypeSymbol)symbol).RelatedTable;
		case SymbolKind.PageExtension:
			return (((PageExtensionTypeSymbol)symbol).Target as PageTypeSymbol)?.RelatedTable;
		case SymbolKind.PageCustomization:
			return (((PageCustomizationTypeSymbol)symbol).Target as PageTypeSymbol)?.RelatedTable;
		case SymbolKind.Codeunit:
			return ((CodeunitTypeSymbol)symbol).RelatedTable;
		case SymbolKind.ReportDataItem:
			return ((ReportDataItemSymbol)symbol).RelatedTable;
		case SymbolKind.QueryDataItem:
			return ((QueryDataItemSymbol)symbol).RelatedTable;
		case SymbolKind.XmlPortNode:
		{
			TypeSymbol baseType = ((XmlPortNodeSymbol)symbol).Type.BaseType;
			if ((object)baseType == null || baseType.Kind != SymbolKind.Table)
			{
				return null;
			}
			return (TableTypeSymbol)baseType;
		}
		default:
			return null;
		}
	}

	internal static IEnumerable<IApplicationObjectExtensionTypeSymbol> GetExtensionsAcrossModules(this IApplicationObjectTypeSymbol applicationObject, Compilation compilation)
	{
		return compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(applicationObject);
	}

	internal static IEnumerable<IApplicationObjectExtensionTypeSymbol> GetExtensionsAcrossModules(this IApplicationObjectTypeSymbol applicationObject, ModuleSymbol module)
	{
		return module.GetApplicationObjectExtensionTypeSymbolsAcrossModules(applicationObject);
	}

	internal static IEnumerable<ISymbol> QueryAllMembers(this IApplicationObjectTypeSymbol applicationObject, Compilation compilation)
	{
		foreach (ISymbol member in applicationObject.GetMembers())
		{
			yield return member;
		}
		foreach (IApplicationObjectExtensionTypeSymbol extensionsAcrossModule in applicationObject.GetExtensionsAcrossModules(compilation))
		{
			foreach (ISymbol member2 in extensionsAcrossModule.GetMembers())
			{
				yield return member2;
			}
		}
	}

	internal static IEnumerable<ISymbol> QueryAllMembers(this IApplicationObjectTypeSymbol applicationObject, ModuleSymbol module)
	{
		foreach (ISymbol member in applicationObject.GetMembers())
		{
			yield return member;
		}
		foreach (IApplicationObjectExtensionTypeSymbol extensionsAcrossModule in applicationObject.GetExtensionsAcrossModules(module))
		{
			foreach (ISymbol member2 in extensionsAcrossModule.GetMembers())
			{
				yield return member2;
			}
		}
	}

	internal static IEnumerable<ISymbol> QueryAllMembers(this IApplicationObjectTypeSymbol applicationObject, string memberName, Compilation compilation)
	{
		foreach (ISymbol member in applicationObject.GetMembers(memberName))
		{
			yield return member;
		}
		foreach (IApplicationObjectExtensionTypeSymbol extensionsAcrossModule in applicationObject.GetExtensionsAcrossModules(compilation))
		{
			foreach (ISymbol member2 in extensionsAcrossModule.GetMembers(memberName))
			{
				yield return member2;
			}
		}
	}

	internal static IEnumerable<ISymbol> QueryAllEvents(this IApplicationObjectTypeSymbol applicationObject, Compilation compilation)
	{
		return from x in applicationObject.QueryAllMembers(compilation)
			where x.IsKind(SymbolKind.Method) && ((MethodSymbol)x).IsEvent
			select x;
	}

	internal static IEnumerable<ISymbol> QueryAllEvents(this IApplicationObjectTypeSymbol applicationObject, string memberName, Compilation compilation)
	{
		return from x in applicationObject.QueryAllMembers(memberName, compilation)
			where x.IsKind(SymbolKind.Method) && ((MethodSymbol)x).IsEvent
			select x;
	}

	internal static IEnumerable<FieldSymbol> QueryAllFields(this TableTypeSymbol table, Compilation compilation)
	{
		foreach (FieldSymbol field in table.GetFields())
		{
			yield return field;
		}
		foreach (TableExtensionTypeSymbol item in table.GetExtensionsAcrossModules(compilation).Cast<TableExtensionTypeSymbol>())
		{
			foreach (FieldSymbol addedField in item.GetAddedFields())
			{
				yield return addedField;
			}
		}
	}

	internal static IEnumerable<FieldSymbol> QueryAllFields(this TableTypeSymbol table, string name, Compilation compilation)
	{
		foreach (FieldSymbol field in table.GetFields())
		{
			if (SemanticFacts.IsSameName(field.Name, name))
			{
				yield return field;
			}
		}
		foreach (TableExtensionTypeSymbol item in table.GetExtensionsAcrossModules(compilation).Cast<TableExtensionTypeSymbol>())
		{
			foreach (FieldSymbol addedField in item.GetAddedFields())
			{
				if (SemanticFacts.IsSameName(addedField.Name, name))
				{
					yield return addedField;
				}
			}
		}
	}

	internal static IEnumerable<ActionSymbol> QueryAllActions(this PageTypeSymbol page, Compilation compilation)
	{
		foreach (ActionSymbol flattenedAction in page.FlattenedActions)
		{
			yield return flattenedAction;
		}
		foreach (PageExtensionTypeSymbol item in page.GetExtensionsAcrossModules(compilation).Cast<PageExtensionTypeSymbol>())
		{
			foreach (ActionSymbol flattenedAction2 in item.FlattenedActions)
			{
				yield return flattenedAction2;
			}
		}
	}

	internal static IEnumerable<ActionSymbol> QueryAllActions(this PageTypeSymbol page, string name, Compilation compilation)
	{
		foreach (ActionSymbol flattenedAction in page.FlattenedActions)
		{
			if (SemanticFacts.IsSameName(flattenedAction.Name, name))
			{
				yield return flattenedAction;
			}
		}
		foreach (PageExtensionTypeSymbol item in page.GetExtensionsAcrossModules(compilation).Cast<PageExtensionTypeSymbol>())
		{
			foreach (ActionSymbol flattenedAction2 in item.FlattenedActions)
			{
				if (SemanticFacts.IsSameName(flattenedAction2.Name, name))
				{
					yield return flattenedAction2;
				}
			}
		}
	}

	internal static IEnumerable<ControlSymbol> QueryAllControls(this PageTypeSymbol page, Compilation compilation)
	{
		foreach (ControlSymbol flattenedControl in page.FlattenedControls)
		{
			yield return flattenedControl;
		}
		foreach (PageExtensionTypeSymbol item in page.GetExtensionsAcrossModules(compilation).Cast<PageExtensionTypeSymbol>())
		{
			foreach (ControlSymbol flattenedControl2 in item.FlattenedControls)
			{
				yield return flattenedControl2;
			}
		}
	}

	internal static IEnumerable<ControlSymbol> QueryAllControls(this PageTypeSymbol page, string name, Compilation compilation)
	{
		foreach (ControlSymbol flattenedControl in page.FlattenedControls)
		{
			if (SemanticFacts.IsSameName(flattenedControl.Name, name))
			{
				yield return flattenedControl;
			}
		}
		foreach (PageExtensionTypeSymbol item in page.GetExtensionsAcrossModules(compilation).Cast<PageExtensionTypeSymbol>())
		{
			foreach (ControlSymbol flattenedControl2 in item.FlattenedControls)
			{
				if (SemanticFacts.IsSameName(flattenedControl2.Name, name))
				{
					yield return flattenedControl2;
				}
			}
		}
	}

	internal static ActionSymbol GetArea(this ActionSymbol action)
	{
		while (action != null && action.ActionKind != ActionKind.Area)
		{
			action = ((action.ContainingSymbol.Kind != SymbolKind.Change) ? (action.ContainingSymbol as ActionSymbol) : ((action.ContainingSymbol as ChangeAddSymbol)?.Anchor as ActionSymbol));
		}
		return action;
	}

	internal static AreaKind GetArea(this ControlSymbol control)
	{
		ControlSymbol controlSymbol = control;
		while (controlSymbol != null && controlSymbol.ControlKind != ControlKind.Area)
		{
			controlSymbol = ((controlSymbol.ContainingSymbol.Kind != SymbolKind.Change) ? (controlSymbol.ContainingSymbol as ControlSymbol) : ((controlSymbol.ContainingSymbol as ChangeAddSymbol)?.Anchor as ControlSymbol));
		}
		if (controlSymbol == null)
		{
			DebugAssertHelper.Assert(controlSymbol != null, "Could not find the area of control " + control.Name);
			return AreaKind.None;
		}
		return SyntaxFacts.GetAreaKind(controlSymbol.Name);
	}

	internal static PageTypeKind GetTypeOfNestedRequestPage(this ApplicationObjectTypeSymbol applicationObject)
	{
		switch (applicationObject.Kind)
		{
		case SymbolKind.Report:
			if (((ReportTypeSymbol)applicationObject).GetBooleanPropertyValue(PropertyKind.ProcessingOnly) == true)
			{
				return PageTypeKind.ReportProcessingOnly;
			}
			return PageTypeKind.ReportPreview;
		case SymbolKind.XmlPort:
			return PageTypeKind.XmlPort;
		default:
			throw ExceptionUtilities.UnexpectedValue(applicationObject.Kind);
		}
	}

	internal static ApplicationObjectExtensionTypeSymbol GetParentExtensionTypeSymbol(this Symbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if (kind == SymbolKind.Method || kind == SymbolKind.Field || (uint)(kind - 60) <= 1u)
		{
			return symbol.GetContainingSymbolOfType<ApplicationObjectExtensionTypeSymbol>();
		}
		return null;
	}

	internal static Location GetLocation(this Symbol symbol)
	{
		ImmutableArray<Location> locations = symbol.Locations;
		DebugAssertHelper.Assert(locations.Length > 0, symbol.Kind.ToString());
		if (locations.Length == 0)
		{
			Symbol containingSymbol = symbol.ContainingSymbol;
			if ((object)containingSymbol == null || containingSymbol.Locations.Length <= 0)
			{
				return Location.None;
			}
			return symbol.ContainingSymbol?.Locations[0];
		}
		return locations[0];
	}

	internal static bool CanCompileWithTarget(this Symbol symbol, CompilationTarget target, ITypeSymbol accessedThroughType = null)
	{
		if (target.IsInternalTarget() || target.IsPersonalizationTarget())
		{
			return true;
		}
		if (target.IsExtensionTarget())
		{
			return CanCompileWithTarget(symbol, (SymbolCompilationScope scope) => scope == SymbolCompilationScope.Personalization || scope == SymbolCompilationScope.Extension, accessedThroughType);
		}
		if (target.IsSolutionTarget())
		{
			return CanCompileWithTarget(symbol, (SymbolCompilationScope scope) => scope == SymbolCompilationScope.Personalization || scope == SymbolCompilationScope.Extension || scope == SymbolCompilationScope.Solution, accessedThroughType);
		}
		return false;
	}

	internal static void CreateInsufficientScopeDiagnostic(this Symbol symbol, CompilationTarget target, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = (PropertySymbol)symbol;
		if (IsFullPropertyProhibited(propertySymbol, target))
		{
			diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, propertySymbol.Property.Syntax.Location, propertySymbol.Name, target.ToString());
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_PropertyValueCannotBeUsedInThisContext, propertySymbol.Property.Syntax.Location, propertySymbol.ValueText, propertySymbol.Name, target.ToString());
		}
	}

	internal static bool IsControlArea(this ISymbol c)
	{
		if (c.Kind == SymbolKind.Control)
		{
			return ((IControlSymbol)c).ControlKind == ControlKind.Area;
		}
		return false;
	}

	internal static bool IsActionArea(this ISymbol a)
	{
		if (a.Kind == SymbolKind.Action)
		{
			return ((IActionSymbol)a).ActionKind == ActionKind.Area;
		}
		return false;
	}

	internal static bool IsCueAction(this ISymbol a)
	{
		if (a.Kind == SymbolKind.Action)
		{
			return a.GetContainingSymbol().IsCueGroup();
		}
		return false;
	}

	internal static bool IsCueControl(this ISymbol a)
	{
		if (a.Kind == SymbolKind.Control)
		{
			return a.GetContainingSymbol().IsCueGroup();
		}
		return false;
	}

	internal static bool IsRepeaterGroup(this ISymbol a)
	{
		if (a.Kind == SymbolKind.Control)
		{
			return ((IControlSymbol)a).ControlKind == ControlKind.Repeater;
		}
		return false;
	}

	internal static bool IsCueGroup(this ISymbol a)
	{
		if (a.Kind == SymbolKind.Control)
		{
			return ((IControlSymbol)a).ControlKind == ControlKind.CueGroup;
		}
		return false;
	}

	internal static LookupOptions GetLookupOptions(this ISymbol type)
	{
		LookupOptions lookupOptions = LookupOptions.MemberAccess;
		SymbolKind kind = type.Kind;
		if (kind == SymbolKind.Class)
		{
			return lookupOptions | LookupOptions.AllMethods | LookupOptions.MustNotBeInstance;
		}
		return lookupOptions | LookupOptions.AllMethods | LookupOptions.MustBeInstance;
	}

	internal static bool IsKind(this ISymbol symbol, SymbolKind kind)
	{
		if (symbol == null)
		{
			return false;
		}
		return symbol.Kind == kind;
	}

	internal static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2)
	{
		if (symbol != null)
		{
			_ = symbol.Kind;
			return symbol.Kind.IsKind(kind1, kind2);
		}
		return false;
	}

	internal static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2, SymbolKind kind3)
	{
		if (symbol != null)
		{
			_ = symbol.Kind;
			return symbol.Kind.IsKind(kind1, kind2, kind3);
		}
		return false;
	}

	internal static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2, SymbolKind kind3, SymbolKind kind4)
	{
		if (symbol != null)
		{
			_ = symbol.Kind;
			return symbol.Kind.IsKind(kind1, kind2, kind3, kind4);
		}
		return false;
	}

	internal static bool TryGetSymbolsFromContainer(this IContainerSymbol container, Compilation compilation, int position, CancellationToken cancellationToken, out IEnumerable<ISymbol> symbols)
	{
		symbols = SpecializedCollections.EmptyEnumerable<ISymbol>();
		TypeSymbol typeSymbol = container as TypeSymbol;
		if (typeSymbol == null)
		{
			return false;
		}
		if (typeSymbol.TypeCategory == TypeCategoryKind.ApplicationObject && typeSymbol.OriginalDefinition != null)
		{
			GetSymbolsFromContainer(compilation, position, typeSymbol, cancellationToken, LookupOptions.Default, out symbols);
		}
		return !object.Equals(symbols, SpecializedCollections.EmptyEnumerable<ISymbol>());
	}

	internal static bool TryGetSymbolsFromOptionType(this IContainerSymbol container, Compilation compilation, int position, CancellationToken cancellationToken, out IEnumerable<ISymbol> symbols)
	{
		symbols = SpecializedCollections.EmptyEnumerable<ISymbol>();
		TypeSymbol typeSymbol = container as TypeSymbol;
		if (typeSymbol == null)
		{
			return false;
		}
		if (typeSymbol.Kind == SymbolKind.Class && typeSymbol.TypeCategory == TypeCategoryKind.ApplicationObject && typeSymbol.OriginalDefinition != null)
		{
			symbols = compilation.GetApplicationObjectTypeSymbolsAcrossModules(typeSymbol.NavTypeKind.ToSymbolKind());
		}
		else if (typeSymbol.NavTypeKind == NavTypeKind.Option && typeSymbol.OriginalDefinition != null)
		{
			GetSymbolsFromContainer(compilation, position, typeSymbol, cancellationToken, LookupOptions.MustBeOption, out symbols);
		}
		return !object.Equals(symbols, SpecializedCollections.EmptyEnumerable<ISymbol>());
	}

	internal static void ValidateRoleCenterPageReference(this PropertySymbol symbol, Compilation declaringCompilation, DiagnosticBag diagnostics)
	{
		if (declaringCompilation.Options.Target.IsPersonalizationTarget())
		{
			return;
		}
		ApplicationObjectTypeSymbol applicationObjectTypeSymbol = symbol.Property?.PropertyValue?.ValueObject as ApplicationObjectTypeSymbol;
		string text = symbol.Property?.PropertyValue?.ValueText;
		Location location = (symbol as SourcePropertySymbol)?.PropertySyntax.Value.Location;
		if (applicationObjectTypeSymbol == null)
		{
			diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, location, SymbolKind.PageCustomization.ToString(), text);
			return;
		}
		if (applicationObjectTypeSymbol.Kind != SymbolKind.Page)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidApplicationObjectIdentifier, location, text);
			return;
		}
		PageTypeSymbol obj = applicationObjectTypeSymbol as PageTypeSymbol;
		if ((object)obj == null || obj.PageType != PageTypeKind.RoleCenter)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidRoleCenterPage, location, text);
		}
	}

	internal static IEnumerable<T> GetMembersOfKind<T>(this ContainerSymbol symbol, SymbolKind kind)
	{
		return symbol?.GetMembers().Where((Symbol m) => m.Kind == kind).Cast<T>() ?? Enumerable.Empty<T>();
	}

	internal static IEnumerable<T> GetCodeMembersOfKind<T>(this ContainerSymbol symbol, SymbolKind kind)
	{
		return symbol?.GetCodeMembers().Where((Symbol m) => m.Kind == kind).Cast<T>() ?? Enumerable.Empty<T>();
	}

	internal static bool IsXmlPortNodeTableElement(this Symbol symbol)
	{
		if (symbol.Kind != SymbolKind.XmlPortNode)
		{
			return false;
		}
		XmlPortNodeSymbol xmlPortNodeSymbol = (XmlPortNodeSymbol)symbol;
		if (xmlPortNodeSymbol.SourceTypeKind == XmlPortSourceTypeKind.Table)
		{
			return xmlPortNodeSymbol.NodeXmlKind == XmlPortNodeXmlKind.Element;
		}
		return false;
	}

	internal static bool IsNullOrTypeMissing(this Symbol type)
	{
		if (!(type == null))
		{
			return type == MissingTypeSymbol.Instance;
		}
		return true;
	}

	internal static IEnumerable<IObjectTypeSymbol> Except<T>(this IEnumerable<T> first, IEnumerable<T> second) where T : IObjectTypeSymbol
	{
		PooledObjectTypeSymbolHashset symbolsHash = PooledObjectTypeSymbolHashset.GetInstance();
		try
		{
			foreach (T item2 in second)
			{
				IObjectTypeSymbol item = item2;
				symbolsHash.Add(item);
			}
			foreach (T item3 in first)
			{
				IObjectTypeSymbol objectTypeSymbol = item3;
				if (symbolsHash.Add(objectTypeSymbol))
				{
					yield return objectTypeSymbol;
				}
			}
		}
		finally
		{
			symbolsHash.Free();
		}
	}

	internal static IEnumerable<IObjectTypeSymbol> Common<T>(this IEnumerable<T> first, IEnumerable<T> second) where T : IObjectTypeSymbol
	{
		PooledObjectTypeSymbolHashset symbolsHash = PooledObjectTypeSymbolHashset.GetInstance();
		try
		{
			foreach (T item2 in second)
			{
				IObjectTypeSymbol item = item2;
				symbolsHash.Add(item);
			}
			foreach (T item3 in first)
			{
				IObjectTypeSymbol objectTypeSymbol = item3;
				if (symbolsHash.Remove(objectTypeSymbol))
				{
					yield return objectTypeSymbol;
				}
			}
		}
		finally
		{
			symbolsHash.Free();
		}
	}

	private static SyntaxNode GetSyntax(this ISymbol symbol)
	{
		if (symbol.IsSourceSymbol())
		{
			DebugAssertHelper.Assert(symbol.DeclaringSyntaxReferences.Length == 1, "This method must be updated with support for symbols with multiple declaring syntax references.");
			return symbol.DeclaringSyntaxReferences[0].GetSyntax();
		}
		return null;
	}

	private static string GetSymbolChainStringified(ISymbol symbol)
	{
		ISymbol symbol2 = symbol;
		StringBuilder stringBuilder = new StringBuilder();
		while (symbol2 != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" <- ");
			}
			stringBuilder.Append(symbol2.Kind);
			symbol2 = symbol2.ContainingSymbol;
		}
		return stringBuilder.ToString();
	}

	private static T? ParseSimplePropertyValueFromText<T>(string valueText) where T : struct
	{
		if (typeof(T) == typeof(bool))
		{
			if (bool.TryParse(valueText, out var result))
			{
				return result as T?;
			}
			if (int.TryParse(valueText, out var result2))
			{
				return (result2 != 0) as T?;
			}
		}
		else
		{
			if (typeof(T) == typeof(string))
			{
				return valueText as T?;
			}
			T result4;
			if (typeof(T) == typeof(int))
			{
				if (int.TryParse(valueText, out var result3))
				{
					return result3 as T?;
				}
			}
			else if (Enum.TryParse<T>(valueText, ignoreCase: true, out result4))
			{
				return result4;
			}
		}
		return null;
	}

	private static T? GetSimplePropertyValueFromBoundValue<T>(PropertySymbol property) where T : struct
	{
		BoundPropertyValue propertyValue = property.Property.PropertyValue;
		if (propertyValue.Kind == BoundKind.LiteralPropertyValue)
		{
			ConstantValue value = ((BoundLiteralPropertyValue)property.Property.PropertyValue).Value;
			if ((value.Discriminator == ConstantValueTypeDiscriminator.Int32 && typeof(T) == typeof(int)) || (value.Discriminator == ConstantValueTypeDiscriminator.Boolean && typeof(T) == typeof(bool)))
			{
				return (T)property.Value;
			}
		}
		if (propertyValue.Kind == BoundKind.EnumPropertyValue && Enum.TryParse<T>(property.ValueText, ignoreCase: true, out var result))
		{
			return result;
		}
		return null;
	}

	private static bool CanCompileWithTarget(Symbol symbol, Func<SymbolCompilationScope, bool> condition, ITypeSymbol accessedThroughType = null)
	{
		if (condition(symbol.CompilationScope))
		{
			return true;
		}
		if (symbol.CompilationScope == SymbolCompilationScope.DefinedByOwner)
		{
			ITypeSymbol typeSymbol = accessedThroughType ?? symbol.ContainingType;
			DebugAssertHelper.Assert(typeSymbol != null);
			return condition(typeSymbol.CompilationScope);
		}
		return false;
	}

	private static bool IsFullPropertyProhibited(PropertySymbol propertySymbol, CompilationTarget target)
	{
		if (propertySymbol.Property.PropertyValue.Kind == BoundKind.EnumPropertyValue)
		{
			EnumPropertyTypeInfo enumInfo = (EnumPropertyTypeInfo)propertySymbol.GetPropertyTypeInfo();
			if (enumInfo != null && enumInfo.Options.Any((string o) => string.Equals(enumInfo.GetScope(o).ToString(), target.ToString(), StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}
		}
		return true;
	}

	private static void GetSymbolsFromContainer(Compilation compilation, int position, TypeSymbol typeSymbol, CancellationToken cancellationToken, LookupOptions options, out IEnumerable<ISymbol> symbols)
	{
		IEnumerable<ISymbol> enumerable = SpecializedCollections.EmptyEnumerable<ISymbol>();
		TypeSymbol originalDefinition = typeSymbol.OriginalDefinition;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (originalDefinition.DeclaringCompilation == null)
		{
			enumerable = from s in originalDefinition.GetMembers()
				where AccessCheck.IsSymbolAccessible(s, compilation.CompiledModule, LookupOptions.Default, ref useSiteDiagnostics)
				select s;
		}
		else
		{
			foreach (SyntaxReference declaringSyntaxReference in originalDefinition.DeclaringSyntaxReferences)
			{
				SemanticModel semanticModel = originalDefinition.DeclaringCompilation.GetSemanticModel(declaringSyntaxReference.SyntaxTree);
				enumerable = enumerable.Concat(semanticModel.LookupSymbols(position, options, typeSymbol, null, cancellationToken));
			}
		}
		symbols = enumerable;
	}
}
