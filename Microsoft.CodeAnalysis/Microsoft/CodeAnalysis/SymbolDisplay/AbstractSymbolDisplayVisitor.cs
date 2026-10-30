using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.SymbolDisplay;

internal abstract class AbstractSymbolDisplayVisitor : SymbolVisitor
{
	protected readonly ArrayBuilder<SymbolDisplayPart> builder;

	protected readonly SymbolDisplayFormat format;

	protected readonly bool isFirstSymbolVisited;

	protected readonly bool inNamespaceOrType;

	protected readonly SemanticModel semanticModelOpt;

	protected readonly int positionOpt;

	private AbstractSymbolDisplayVisitor _lazyNotFirstVisitor;

	private AbstractSymbolDisplayVisitor _lazyNotFirstVisitorNamespaceOrType;

	protected AbstractSymbolDisplayVisitor NotFirstVisitor
	{
		get
		{
			if (_lazyNotFirstVisitor == null)
			{
				_lazyNotFirstVisitor = MakeNotFirstVisitor();
			}
			return _lazyNotFirstVisitor;
		}
	}

	protected AbstractSymbolDisplayVisitor NotFirstVisitorNamespaceOrType
	{
		get
		{
			if (_lazyNotFirstVisitorNamespaceOrType == null)
			{
				_lazyNotFirstVisitorNamespaceOrType = MakeNotFirstVisitor(inNamespaceOrType: true);
			}
			return _lazyNotFirstVisitorNamespaceOrType;
		}
	}

	protected bool IsMinimizing => semanticModelOpt != null;

	protected AbstractSymbolDisplayVisitor(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, bool isFirstSymbolVisited, SemanticModel semanticModelOpt, int positionOpt, bool inNamespaceOrType = false)
	{
		this.builder = builder;
		this.format = format;
		this.isFirstSymbolVisited = isFirstSymbolVisited;
		this.semanticModelOpt = semanticModelOpt;
		this.positionOpt = positionOpt;
		this.inNamespaceOrType = inNamespaceOrType;
		if (!isFirstSymbolVisited)
		{
			_lazyNotFirstVisitor = this;
		}
	}

	protected abstract AbstractSymbolDisplayVisitor MakeNotFirstVisitor(bool inNamespaceOrType = false);

	protected abstract void AddLiteralValue(SpecialType type, object value);

	protected abstract void AddExplicitlyCastedLiteralValue(INamedTypeSymbol namedType, SpecialType type, object value);

	protected abstract void AddSpace();

	protected abstract void AddBitwiseOr();

	protected void AddNonNullConstantValue(ITypeSymbol type, object constantValue, bool preferNumericValueOrExpandedFlagsForEnum = false)
	{
		if (type.TypeKind == TypeKind.Enum)
		{
			AddEnumConstantValue((INamedTypeSymbol)type, constantValue, preferNumericValueOrExpandedFlagsForEnum);
		}
		else
		{
			AddLiteralValue(type.SpecialType, constantValue);
		}
	}

	private void AddEnumConstantValue(INamedTypeSymbol enumType, object constantValue, bool preferNumericValueOrExpandedFlags)
	{
		if (IsFlagsEnum(enumType))
		{
			AddFlagsEnumConstantValue(enumType, constantValue, preferNumericValueOrExpandedFlags);
		}
		else if (preferNumericValueOrExpandedFlags)
		{
			AddLiteralValue(enumType.EnumUnderlyingType.SpecialType, constantValue);
		}
		else
		{
			AddNonFlagsEnumConstantValue(enumType, constantValue);
		}
	}

