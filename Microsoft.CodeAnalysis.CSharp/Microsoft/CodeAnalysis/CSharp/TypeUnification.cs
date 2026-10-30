using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class TypeUnification
{
	public static bool CanUnify(TypeSymbol t1, TypeSymbol t2)
	{
		if (t1 == t2)
		{
			return true;
		}
		MutableTypeMap substitution = null;
		return CanUnifyHelper(((object)t1 == null) ? default(TypeWithModifiers) : new TypeWithModifiers(t1), ((object)t2 == null) ? default(TypeWithModifiers) : new TypeWithModifiers(t2), ref substitution);
	}

	private static bool CanUnifyHelper(TypeWithModifiers t1, TypeWithModifiers t2, ref MutableTypeMap substitution)
	{
		if (t1 == t2)
		{
			return true;
		}
		if ((object)t1.Type == null || (object)t2.Type == null)
		{
			return false;
		}
		if (substitution != null)
		{
			t1 = t1.SubstituteType(substitution);
			t2 = t2.SubstituteType(substitution);
		}
		if (t1 == t2)
		{
			return true;
		}
		if (!t1.Type.IsTypeParameter() && t2.Type.IsTypeParameter())
		{
			TypeWithModifiers typeWithModifiers = t1;
			t1 = t2;
			t2 = typeWithModifiers;
		}
		switch (t1.Type.Kind)
		{
		case SymbolKind.ArrayType:
		{
			if (t2.Type.TypeKind != t1.Type.TypeKind || !t2.CustomModifiers.SequenceEqual(t1.CustomModifiers))
			{
				return false;
			}
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)t1.Type;
			ArrayTypeSymbol arrayTypeSymbol2 = (ArrayTypeSymbol)t2.Type;
			if (!arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2))
			{
				return false;
			}
			return CanUnifyHelper(new TypeWithModifiers(arrayTypeSymbol.ElementType, arrayTypeSymbol.CustomModifiers), new TypeWithModifiers(arrayTypeSymbol2.ElementType, arrayTypeSymbol2.CustomModifiers), ref substitution);
		}
		case SymbolKind.PointerType:
		{
			if (t2.Type.TypeKind != t1.Type.TypeKind || !t2.CustomModifiers.SequenceEqual(t1.CustomModifiers))
			{
				return false;
			}
			PointerTypeSymbol pointerTypeSymbol = (PointerTypeSymbol)t1.Type;
			PointerTypeSymbol pointerTypeSymbol2 = (PointerTypeSymbol)t2.Type;
			return CanUnifyHelper(new TypeWithModifiers(pointerTypeSymbol.PointedAtType, pointerTypeSymbol.CustomModifiers), new TypeWithModifiers(pointerTypeSymbol2.PointedAtType, pointerTypeSymbol2.CustomModifiers), ref substitution);
		}
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		{
			if (t2.Type.TypeKind != t1.Type.TypeKind || !t2.CustomModifiers.SequenceEqual(t1.CustomModifiers))
			{
				return false;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)t1.Type;
			NamedTypeSymbol namedTypeSymbol2 = (NamedTypeSymbol)t2.Type;
			if (namedTypeSymbol.IsTupleType)
			{
				if (!namedTypeSymbol2.IsTupleType)
				{
					return false;
				}
				return CanUnifyHelper(new TypeWithModifiers(namedTypeSymbol.TupleUnderlyingType), new TypeWithModifiers(namedTypeSymbol2.TupleUnderlyingType), ref substitution);
			}
			if (!namedTypeSymbol.IsGenericType)
			{
				if (!namedTypeSymbol2.IsGenericType)
				{
					return namedTypeSymbol == namedTypeSymbol2;
				}
				return false;
			}
			if (!namedTypeSymbol2.IsGenericType)
			{
				return false;
			}
			int arity = namedTypeSymbol.Arity;
			if (namedTypeSymbol2.Arity != arity || namedTypeSymbol2.OriginalDefinition != namedTypeSymbol.OriginalDefinition)
			{
				return false;
			}
			ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics = namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics;
			ImmutableArray<TypeSymbol> typeArgumentsNoUseSiteDiagnostics2 = namedTypeSymbol2.TypeArgumentsNoUseSiteDiagnostics;
			bool hasTypeArgumentsCustomModifiers = namedTypeSymbol.HasTypeArgumentsCustomModifiers;
			bool hasTypeArgumentsCustomModifiers2 = namedTypeSymbol2.HasTypeArgumentsCustomModifiers;
			for (int i = 0; i < arity; i++)
			{
				if (!CanUnifyHelper(new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics[i], hasTypeArgumentsCustomModifiers ? namedTypeSymbol.GetTypeArgumentCustomModifiers(i) : default(ImmutableArray<CustomModifier>)), new TypeWithModifiers(typeArgumentsNoUseSiteDiagnostics2[i], hasTypeArgumentsCustomModifiers2 ? namedTypeSymbol2.GetTypeArgumentCustomModifiers(i) : default(ImmutableArray<CustomModifier>)), ref substitution))
				{
					return false;
				}
			}
			if ((object)namedTypeSymbol.ContainingType != null)
			{
				return CanUnifyHelper(new TypeWithModifiers(namedTypeSymbol.ContainingType), new TypeWithModifiers(namedTypeSymbol2.ContainingType), ref substitution);
			}
			return true;
		}
		case SymbolKind.TypeParameter:
		{
			if (t2.Type.TypeKind == TypeKind.Pointer || t2.Type.SpecialType == SpecialType.System_Void)
			{
				return false;
			}
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)t1.Type;
			if (Contains(t2.Type, typeParameterSymbol))
			{
				return false;
			}
			if (t1.CustomModifiers.IsDefaultOrEmpty)
			{
				AddSubstitution(ref substitution, typeParameterSymbol, t2);
				return true;
			}
			if (t1.CustomModifiers.SequenceEqual(t2.CustomModifiers))
			{
				AddSubstitution(ref substitution, typeParameterSymbol, new TypeWithModifiers(t2.Type));
				return true;
			}
			if (t1.CustomModifiers.Length < t2.CustomModifiers.Length && t1.CustomModifiers.SequenceEqual(t2.CustomModifiers.Take(t1.CustomModifiers.Length)))
			{
				AddSubstitution(ref substitution, typeParameterSymbol, new TypeWithModifiers(t2.Type, ImmutableArray.Create(t2.CustomModifiers, t1.CustomModifiers.Length, t2.CustomModifiers.Length - t1.CustomModifiers.Length)));
				return true;
			}
			if (t2.Type.IsTypeParameter())
			{
				TypeParameterSymbol tp = (TypeParameterSymbol)t2.Type;
				if (t2.CustomModifiers.IsDefaultOrEmpty)
				{
					AddSubstitution(ref substitution, tp, t1);
					return true;
				}
				if (t2.CustomModifiers.Length < t1.CustomModifiers.Length && t2.CustomModifiers.SequenceEqual(t1.CustomModifiers.Take(t2.CustomModifiers.Length)))
				{
					AddSubstitution(ref substitution, tp, new TypeWithModifiers(t1.Type, ImmutableArray.Create(t1.CustomModifiers, t2.CustomModifiers.Length, t1.CustomModifiers.Length - t2.CustomModifiers.Length)));
					return true;
				}
			}
			return false;
		}
		default:
			return t1 == t2;
		}
	}

	private static void AddSubstitution(ref MutableTypeMap substitution, TypeParameterSymbol tp1, TypeWithModifiers t2)
	{
		if (substitution == null)
		{
			substitution = new MutableTypeMap();
		}
		substitution.Add(tp1, t2);
	}

	private static bool Contains(TypeSymbol type, TypeParameterSymbol typeParam)
	{
		switch (type.Kind)
		{
		case SymbolKind.ArrayType:
			return Contains(((ArrayTypeSymbol)type).ElementType, typeParam);
		case SymbolKind.PointerType:
			return Contains(((PointerTypeSymbol)type).PointedAtType, typeParam);
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			while ((object)namedTypeSymbol != null)
			{
				foreach (TypeSymbol item in namedTypeSymbol.IsTupleType ? namedTypeSymbol.TupleElementTypes : namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics)
				{
					if (Contains(item, typeParam))
					{
						return true;
					}
				}
				namedTypeSymbol = namedTypeSymbol.ContainingType;
			}
			return false;
		}
		case SymbolKind.TypeParameter:
			return type == typeParam;
		default:
			return false;
		}
	}
}
