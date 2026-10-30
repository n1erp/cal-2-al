using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BestTypeInferrer
{
	public static TypeSymbol InferBestType(ImmutableArray<TypeSymbol> types, Conversions conversions, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return GetBestType(types, conversions, ref useSiteDiagnostics);
	}

	public static TypeSymbol InferBestType(ImmutableArray<BoundExpression> exprs, Conversions conversions, out bool hadMultipleCandidates, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		HashSet<TypeSymbol> hashSet = new HashSet<TypeSymbol>();
		foreach (BoundExpression item in exprs)
		{
			TypeSymbol type = item.Type;
			if ((object)type != null)
			{
				if (type.IsErrorType())
				{
					hadMultipleCandidates = false;
					return type;
				}
				hashSet.Add(type);
			}
		}
		hadMultipleCandidates = hashSet.Count > 1;
		return InferBestType(hashSet.AsImmutableOrEmpty(), conversions, ref useSiteDiagnostics);
	}

	public static TypeSymbol InferBestTypeForConditionalOperator(BoundExpression expr1, BoundExpression expr2, Conversions conversions, out bool hadMultipleCandidates, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		TypeSymbol type = expr1.Type;
		if ((object)type != null)
		{
			if (type.IsErrorType())
			{
				instance.Free();
				hadMultipleCandidates = false;
				return type;
			}
			if (conversions.ClassifyImplicitConversionFromExpression(expr2, type, ref useSiteDiagnostics).Exists)
			{
				instance.Add(type);
			}
		}
		TypeSymbol type2 = expr2.Type;
		if ((object)type2 != null && type2 != type)
		{
			if (type2.IsErrorType())
			{
				instance.Free();
				hadMultipleCandidates = false;
				return type2;
			}
			if (conversions.ClassifyImplicitConversionFromExpression(expr1, type2, ref useSiteDiagnostics).Exists)
			{
				instance.Add(type2);
			}
		}
		hadMultipleCandidates = instance.Count > 1;
		return InferBestType(instance.ToImmutableAndFree(), conversions, ref useSiteDiagnostics);
	}

	private static TypeSymbol GetBestType(ImmutableArray<TypeSymbol> types, Conversions conversions, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (types.IsEmpty)
		{
			return null;
		}
		if (types.Length == 1)
		{
			return types[0];
		}
		TypeSymbol typeSymbol = null;
		int num = -1;
		for (int i = 0; i < types.Length; i++)
		{
			TypeSymbol typeSymbol2 = types[i];
			if ((object)typeSymbol == null)
			{
				typeSymbol = typeSymbol2;
				num = i;
				continue;
			}
			TypeSymbol typeSymbol3 = Better(typeSymbol, typeSymbol2, conversions, ref useSiteDiagnostics);
			if ((object)typeSymbol3 == null)
			{
				typeSymbol = null;
			}
			else if ((object)typeSymbol3 != typeSymbol)
			{
				typeSymbol = typeSymbol3;
				num = i;
			}
		}
		if ((object)typeSymbol == null)
		{
			return null;
		}
		for (int j = 0; j < num; j++)
		{
			TypeSymbol type = types[j];
			if (Better(typeSymbol, type, conversions, ref useSiteDiagnostics) != typeSymbol)
			{
				return null;
			}
		}
		return typeSymbol;
	}

	private static TypeSymbol Better(TypeSymbol type1, TypeSymbol type2, Conversions conversions, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (type1.IsErrorType())
		{
			return type2;
		}
		if ((object)type2 == null || type2.IsErrorType())
		{
			return type1;
		}
		bool exists = conversions.ClassifyImplicitConversionFromType(type1, type2, ref useSiteDiagnostics).Exists;
		bool exists2 = conversions.ClassifyImplicitConversionFromType(type2, type1, ref useSiteDiagnostics).Exists;
		if (exists & exists2)
		{
			if (type1.IsDynamic())
			{
				return type1;
			}
			if (type2.IsDynamic())
			{
				return type2;
			}
			if (type1.Equals(type2, TypeCompareKind.IgnoreDynamicAndTupleNames))
			{
				return MethodTypeInferrer.MergeTupleNames(MethodTypeInferrer.MergeDynamic(type1, type2, conversions.CorLibrary), type2);
			}
			return null;
		}
		if (exists)
		{
			return type2;
		}
		if (exists2)
		{
			return type1;
		}
		return null;
	}
}