	private static bool IsFlagsEnum(ITypeSymbol typeSymbol)
	{
		if (typeSymbol.TypeKind != TypeKind.Enum)
		{
			return false;
		}
		foreach (AttributeData attribute in typeSymbol.GetAttributes())
		{
			IMethodSymbol attributeConstructor = attribute.AttributeConstructor;
			if (attributeConstructor == null)
			{
				continue;
			}
			INamedTypeSymbol containingType = attributeConstructor.ContainingType;
			if (!attributeConstructor.Parameters.Any() && containingType.Name == "FlagsAttribute")
			{
				ISymbol containingSymbol = containingType.ContainingSymbol;
				if (containingSymbol.Kind == SymbolKind.Namespace && containingSymbol.Name == "System" && ((INamespaceSymbol)containingSymbol.ContainingSymbol).IsGlobalNamespace)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void AddFlagsEnumConstantValue(INamedTypeSymbol enumType, object constantValue, bool preferNumericValueOrExpandedFlags)
	{
		ArrayBuilder<EnumField> instance = ArrayBuilder<EnumField>.GetInstance();
		GetSortedEnumFields(enumType, instance);
		ArrayBuilder<EnumField> instance2 = ArrayBuilder<EnumField>.GetInstance();
		try
		{
			AddFlagsEnumConstantValue(enumType, constantValue, instance, instance2, preferNumericValueOrExpandedFlags);
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private void AddFlagsEnumConstantValue(INamedTypeSymbol enumType, object constantValue, ArrayBuilder<EnumField> allFieldsAndValues, ArrayBuilder<EnumField> usedFieldsAndValues, bool preferNumericValueOrExpandedFlags)
	{
		SpecialType specialType = enumType.EnumUnderlyingType.SpecialType;
		ulong num = EnumUtilities.ConvertEnumUnderlyingTypeToUInt64(constantValue, specialType);
		ulong num2 = num;
		if (num2 != 0L)
		{
			foreach (EnumField allFieldsAndValue in allFieldsAndValues)
			{
				ulong value = allFieldsAndValue.Value;
				if ((!preferNumericValueOrExpandedFlags || value != num) && value != 0L && (num2 & value) == value)
				{
					usedFieldsAndValues.Add(allFieldsAndValue);
					num2 -= value;
					if (num2 == 0L)
					{
						break;
					}
				}
			}
		}
		if (num2 == 0L && usedFieldsAndValues.Count > 0)
		{
			for (int num3 = usedFieldsAndValues.Count - 1; num3 >= 0; num3--)
			{
				if (num3 != usedFieldsAndValues.Count - 1)
				{
					AddSpace();
					AddBitwiseOr();
					AddSpace();
				}
				((IFieldSymbol)usedFieldsAndValues[num3].IdentityOpt).Accept(NotFirstVisitor);
			}
		}
		else if (preferNumericValueOrExpandedFlags)
		{
			AddLiteralValue(specialType, constantValue);
		}
		else
		{
			EnumField enumField = ((num == 0L) ? EnumField.FindValue(allFieldsAndValues, 0uL) : default(EnumField));
			if (!enumField.IsDefault)
			{
				((IFieldSymbol)enumField.IdentityOpt).Accept(NotFirstVisitor);
			}
			else
			{
				AddExplicitlyCastedLiteralValue(enumType, specialType, constantValue);
			}
		}
	}

	private static void GetSortedEnumFields(INamedTypeSymbol enumType, ArrayBuilder<EnumField> enumFields)
	{
		SpecialType specialType = enumType.EnumUnderlyingType.SpecialType;
		foreach (ISymbol member in enumType.GetMembers())
		{
			if (member.Kind == SymbolKind.Field)
			{
				IFieldSymbol fieldSymbol = (IFieldSymbol)member;
				if (fieldSymbol.HasConstantValue)
				{
					EnumField item = new EnumField(fieldSymbol.Name, EnumUtilities.ConvertEnumUnderlyingTypeToUInt64(fieldSymbol.ConstantValue, specialType), fieldSymbol);
					enumFields.Add(item);
				}
			}
		}
		enumFields.Sort(EnumField.Comparer);
	}

	private void AddNonFlagsEnumConstantValue(INamedTypeSymbol enumType, object constantValue)
	{
		SpecialType specialType = enumType.EnumUnderlyingType.SpecialType;
		ulong value = EnumUtilities.ConvertEnumUnderlyingTypeToUInt64(constantValue, specialType);
		ArrayBuilder<EnumField> instance = ArrayBuilder<EnumField>.GetInstance();
		GetSortedEnumFields(enumType, instance);
		EnumField enumField = EnumField.FindValue(instance, value);
		if (!enumField.IsDefault)
		{
			((IFieldSymbol)enumField.IdentityOpt).Accept(NotFirstVisitor);
		}
		else
		{
			AddExplicitlyCastedLiteralValue(enumType, specialType, constantValue);
		}
		instance.Free();
	}

	protected abstract bool ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes();

	protected bool NameBoundSuccessfullyToSameSymbol(INamedTypeSymbol symbol)
	{
		ISymbol symbol2 = SingleSymbolWithArity(ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes() ? semanticModelOpt.LookupNamespacesAndTypes(positionOpt, null, symbol.Name) : semanticModelOpt.LookupSymbols(positionOpt, null, symbol.Name), symbol.Arity);
		if (symbol2 == null)
		{
			return false;
		}
		if (symbol2.Equals(symbol.OriginalDefinition))
		{
			return true;
		}
		ISymbol symbol3 = SingleSymbolWithArity(semanticModelOpt.LookupNamespacesAndTypes(positionOpt, null, symbol.Name), symbol.Arity);
		if (symbol3 == null)
		{
			return false;
		}
		ITypeSymbol symbolType = GetSymbolType(symbol2);
		ITypeSymbol symbolType2 = GetSymbolType(symbol3);
		if (symbolType != null && symbolType2 != null && symbolType.Equals(symbolType2))
		{
			return symbol3.Equals(symbol.OriginalDefinition);
		}
		return false;
	}

	private static ISymbol SingleSymbolWithArity(ImmutableArray<ISymbol> candidates, int desiredArity)
	{
		ISymbol symbol = null;
		foreach (ISymbol item in candidates)
		{
			if (item.Kind switch
			{
				SymbolKind.NamedType => ((INamedTypeSymbol)item).Arity, 
				SymbolKind.Method => ((IMethodSymbol)item).Arity, 
				_ => 0, 
			} == desiredArity)
			{
				if (symbol != null)
				{
					symbol = null;
					break;
				}
				symbol = item;
			}
		}
		return symbol;
	}

	protected static ITypeSymbol GetSymbolType(ISymbol symbol)
	{
		if (symbol is ILocalSymbol localSymbol)
		{
			return localSymbol.Type;
		}
		if (symbol is IFieldSymbol fieldSymbol)
		{
			return fieldSymbol.Type;
		}
		if (symbol is IPropertySymbol propertySymbol)
		{
			return propertySymbol.Type;
		}
		if (symbol is IParameterSymbol parameterSymbol)
		{
			return parameterSymbol.Type;
		}
		if (symbol is IAliasSymbol aliasSymbol)
		{
			return aliasSymbol.Target as ITypeSymbol;
		}
		return symbol as ITypeSymbol;
	}
}
