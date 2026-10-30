using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class EmptyStructTypeCache
{
	private SmallDictionary<NamedTypeSymbol, bool> _cache;

	private readonly bool _dev12CompilerCompatibility;

	private readonly SourceAssemblySymbol _sourceAssembly;

	private SmallDictionary<NamedTypeSymbol, bool> Cache => _cache ?? (_cache = new SmallDictionary<NamedTypeSymbol, bool>());

	internal EmptyStructTypeCache(Compilation compilation, bool dev12CompilerCompatibility)
	{
		_dev12CompilerCompatibility = dev12CompilerCompatibility;
		_sourceAssembly = (SourceAssemblySymbol)(compilation?.Assembly);
	}

	public virtual bool IsEmptyStructType(TypeSymbol type)
	{
		return IsEmptyStructType(type, ConsList<NamedTypeSymbol>.Empty);
	}

	private bool IsEmptyStructType(TypeSymbol type, ConsList<NamedTypeSymbol> typesWithMembersOfThisType)
	{
		if (!(type is NamedTypeSymbol namedTypeSymbol) || !IsTrackableStructType(namedTypeSymbol))
		{
			return false;
		}
		if (Cache.TryGetValue(namedTypeSymbol, out var value))
		{
			return value;
		}
		value = CheckStruct(typesWithMembersOfThisType, namedTypeSymbol);
		Cache[namedTypeSymbol] = value;
		return value;
	}

	private bool CheckStruct(ConsList<NamedTypeSymbol> typesWithMembersOfThisType, NamedTypeSymbol nts)
	{
		if (!typesWithMembersOfThisType.ContainsReference(nts))
		{
			typesWithMembersOfThisType = new ConsList<NamedTypeSymbol>(nts, typesWithMembersOfThisType);
			return CheckStructInstanceFields(typesWithMembersOfThisType, nts);
		}
		return true;
	}

	public static bool IsTrackableStructType(TypeSymbol type)
	{
		if ((object)type == null)
		{
			return false;
		}
		if (!(type.OriginalDefinition is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		if (namedTypeSymbol.IsStructType() && namedTypeSymbol.SpecialType == SpecialType.None)
		{
			return !namedTypeSymbol.KnownCircularStruct;
		}
		return false;
	}

	private bool CheckStructInstanceFields(ConsList<NamedTypeSymbol> typesWithMembersOfThisType, NamedTypeSymbol type)
	{
		foreach (Symbol item in type.OriginalDefinition.GetMembersUnordered())
		{
			FieldSymbol actualInstanceField = GetActualInstanceField(item, type);
			if ((object)actualInstanceField != null)
			{
				TypeSymbol type2 = actualInstanceField.Type;
				if (!IsEmptyStructType(type2, typesWithMembersOfThisType))
				{
					return false;
				}
			}
		}
		return true;
	}

	public IEnumerable<FieldSymbol> GetStructInstanceFields(TypeSymbol type)
	{
		if (!(type is NamedTypeSymbol type2))
		{
			return SpecializedCollections.EmptyEnumerable<FieldSymbol>();
		}
		return GetStructInstanceFields(type2);
	}

	public IEnumerable<FieldSymbol> GetStructInstanceFields(NamedTypeSymbol type)
	{
		foreach (Symbol item in type.OriginalDefinition.GetMembersUnordered())
		{
			FieldSymbol actualInstanceField = GetActualInstanceField(item, type);
			if ((object)actualInstanceField != null)
			{
				yield return actualInstanceField;
			}
		}
	}

	private FieldSymbol GetActualInstanceField(Symbol member, NamedTypeSymbol type)
	{
		if (!member.IsStatic)
		{
			switch (member.Kind)
			{
			case SymbolKind.Field:
			{
				FieldSymbol fieldSymbol = (FieldSymbol)member;
				if (fieldSymbol.IsVirtualTupleField)
				{
					return null;
				}
				if (!fieldSymbol.IsFixed && !ShouldIgnoreStructField(fieldSymbol, fieldSymbol.Type))
				{
					return fieldSymbol.AsMember(type);
				}
				return null;
			}
			case SymbolKind.Event:
			{
				EventSymbol eventSymbol = (EventSymbol)member;
				if (eventSymbol.HasAssociatedField && !ShouldIgnoreStructField(eventSymbol, eventSymbol.Type))
				{
					return eventSymbol.AssociatedField.AsMember(type);
				}
				return null;
			}
			}
		}
		return null;
	}

	private bool ShouldIgnoreStructField(Symbol member, TypeSymbol memberType)
	{
		if (_dev12CompilerCompatibility && ((object)member.ContainingAssembly != _sourceAssembly || member.ContainingModule.Ordinal != 0) && IsIgnorableType(memberType))
		{
			return !IsAccessibleInAssembly(member, _sourceAssembly);
		}
		return false;
	}

	private static bool IsIgnorableType(TypeSymbol type)
	{
		while (true)
		{
			switch (type.TypeKind)
			{
			case TypeKind.Enum:
			case TypeKind.Struct:
			case TypeKind.TypeParameter:
				return false;
			case TypeKind.Array:
				break;
			default:
				return true;
			}
			type = ((ArrayTypeSymbol)type).BaseTypeNoUseSiteDiagnostics;
		}
	}

	private static bool IsAccessibleInAssembly(Symbol symbol, SourceAssemblySymbol assembly)
	{
		while (symbol != null && symbol.Kind != SymbolKind.Namespace)
		{
			switch (symbol.DeclaredAccessibility)
			{
			case Accessibility.ProtectedAndInternal:
			case Accessibility.Internal:
				if (!assembly.HasInternalAccessTo(symbol.ContainingAssembly))
				{
					return false;
				}
				break;
			case Accessibility.Private:
				return false;
			}
			symbol = symbol.ContainingSymbol;
		}
		return true;
	}
}
