using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class AccessCheck
{
	public static bool IsSymbolAccessible(Symbol symbol, AssemblySymbol within, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool failedThroughTypeCheck;
		return IsSymbolAccessibleCore(symbol, within, null, out failedThroughTypeCheck, within.DeclaringCompilation, ref useSiteDiagnostics);
	}

	public static bool IsSymbolAccessible(Symbol symbol, NamedTypeSymbol within, ref HashSet<DiagnosticInfo> useSiteDiagnostics, TypeSymbol throughTypeOpt = null)
	{
		bool failedThroughTypeCheck;
		return IsSymbolAccessibleCore(symbol, within, throughTypeOpt, out failedThroughTypeCheck, within.DeclaringCompilation, ref useSiteDiagnostics);
	}

	public static bool IsSymbolAccessible(Symbol symbol, NamedTypeSymbol within, TypeSymbol throughTypeOpt, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		return IsSymbolAccessibleCore(symbol, within, throughTypeOpt, out failedThroughTypeCheck, within.DeclaringCompilation, ref useSiteDiagnostics, basesBeingResolved);
	}

	private static bool IsSymbolAccessibleCore(Symbol symbol, Symbol within, TypeSymbol throughTypeOpt, out bool failedThroughTypeCheck, CSharpCompilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		failedThroughTypeCheck = false;
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
			return IsSymbolAccessibleCore(((ArrayTypeSymbol)symbol).ElementType, within, null, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics, basesBeingResolved);
		case SymbolKind.PointerType:
			return IsSymbolAccessibleCore(((PointerTypeSymbol)symbol).PointedAtType, within, null, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics, basesBeingResolved);
		case SymbolKind.NamedType:
			return IsNamedTypeAccessible((NamedTypeSymbol)symbol, within, ref useSiteDiagnostics, basesBeingResolved);
		case SymbolKind.ErrorType:
			return true;
		case SymbolKind.Assembly:
		case SymbolKind.DynamicType:
		case SymbolKind.Label:
		case SymbolKind.Local:
		case SymbolKind.NetModule:
		case SymbolKind.Namespace:
		case SymbolKind.Parameter:
		case SymbolKind.RangeVariable:
		case SymbolKind.TypeParameter:
			return true;
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Method:
		case SymbolKind.Property:
			if (symbol.IsStatic)
			{
				throughTypeOpt = null;
			}
			return IsMemberAccessible(symbol.ContainingType, symbol.DeclaredAccessibility, within, throughTypeOpt, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	private static bool IsNamedTypeAccessible(NamedTypeSymbol type, Symbol within, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		CSharpCompilation declaringCompilation = within.DeclaringCompilation;
		bool failedThroughTypeCheck;
		if (!type.IsDefinition)
		{
			foreach (TypeSymbol item in type.TypeArgumentsWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
			{
				if (item.Kind != SymbolKind.TypeParameter && !IsSymbolAccessibleCore(item, within, null, out failedThroughTypeCheck, declaringCompilation, ref useSiteDiagnostics, basesBeingResolved))
				{
					return false;
				}
			}
		}
		NamedTypeSymbol containingType = type.ContainingType;
		if ((object)containingType != null)
		{
			return IsMemberAccessible(containingType, type.DeclaredAccessibility, within, null, out failedThroughTypeCheck, declaringCompilation, ref useSiteDiagnostics, basesBeingResolved);
		}
		return IsNonNestedTypeAccessible(type.ContainingAssembly, type.DeclaredAccessibility, within);
	}

	private static bool IsNonNestedTypeAccessible(AssemblySymbol assembly, Accessibility declaredAccessibility, Symbol within)
	{
		switch (declaredAccessibility)
		{
		case Accessibility.NotApplicable:
		case Accessibility.Public:
			return true;
		case Accessibility.Private:
		case Accessibility.ProtectedAndInternal:
		case Accessibility.Protected:
			return false;
		case Accessibility.Internal:
		case Accessibility.ProtectedOrInternal:
		{
			AssemblySymbol assemblySymbol = ((within is NamedTypeSymbol namedTypeSymbol) ? namedTypeSymbol.ContainingAssembly : ((AssemblySymbol)within));
			if ((object)assemblySymbol != assembly)
			{
				return assemblySymbol.HasInternalAccessTo(assembly);
			}
			return true;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(declaredAccessibility);
		}
	}

	private static bool IsMemberAccessible(NamedTypeSymbol containingType, Accessibility declaredAccessibility, Symbol within, TypeSymbol throughTypeOpt, out bool failedThroughTypeCheck, CSharpCompilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		failedThroughTypeCheck = false;
		if (containingType.IsTupleType)
		{
			containingType = containingType.TupleUnderlyingType;
		}
		if ((object)containingType == within)
		{
			return true;
		}
		if (!IsNamedTypeAccessible(containingType, within, ref useSiteDiagnostics, basesBeingResolved))
		{
			return false;
		}
		if (declaredAccessibility == Accessibility.Public)
		{
			return true;
		}
		return IsNonPublicMemberAccessible(containingType, declaredAccessibility, within, throughTypeOpt, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics, basesBeingResolved);
	}

	private static bool IsNonPublicMemberAccessible(NamedTypeSymbol containingType, Accessibility declaredAccessibility, Symbol within, TypeSymbol throughTypeOpt, out bool failedThroughTypeCheck, CSharpCompilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		failedThroughTypeCheck = false;
		NamedTypeSymbol originalDefinition = containingType.OriginalDefinition;
		NamedTypeSymbol namedTypeSymbol = within as NamedTypeSymbol;
		AssemblySymbol assembly = (((object)namedTypeSymbol != null) ? namedTypeSymbol.ContainingAssembly : ((AssemblySymbol)within));
		switch (declaredAccessibility)
		{
		case Accessibility.NotApplicable:
			return true;
		case Accessibility.Private:
			if (containingType.TypeKind == TypeKind.Submission)
			{
				return true;
			}
			if ((object)namedTypeSymbol != null)
			{
				return IsPrivateSymbolAccessible(namedTypeSymbol, originalDefinition);
			}
			return false;
		case Accessibility.Internal:
			return assembly.HasInternalAccessTo(containingType.ContainingAssembly);
		case Accessibility.ProtectedAndInternal:
			if (!assembly.HasInternalAccessTo(containingType.ContainingAssembly))
			{
				return false;
			}
			return IsProtectedSymbolAccessible(namedTypeSymbol, throughTypeOpt, originalDefinition, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics, basesBeingResolved);
		case Accessibility.ProtectedOrInternal:
			if (assembly.HasInternalAccessTo(containingType.ContainingAssembly))
			{
				return true;
			}
			return IsProtectedSymbolAccessible(namedTypeSymbol, throughTypeOpt, originalDefinition, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics, basesBeingResolved);
		case Accessibility.Protected:
			return IsProtectedSymbolAccessible(namedTypeSymbol, throughTypeOpt, originalDefinition, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics, basesBeingResolved);
		default:
			throw ExceptionUtilities.UnexpectedValue(declaredAccessibility);
		}
	}

	private static bool IsProtectedSymbolAccessible(NamedTypeSymbol withinType, TypeSymbol throughTypeOpt, NamedTypeSymbol originalContainingType, out bool failedThroughTypeCheck, CSharpCompilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		failedThroughTypeCheck = false;
		if (originalContainingType.TypeKind == TypeKind.Submission)
		{
			return true;
		}
		if ((object)withinType == null)
		{
			return false;
		}
		if (IsNestedWithinOriginalContainingType(withinType, originalContainingType))
		{
			return true;
		}
		NamedTypeSymbol namedTypeSymbol = withinType.OriginalDefinition;
		TypeSymbol typeSymbol = throughTypeOpt?.OriginalDefinition;
		while ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol.InheritsFromIgnoringConstruction(originalContainingType, compilation, ref useSiteDiagnostics, basesBeingResolved))
			{
				if ((object)typeSymbol == null || typeSymbol.InheritsFromIgnoringConstruction(namedTypeSymbol, compilation, ref useSiteDiagnostics))
				{
					return true;
				}
				failedThroughTypeCheck = true;
			}
			namedTypeSymbol = namedTypeSymbol.ContainingType;
		}
		return false;
	}

	private static bool IsPrivateSymbolAccessible(Symbol within, NamedTypeSymbol originalContainingType)
	{
		if (!(within is NamedTypeSymbol withinType))
		{
			return false;
		}
		return IsNestedWithinOriginalContainingType(withinType, originalContainingType);
	}

	private static bool IsNestedWithinOriginalContainingType(NamedTypeSymbol withinType, NamedTypeSymbol originalContainingType)
	{
		NamedTypeSymbol namedTypeSymbol = withinType.OriginalDefinition;
		while ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol.Equals(originalContainingType))
			{
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.ContainingType;
		}
		return false;
	}

	private static bool InheritsFromIgnoringConstruction(this TypeSymbol type, NamedTypeSymbol baseType, CSharpCompilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ConsList<Symbol> basesBeingResolved = null)
	{
		PooledHashSet<NamedTypeSymbol> visited = null;
		TypeSymbol typeSymbol = type;
		bool result = false;
		while ((object)typeSymbol != null)
		{
			if (typeSymbol.Equals(baseType))
			{
				result = true;
				break;
			}
			TypeSymbol nextBaseTypeNoUseSiteDiagnostics = typeSymbol.GetNextBaseTypeNoUseSiteDiagnostics(basesBeingResolved, compilation, ref visited);
			if ((object)nextBaseTypeNoUseSiteDiagnostics == null)
			{
				typeSymbol = null;
				continue;
			}
			typeSymbol = nextBaseTypeNoUseSiteDiagnostics.OriginalDefinition;
			typeSymbol.AddUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		visited?.Free();
		return result;
	}

	internal static bool HasInternalAccessTo(this AssemblySymbol assembly, AssemblySymbol toAssembly)
	{
		if (object.Equals(assembly, toAssembly))
		{
			return true;
		}
		if (assembly.AreInternalsVisibleToThisAssembly(toAssembly))
		{
			return true;
		}
		if (assembly.IsInteractive && toAssembly.IsInteractive)
		{
			return true;
		}
		return false;
	}

	internal static ErrorCode GetProtectedMemberInSealedTypeError(NamedTypeSymbol containingType)
	{
		if (containingType.TypeKind != TypeKind.Struct)
		{
			return ErrorCode.WRN_ProtectedInSealed;
		}
		return ErrorCode.ERR_ProtectedInStruct;
	}
}
