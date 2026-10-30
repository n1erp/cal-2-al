using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal class SymbolWithPropertiesVisitor : SymbolVisitor<DiagnosticBag, DiagnosticBag>
{
	private static readonly Lazy<SymbolWithPropertiesVisitor> instance = new Lazy<SymbolWithPropertiesVisitor>(() => new SymbolWithPropertiesVisitor());

	public static SymbolWithPropertiesVisitor Instance = instance.Value;

	private static readonly ImmutableArray<PropertyKind> MandatoryPropertyKindsInternal = ImmutableArray.Create(PropertyKind.EntitySetName, PropertyKind.EntityName);

	private static readonly ImmutableArray<PropertyKind> MandatoryPropertyKinds = ImmutableArray.Create(PropertyKind.EntitySetName, PropertyKind.EntityName, PropertyKind.APIPublisher, PropertyKind.APIGroup);

	public override DiagnosticBag VisitQueryColumn(QueryColumnSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		diagnostics = DefaultVisit(symbol, diagnostics);
		diagnostics = ValidateMethodProperty(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitQueryType(QueryTypeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		diagnostics = DefaultVisit(symbol, diagnostics);
		ValidateTopNumberOfRows(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitQueryDataItem(QueryDataItemSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		ValidateDataItemLink(symbol, diagnostics);
		return DefaultVisit(symbol, diagnostics);
	}

	private static void ValidateTopNumberOfRows(QueryTypeSymbol symbol, DiagnosticBag diagnostics)
	{
		BoundLiteralPropertyValue boundPropertyValue = symbol.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.TopNumberOfRows);
		if (boundPropertyValue != null && boundPropertyValue.Value.Int32Value < 0)
		{
			diagnostics.Add(ErrorCode.ERR_PropertyValueMustBePositive, boundPropertyValue.Syntax.GetLocation(), boundPropertyValue.GetPropertyTypeInfo().Name);
		}
	}

	private static void ValidateDataItemLink(QueryDataItemSymbol symbol, DiagnosticBag diagnostics)
	{
		bool flag = symbol.GetBoundPropertyValue<BoundDataItemLinkPropertyValue>(PropertyKind.DataItemLink) != null;
		bool flag2 = symbol.ContainingSymbol.Kind == SymbolKind.Query;
		bool flag3 = symbol.GetEnumPropertyValueOrDefault<SqlJoinTypeKind>(PropertyKind.SqlJoinType) == SqlJoinTypeKind.CrossJoin;
		if (flag2 & flag)
		{
			diagnostics.Add(ErrorCode.ERR_TopLevelDataItemCannotHaveLinkProperty, symbol.GetLocation());
		}
		else if (!flag2 && !flag && !flag3)
		{
			diagnostics.Add(ErrorCode.ERR_DataItemLinkMustBeSetForNestedDataItems, symbol.GetLocation());
		}
		else if (flag3 & flag)
		{
			diagnostics.Add(ErrorCode.ERR_PropertyValuesAreMutuallyExclusive, symbol.GetLocation(), PropertyKind.DataItemLink.ToString(), PropertyKind.SqlJoinType.ToString(), SqlJoinTypeKind.CrossJoin.ToString());
		}
	}

	private static DiagnosticBag ValidateMethodProperty(QueryColumnSymbol symbol, DiagnosticBag diagnostics)
	{
		PropertySymbol property = symbol.GetProperty(PropertyKind.Method);
		QueryColumnMethodKind? enumPropertyValue = symbol.GetEnumPropertyValue<QueryColumnMethodKind>(PropertyKind.Method);
		if (!enumPropertyValue.HasValue)
		{
			return diagnostics;
		}
		if (enumPropertyValue == QueryColumnMethodKind.Count)
		{
			return diagnostics;
		}
		if (enumPropertyValue.Value.IsAggregationMethod())
		{
			string text = new QueryColumnMethodKind[4]
			{
				QueryColumnMethodKind.Average,
				QueryColumnMethodKind.Sum,
				QueryColumnMethodKind.Max,
				QueryColumnMethodKind.Min
			}.Select((QueryColumnMethodKind x) => x.ToString()).Join(", ");
			if (!SemanticFacts.IsValidColumnTypeForAggregationMethod(symbol.RelatedField?.Type))
			{
				diagnostics.Add(ErrorCode.ERR_AggregationMethodsIncompatibleWithQueryColumnType, property.GetLocation(), text);
				return diagnostics;
			}
		}
		else
		{
			string text2 = new QueryColumnMethodKind[3]
			{
				QueryColumnMethodKind.Day,
				QueryColumnMethodKind.Month,
				QueryColumnMethodKind.Year
			}.Select((QueryColumnMethodKind x) => x.ToString()).Join(", ");
			if (!SemanticFacts.IsValidColumnTypeForDateMethod(symbol.RelatedField?.Type))
			{
				diagnostics.Add(ErrorCode.ERR_DateMethodsIncompatibleWithQueryColumnType, property.GetLocation(), text2);
				return diagnostics;
			}
		}
		return diagnostics;
	}

	public override DiagnosticBag DefaultVisit(Symbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		CheckForDuplicateProperties(symbol.Properties, diagnostics);
		CheckDependentProperties(symbol, diagnostics);
		CheckPageAndQueryOfTypeApiMandatoryProperties(symbol, diagnostics);
		CheckLayout(symbol.Properties, diagnostics);
		ValidatePermissions(symbol, diagnostics);
		CheckForLabelAndMultilanguageAtTheSameTime(symbol, diagnostics);
		VisitMembersOfSymbol(symbol, diagnostics);
		return diagnostics;
	}

	private static void CheckForLabelAndMultilanguageAtTheSameTime(Symbol symbolWithProperties, DiagnosticBag diagnostics)
	{
		ImmutableArray<PropertySymbol> properties = symbolWithProperties.Properties;
		foreach (PropertySymbol item in properties)
		{
			foreach (PropertySymbol item2 in properties)
			{
				if (item.Property.PropertyValue.Kind == BoundKind.MultilanguagePropertyValue && item2.Property.PropertyValue.Kind == BoundKind.LabelPropertyValue && item.Name.Length - 2 == item2.Name.Length && item.Name.StartsWith(item2.Name, StringComparison.Ordinal))
				{
					diagnostics.Add(ErrorCode.ERR_CannotSpecifyLabelAndMultilanguageSameTime, item.GetLocation(), item.Name, item2.Name);
					diagnostics.Add(ErrorCode.ERR_CannotSpecifyLabelAndMultilanguageSameTime, item2.GetLocation(), item.Name, item2.Name);
				}
			}
		}
	}

	private void VisitMembersOfSymbol(Symbol symbol, DiagnosticBag diagnostics)
	{
		if (!(symbol is ContainerSymbol containerSymbol))
		{
			return;
		}
		foreach (Symbol member in containerSymbol.GetMembers())
		{
			if (member.Kind != SymbolKind.DotNetTypeDeclaration)
			{
				member.Accept(this, diagnostics);
			}
		}
	}

	private static void CheckDependentProperties(Symbol symbolWithProperties, DiagnosticBag diagnostics)
	{
		foreach (PropertySymbol property in symbolWithProperties.Properties)
		{
			PropertyTypeInfo propertyTypeInfo = symbolWithProperties.GetPropertyTypeInfo(property.Name);
			if (propertyTypeInfo == null)
			{
				continue;
			}
			Symbol symbol = (symbolWithProperties as ChangeModifySymbol)?.Target as Symbol;
			TypeSymbol typeSymbol = ((symbol != null && symbol.PropertyTypeInfos.Count != 0) ? symbol.GetTypeSymbol() : symbolWithProperties.GetTypeSymbol());
			if (typeSymbol != null)
			{
				VerifyTypeDependencies(typeSymbol, diagnostics, property, propertyTypeInfo);
			}
			foreach (DependentProperty dependentProperty in propertyTypeInfo.DependentProperties)
			{
				VerifyDependentProperty(symbolWithProperties, property, dependentProperty, diagnostics);
			}
			foreach (DependentProperty dependentDeclaringApplicationObjectProperty in propertyTypeInfo.DependentDeclaringApplicationObjectProperties)
			{
				VerifyApplicationObjectDependency(dependentDeclaringApplicationObjectProperty, property, diagnostics);
			}
		}
	}

	private static void CheckPageAndQueryOfTypeApiMandatoryProperties(Symbol symbolWithProperties, DiagnosticBag diagnostics)
	{
		switch (symbolWithProperties.Kind)
		{
		case SymbolKind.Page:
			CheckPageAndQueryOfTypeApiMandatoryProperties(symbolWithProperties, PropertyKind.PageType, PageTypeKind.API, diagnostics);
			break;
		case SymbolKind.Query:
			CheckPageAndQueryOfTypeApiMandatoryProperties(symbolWithProperties, PropertyKind.QueryType, QueryTypeKind.API, diagnostics);
			break;
		case SymbolKind.Control:
			CheckPagePartOnPageOfTypeApiMandatoryProperties(symbolWithProperties, diagnostics);
			break;
		}
	}

	private static void CheckPageAndQueryOfTypeApiMandatoryProperties<TApiKind>(Symbol symbolWithProperties, PropertyKind objectTypePropertyKind, TApiKind apiKind, DiagnosticBag diagnostics) where TApiKind : struct
	{
		if (!IsObjectOfTypeApi(symbolWithProperties, objectTypePropertyKind, apiKind, out var objectTypeProperty))
		{
			return;
		}
		bool flag = symbolWithProperties.DeclaringCompilation.Options.Target == CompilationTarget.Internal;
		ImmutableArray<PropertyKind> immutableArray = MandatoryPropertyKindsInternal;
		if (!flag)
		{
			immutableArray = MandatoryPropertyKinds;
		}
		else
		{
			PropertySymbol property = symbolWithProperties.GetProperty(PropertyKind.APIPublisher);
			PropertySymbol property2 = symbolWithProperties.GetProperty(PropertyKind.APIGroup);
			if (property != null && property2 == null)
			{
				diagnostics.Add(ErrorCode.ERR_MissingAssociatedProperty, objectTypeProperty.GetLocation(), PropertyKind.APIPublisher, PropertyKind.APIGroup);
			}
			else if (property2 != null && property == null)
			{
				diagnostics.Add(ErrorCode.ERR_MissingAssociatedProperty, objectTypeProperty.GetLocation(), PropertyKind.APIGroup, PropertyKind.APIPublisher);
			}
		}
		foreach (PropertyKind item in immutableArray)
		{
			if (symbolWithProperties.GetProperty(item) == null)
			{
				diagnostics.Add(ErrorCode.ERR_MandatoryProperty, objectTypeProperty.GetLocation(), item, objectTypePropertyKind, symbolWithProperties.Kind, apiKind);
			}
		}
	}

	private static bool IsObjectOfTypeApi<TApiKind>(Symbol symbolWithProperties, PropertyKind objectTypePropertyKind, TApiKind apiKind, out PropertySymbol objectTypeProperty) where TApiKind : struct
	{
		objectTypeProperty = symbolWithProperties.GetProperty(objectTypePropertyKind);
		if (objectTypeProperty == null)
		{
			return false;
		}
		TApiKind? valueAsEnum = objectTypeProperty.GetValueAsEnum<TApiKind>();
		if (!valueAsEnum.HasValue)
		{
			return false;
		}
		if (!EqualityComparer<TApiKind>.Default.Equals(valueAsEnum.Value, apiKind))
		{
			return false;
		}
		return true;
	}

	private static void CheckPagePartOnPageOfTypeApiMandatoryProperties(Symbol symbolWithProperties, DiagnosticBag diagnostics)
	{
		ControlSymbol controlSymbol = (ControlSymbol)symbolWithProperties;
		if (controlSymbol.ControlKind != ControlKind.Part)
		{
			return;
		}
		IPageBaseTypeSymbol containingPage = controlSymbol.GetContainingPage();
		if (containingPage == null || containingPage.PageType != PageTypeKind.API)
		{
			return;
		}
		HashSet<PropertyKind> presentPropertyKinds = new HashSet<PropertyKind>(controlSymbol.Properties.Select((PropertySymbol p) => p.PropertyKind));
		foreach (PropertyKind item in new List<PropertyKind>
		{
			PropertyKind.EntitySetName,
			PropertyKind.EntityName
		}.Where((PropertyKind p) => !presentPropertyKinds.Contains(p)))
		{
			diagnostics.Add(ErrorCode.ERR_MandatoryProperty, controlSymbol.GetLocation(), item, PropertyKind.PageType, SymbolKind.Page, PageTypeKind.API);
		}
	}

	private static void VerifyDependentProperty(Symbol symbolWithDependentProperties, Symbol dependeeSymbol, DependentProperty dependentProperty, DiagnosticBag diagnostics)
	{
		if (!IsPropertyDependencySatisfied(symbolWithDependentProperties, dependentProperty))
		{
			SetError(dependentProperty, dependeeSymbol, diagnostics);
		}
	}

	private static void VerifyTypeDependencies(ITypeSymbol typeSymbol, DiagnosticBag diagnostics, PropertySymbol propertySymbol, PropertyTypeInfo info)
	{
		string typeName = typeSymbol.NavTypeKind.ToString();
		info?.DependentParentProperties.Do((DependentProperty dependentProperty) =>
		{
			if (!dependentProperty.AllowedValues.Contains(typeName))
			{
				SetError(dependentProperty, propertySymbol, diagnostics);
			}
		});
	}

	private static void SetError(DependentProperty dependentProperty, Symbol propertySymbol, DiagnosticBag diagnostics)
	{
		switch (dependentProperty.AllowedValues.Count)
		{
		case 0:
		{
			ErrorCode code = ErrorCode.ERR_MissingAssociatedProperty;
			diagnostics.Add(code, propertySymbol.GetLocation(), propertySymbol.Name, dependentProperty.Name);
			break;
		}
		case 1:
		{
			ErrorCode code = ErrorCode.ERR_MissingAssociatedPropertySingleValue;
			diagnostics.Add(code, propertySymbol.GetLocation(), propertySymbol.Name, dependentProperty.Name, dependentProperty.ConcatenateValues());
			break;
		}
		default:
		{
			ErrorCode code = ErrorCode.ERR_MissingAssociatedPropertyMultipleValues;
			diagnostics.Add(code, propertySymbol.GetLocation(), propertySymbol.Name, dependentProperty.Name, dependentProperty.ConcatenateValues());
			break;
		}
		}
	}

	private static void VerifyApplicationObjectDependency(DependentProperty dependentProperty, PropertySymbol ownerProperty, DiagnosticBag diagnostics)
	{
		ApplicationObjectTypeSymbol containingSymbolOfType = ownerProperty.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		if (containingSymbolOfType != null)
		{
			VerifyDependentProperty(containingSymbolOfType, ownerProperty, dependentProperty, diagnostics);
		}
	}

	internal static bool IsPropertyDependencySatisfied(Symbol symbolWithDependentProperties, DependentProperty dependentProperty)
	{
		PropertySymbol propertySymbol = symbolWithDependentProperties.Properties.FirstOrDefault((PropertySymbol p) => p.PropertyKind == SyntaxFacts.GetPropertyKind(dependentProperty.Name));
		string defaultValue = symbolWithDependentProperties.GetPropertyTypeInfo(dependentProperty.Name).DefaultValue;
		if (((SourcePropertySymbol)propertySymbol)?.Syntax is PropertySyntax { Value: var value } propertySyntax)
		{
			string text = value?.GetPropertyValueText();
			if (text != null)
			{
				if ((NavTypeExtensions.FromLiteralSyntaxKind(propertySyntax.Value.Kind) == NavTypeKind.Boolean && (bool)propertySyntax.Value.GetPropertyValue()) || dependentProperty.AllowedValues.Contains(text, SemanticFacts.NameEqualityComparer))
				{
					return true;
				}
			}
			else if (!propertySyntax.HasErrors)
			{
				return true;
			}
		}
		else if (defaultValue != null && dependentProperty.AllowedValues.Contains(defaultValue))
		{
			return true;
		}
		return false;
	}

	public override DiagnosticBag VisitControl(ControlSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		if (symbol.ControlKind == ControlKind.Field)
		{
			ValidatePropertiesForFieldDependency(symbol.Properties, symbol.Type, diagnostics);
		}
		else
		{
			CheckClientExpressions(symbol.Properties, clientExpressionsIsError: false, diagnostics);
		}
		DefaultVisit(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitAction(ActionSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		CheckClientExpressions(symbol.Properties, clientExpressionsIsError: false, diagnostics);
		DefaultVisit(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitField(FieldSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		ValidatePropertiesForFieldDependency(symbol.Properties, symbol.Type, diagnostics);
		DefaultVisit(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitDotNetTypeDeclaration(DotNetTypeDeclarationSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		BoundLiteralPropertyValue boundPropertyValue = symbol.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.IsControlAddIn);
		if (boundPropertyValue != null && boundPropertyValue.ConstantValue.BooleanValue)
		{
			DotNetTypeSymbol dotNetType = symbol.DotNetType;
			if ((object)dotNetType != null && dotNetType.DotNetTypeInfo?.IsControlAddIn == false)
			{
				diagnostics.Add(ErrorCode.ERR_DotNetTypeIsNotControlAddIn, symbol.GetLocation(), symbol.TypeName);
			}
		}
		DefaultVisit(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitChange(ChangeSymbol symbol, DiagnosticBag diagnostics)
	{
		SourceChangeModifySymbol sourceChangeModifySymbol = symbol as SourceChangeModifySymbol;
		Symbol symbol2 = sourceChangeModifySymbol?.Target as Symbol;
		if (symbol2 != null && symbol2.PropertyTypeInfos.Count != 0)
		{
			TypeSymbol typeSymbol = symbol2.GetTypeSymbol();
			ValidatePropertiesForFieldDependency(sourceChangeModifySymbol.Properties, typeSymbol, diagnostics);
			ValidateGridLayoutChange(sourceChangeModifySymbol, symbol2, diagnostics);
			ValidateCueGroupLayoutChange(sourceChangeModifySymbol, symbol2, diagnostics);
			ValidateExtensionProperties(sourceChangeModifySymbol, symbol2, diagnostics);
		}
		DefaultVisit(symbol, diagnostics);
		return diagnostics;
	}

	public override DiagnosticBag VisitFieldNameList(FieldNameListSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.IsSourceSymbol())
		{
			return diagnostics;
		}
		if (symbol.IsKind(SymbolKind.Key))
		{
			ValidateKeyProperties(symbol, diagnostics);
		}
		DefaultVisit(symbol, diagnostics);
		return diagnostics;
	}

	private static void ValidateKeyProperties(FieldNameListSymbol symbol, DiagnosticBag diagnostics)
	{
		ValidateSumIndexFields(symbol, diagnostics);
	}

	private static void ValidateSumIndexFields(FieldNameListSymbol symbol, DiagnosticBag diagnostics)
	{
		BoundProperty boundProperty = symbol.GetPropertySymbol(PropertyKind.SumIndexFields)?.Property;
		if (boundProperty?.PropertyValue is BoundIdentifierListPropertyValue boundIdentifierListPropertyValue)
		{
			ImmutableArray<FieldSymbol> fields = (from BoundFieldAccess x in boundIdentifierListPropertyValue.Value
				select x.FieldSymbol).ToImmutableArray();
			if (!TryValidateFieldsDefinedOnSumIndexField(symbol.ContainingType, fields, out var diagnosticInfo))
			{
				diagnostics.Add(diagnosticInfo, boundProperty.Syntax.Location);
			}
		}
	}

	private static bool TryValidateFieldsDefinedOnSumIndexField(Symbol containingSymbol, ImmutableArray<FieldSymbol> fields, out NavDiagnosticInfo diagnosticInfo)
	{
		diagnosticInfo = null;
		if (fields.IsDefaultOrEmpty)
		{
			return true;
		}
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].ContainingType != containingSymbol)
			{
				diagnosticInfo = new NavDiagnosticInfo(ErrorCode.ERR_PropertyCanBeUsedOnlyOnKeysWithFieldsFromTheSameTable, "SumIndexFields");
				return false;
			}
			if (!fields[i].HasValidFieldClassForKey())
			{
				diagnosticInfo = new NavDiagnosticInfo(ErrorCode.ERR_FieldWithInvalidFieldClassAsSumIndexField, fields[i].Name);
				return false;
			}
		}
		return true;
	}

	private static void ValidateExtensionProperties(SourceChangeModifySymbol modifySymbol, Symbol target, DiagnosticBag diagnostics)
	{
		foreach (PropertySymbol property in modifySymbol.Properties)
		{
			PropertyTypeInfo propertyTypeInfo = target.GetPropertyTypeInfo(property.Name);
			SymbolKind kind = property.ContainingType.Kind;
			if (propertyTypeInfo == null)
			{
				diagnostics.Add(ErrorCode.ERR_PropertyInfoNotAvailable, property.GetLocation(), property.Name);
			}
			else if (propertyTypeInfo.Modification == ModificationKind.None)
			{
				diagnostics.Add(ErrorCode.ERR_NonCustomizableProperty, property.GetLocation(), propertyTypeInfo.Name);
			}
			else if (kind == SymbolKind.PageCustomization || kind == SymbolKind.View)
			{
				if (propertyTypeInfo.Modification == ModificationKind.Customizations)
				{
					VersionChecker.CheckPropertyModificationAvailability(ModificationKind.Customizations, propertyTypeInfo, (PropertySyntax)property.GetFirstSyntax(), diagnostics);
					continue;
				}
				diagnostics.Add(ErrorCode.ERR_NonCustomizableProperty, property.GetLocation(), propertyTypeInfo.Name);
			}
		}
	}

	private static void ValidateGridLayoutChange(SourceChangeModifySymbol changeSymbol, Symbol target, DiagnosticBag diagnostics)
	{
		PropertySymbol property = changeSymbol.GetProperty(PropertyKind.GridLayout);
		if (property != null && (target.Kind != SymbolKind.Control || ((ControlSymbol)target).ControlKind != ControlKind.Grid))
		{
			diagnostics.Add(ErrorCode.ERR_GridLayoutMustBeOnGrid, ((SourcePropertySymbol)property).GetLocation());
		}
	}

	private static void ValidateCueGroupLayoutChange(SourceChangeModifySymbol changeSymbol, Symbol target, DiagnosticBag diagnostics)
	{
		PropertySymbol property = changeSymbol.GetProperty(PropertyKind.CuegroupLayout);
		if (property != null && (target.Kind != SymbolKind.Control || ((ControlSymbol)target).ControlKind != ControlKind.CueGroup))
		{
			diagnostics.Add(ErrorCode.ERR_CuegroupLayoutMustBeOnCuegroup, ((SourcePropertySymbol)property).GetLocation());
		}
	}

	private static void ValidatePropertiesForFieldDependency(ImmutableArray<PropertySymbol> fieldProperties, TypeSymbol fieldType, DiagnosticBag diagnostics)
	{
		ValidateMinMaxInit(fieldProperties, fieldType, PropertyKind.InitValue, diagnostics);
		ValidateMinMaxInit(fieldProperties, fieldType, PropertyKind.MinValue, diagnostics);
		ValidateMinMaxInit(fieldProperties, fieldType, PropertyKind.MaxValue, diagnostics);
		ValidateExtendedDataType(fieldProperties, fieldType, diagnostics);
		ValidateOptionOrdinalValues(fieldProperties, diagnostics);
		CheckClientExpressions(fieldProperties, clientExpressionsIsError: true, diagnostics);
	}

	private static void ValidateOptionOrdinalValues(ImmutableArray<PropertySymbol> symbolProperties, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = symbolProperties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.OptionOrdinalValues);
		BoundProperty boundProperty = propertySymbol?.Property;
		if (boundProperty != null && boundProperty.PropertyValue is BoundConstantValueListPropertyValue boundConstantValueListPropertyValue)
		{
			ValidateOptionOrdinalValuesDependencies(propertySymbol, diagnostics);
			int length = boundConstantValueListPropertyValue.Values.Length;
			int num = ((symbolProperties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.OptionMembers)?.Property)?.PropertyValue as BoundOptionMembersPropertyValue)?.Value?.GetMembers().Length ?? 0;
			if (length != num)
			{
				diagnostics.Add(ErrorCode.ERR_InvalidNumberOfOptionOrdinalValues, boundProperty.Syntax.Location);
			}
		}
	}

	private static void ValidateOptionOrdinalValuesDependencies(PropertySymbol propertySymbol, DiagnosticBag diagnostics)
	{
		ApplicationObjectTypeSymbol containingSymbolOfType = propertySymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		if (containingSymbolOfType == null || !SystemTables.IsOptionalOrdinalValuesAllowed(containingSymbolOfType.Id))
		{
			DependentProperty dependentProperty = new DependentProperty("ExternalType('Picklist','State','Status')");
			VerifyDependentProperty(propertySymbol.ContainingSymbol, propertySymbol, dependentProperty, diagnostics);
			DependentProperty dependentProperty2 = new DependentProperty("TableType(CRM)");
			VerifyDependentProperty(containingSymbolOfType, propertySymbol, dependentProperty2, diagnostics);
		}
	}

	private static void ValidateExtendedDataType(ImmutableArray<PropertySymbol> symbolProperties, TypeSymbol fieldType, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = symbolProperties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.ExtendedDatatype);
		if (propertySymbol == null)
		{
			return;
		}
		ExtendedDatatypeKind? valueAsEnum = propertySymbol.GetValueAsEnum<ExtendedDatatypeKind>();
		if (valueAsEnum.HasValue)
		{
			ExtendedDatatypeKind value = valueAsEnum.Value;
			NavTypeKind navTypeKind = fieldType.NavTypeKind;
			ErrorCode errorCode;
			switch (value)
			{
			default:
				return;
			case ExtendedDatatypeKind.Ratio:
				errorCode = navTypeKind.ValidateRatio();
				break;
			case ExtendedDatatypeKind.PhoneNo:
			case ExtendedDatatypeKind.URL:
			case ExtendedDatatypeKind.EMail:
				errorCode = navTypeKind.ValidateForText();
				break;
			case ExtendedDatatypeKind.Person:
				errorCode = navTypeKind.ValidateForMedia();
				break;
			case ExtendedDatatypeKind.None:
			case ExtendedDatatypeKind.Masked:
				return;
			}
			if (errorCode != ErrorCode.None)
			{
				diagnostics.Add(errorCode, propertySymbol.Property.PropertyValue.Syntax.Location, propertySymbol.ContainingSymbol.Name, propertySymbol.ValueText);
			}
		}
	}

	private static void CheckLayout(ImmutableArray<PropertySymbol> fieldProperties, DiagnosticBag diagnostics)
	{
		foreach (PropertySymbol item in fieldProperties)
		{
			if (item.PropertyKind == PropertyKind.RDLCLayout || item.PropertyKind == PropertyKind.WordLayout)
			{
				IFileSystem fileSystem = item.DeclaringCompilation.FileSystem ?? throw ExceptionUtilities.Unreachable;
				string valueText = item.ValueText;
				if (fileSystem.Exists(valueText) && PathUtilities.IsAbsolute(valueText))
				{
					diagnostics.Add(ErrorCode.ERR_PathMustBeRelative, item.GetLocation());
				}
				if (fileSystem.Exists(valueText) && !ValidLayoutFilename(valueText, item.PropertyKind))
				{
					diagnostics.Add(ErrorCode.ERR_LayoutNotSupportedFileFormat, item.GetLocation(), valueText);
				}
			}
		}
	}

	private static bool ValidLayoutFilename(string filename, PropertyKind propertyKind)
	{
		switch (propertyKind)
		{
		case PropertyKind.WordLayout:
			return string.Equals(Path.GetExtension(filename), ".docx", StringComparison.OrdinalIgnoreCase);
		case PropertyKind.RDLCLayout:
			if (!string.Equals(Path.GetExtension(filename), ".rdl", StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(Path.GetExtension(filename), ".rdlc", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		default:
			return false;
		}
	}

	private static void ValidateMinMaxInit(ImmutableArray<PropertySymbol> fieldProperties, TypeSymbol fieldType, PropertyKind targetProperty, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = fieldProperties.Where((PropertySymbol p) => p.PropertyKind == targetProperty).FirstIfSingle();
		if (propertySymbol == null)
		{
			return;
		}
		BoundProperty property = propertySymbol.Property;
		if (property == null)
		{
			return;
		}
		if (targetProperty == PropertyKind.InitValue && fieldType.NavTypeKind == NavTypeKind.DateFormula)
		{
			NavDiagnosticInfo navDiagnosticInfo = new DateFormulaEvaluator(property.PropertyValue.ValueText).Parse();
			if (navDiagnosticInfo != null)
			{
				diagnostics.Add(navDiagnosticInfo.Code, property.PropertyValue.Syntax.Location, navDiagnosticInfo.Arguments);
			}
			return;
		}
		BoundPropertyValue propertyValue = property.PropertyValue;
		Conversions conversions = new Conversions();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = conversions.ClassifyConversionFromExpression(propertyValue, fieldType, ref useSiteDiagnostics);
		diagnostics.Add(property.Syntax, useSiteDiagnostics);
		if (fieldType.NavTypeKind == NavTypeKind.Enum)
		{
			ValidateEnumMinMaxInit((EnumTypeSymbol)fieldType, propertyValue, diagnostics);
		}
		else if (!conversion.IsImplicit || conversion.Kind.IsImplicitToSmaller() || (fieldType.NavTypeKind != NavTypeKind.Option && propertyValue.Type.NavTypeKind == NavTypeKind.Option))
		{
			string text = propertyValue.Syntax.GetText().ToString();
			if (propertyValue.Kind == BoundKind.BadPropertyValue)
			{
				text = ((PropertySyntax)propertyValue.Syntax).Name.Identifier.ValueText;
			}
			diagnostics.Add(ErrorCode.ERR_PropertyTypeFieldTypeMismatch, propertyValue.Syntax.Location, text);
		}
		else if (fieldType.NavTypeKind == NavTypeKind.Option)
		{
			ValidateOptionMinMaxInit(fieldProperties, propertyValue, diagnostics);
		}
	}

	private static void ValidateOptionMinMaxInit(ImmutableArray<PropertySymbol> fieldProperties, BoundPropertyValue propertyValue, DiagnosticBag diagnostics)
	{
		ImmutableArray<PropertySymbol> immutableArray = fieldProperties.WhereAsArray((PropertySymbol p) => p.PropertyKind == PropertyKind.OptionMembers);
		if (immutableArray.Count() != 1)
		{
			return;
		}
		BoundProperty property = immutableArray.First().Property;
		if (property == null || property.Kind == BoundKind.BadProperty)
		{
			return;
		}
		DebugAssertHelper.Assert(property.PropertyValue.Kind == BoundKind.OptionMembersPropertyValue);
		BoundOptionMembersPropertyValue boundOptionMembersPropertyValue = (BoundOptionMembersPropertyValue)property.PropertyValue;
		DebugAssertHelper.Assert(boundOptionMembersPropertyValue.Value.Kind == SymbolKind.OptionType);
		SourceOptionTypeSymbol sourceOptionTypeSymbol = (SourceOptionTypeSymbol)boundOptionMembersPropertyValue.Value;
		if (propertyValue.Type.NavTypeKind == NavTypeKind.Option)
		{
			string name = propertyValue.ValueText.UnquoteIdentifier();
			if (sourceOptionTypeSymbol.GetMembers(name).Length == 0)
			{
				diagnostics.Add(ErrorCode.ERR_PropertyValueNotAValidOptionValue, propertyValue.Syntax.Location, propertyValue.Syntax.GetText());
			}
			return;
		}
		Conversions conversions = new Conversions();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = conversions.ClassifyConversionFromExpression(propertyValue, new SimpleNavTypeSymbol(NavTypeKind.Integer), ref useSiteDiagnostics);
		diagnostics.Add(propertyValue.Syntax, useSiteDiagnostics);
		DebugAssertHelper.Assert(int.TryParse(propertyValue.ValueText, out var result));
		if (conversion.IsValid && int.TryParse(propertyValue.ValueText, out result) && (result < 1 || result > sourceOptionTypeSymbol.OptionsDefinition.Count))
		{
			diagnostics.Add(ErrorCode.ERR_PropertyValueNotAValidOptionValue, propertyValue.Syntax.Location, propertyValue.Syntax.GetText());
		}
	}

	private static void ValidateEnumMinMaxInit(EnumTypeSymbol fieldType, BoundPropertyValue propertyValue, DiagnosticBag diagnostics)
	{
		if (fieldType.Values.FirstOrDefault((EnumValueSymbol v) => SemanticFacts.IsSameName(v.Name, propertyValue.ValueText)) == null)
		{
			diagnostics.Add(ErrorCode.ERR_PropertyValueNotAValidEnumValue, propertyValue.Syntax.Location, propertyValue.Syntax.GetText(), fieldType.Name);
		}
	}

	private static void CheckForDuplicateProperties(ImmutableArray<PropertySymbol> fieldProperties, DiagnosticBag diagnostics)
	{
		(from p in fieldProperties
			where p.PropertyKind != PropertyKind.None
			group p by p.PropertyKind into @group
			where @group.Count() > 1
			select @group).Do((IGrouping<PropertyKind, PropertySymbol> propertyGroup) =>
		{
			propertyGroup.Do((PropertySymbol property) =>
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateProperty, ((SourcePropertySymbol)property).PropertySyntax.Name.Location);
			});
		});
	}

	private static void CheckClientExpressions(ImmutableArray<PropertySymbol> properties, bool clientExpressionsIsError, DiagnosticBag diagnostics)
	{
		foreach (PropertySymbol item in properties)
		{
			BoundProperty property = item.Property;
			if (property != null && property.PropertyValue.Syntax.Kind.IsPropertyClientExpresion() && !item.Property.HasAnyErrors)
			{
				MarkVariablesWithInDataSet(item, clientExpressionsIsError, diagnostics);
			}
		}
	}

	private static void MarkVariablesWithInDataSet(PropertySymbol propertySymbol, bool clientExpressionsIsError, DiagnosticBag diagnostics)
	{
		BoundProperty property = propertySymbol.Property;
		if (propertySymbol.Value != null)
		{
			InDataSetExpressionVisitor inDataSetExpressionVisitor = new InDataSetExpressionVisitor(clientExpressionsIsError, diagnostics);
			switch (property.PropertyValue.Kind)
			{
			case BoundKind.CodeExpressionPropertyValue:
			{
				BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue = (BoundCodeExpressionPropertyValue)property.PropertyValue;
				inDataSetExpressionVisitor.Visit(boundCodeExpressionPropertyValue.Expression);
				break;
			}
			case BoundKind.StyleExpressionPropertyValue:
			{
				BoundStyleExpressionPropertyValue boundStyleExpressionPropertyValue = (BoundStyleExpressionPropertyValue)property.PropertyValue;
				inDataSetExpressionVisitor.Visit(boundStyleExpressionPropertyValue.Expression);
				break;
			}
			default:
				inDataSetExpressionVisitor.Visit(property.PropertyValue);
				break;
			}
		}
	}

	private static void ValidatePermissions(Symbol symbol, DiagnosticBag diagnostics)
	{
		PropertySymbol propertySymbol = symbol.Properties.Where((PropertySymbol p) => p.PropertyKind == PropertyKind.Permissions).FirstIfSingle();
		if (!(propertySymbol == null) && propertySymbol?.Property?.PropertyValue is BoundPermissionPropertyValue boundPermissionPropertyValue)
		{
			ValidateDuplicateReferencesInPermissions(boundPermissionPropertyValue.Value, diagnostics);
		}
	}

	private static void ValidateDuplicateReferencesInPermissions(ImmutableArray<BoundPermissionExpression> permissionsObjects, DiagnosticBag diagnostics)
	{
		PooledDictionary<TypeSymbol, BoundPermissionExpression> pooledDictionary = PooledDictionary<TypeSymbol, BoundPermissionExpression>.GetInstance();
		PooledHashSet<BoundPermissionExpression> pooledHashSet = PooledHashSet<BoundPermissionExpression>.GetInstance();
		try
		{
			foreach (BoundPermissionExpression item in permissionsObjects)
			{
				ApplicationObjectTypeSymbol applicationObjectTypeSymbol = ((BoundApplicationObjectAccess)item.Target).ApplicationObjectTypeSymbol;
				if (pooledDictionary.ContainsKey(applicationObjectTypeSymbol))
				{
					pooledHashSet.Add(item);
					pooledHashSet.Add(pooledDictionary[applicationObjectTypeSymbol]);
				}
				else
				{
					pooledDictionary.Add(applicationObjectTypeSymbol, item);
				}
			}
			foreach (BoundPermissionExpression item2 in pooledHashSet)
			{
				diagnostics.Add(ErrorCode.ERR_PermissionDuplicatedObject, item2.Syntax.GetLocation(), ((PermissionSyntax)item2.Syntax).ObjectReference.Identifier);
			}
		}
		finally
		{
			pooledDictionary.Free();
			pooledHashSet.Free();
		}
	}
}
