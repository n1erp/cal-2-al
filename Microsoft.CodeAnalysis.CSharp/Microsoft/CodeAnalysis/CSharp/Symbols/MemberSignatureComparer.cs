using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class MemberSignatureComparer : IEqualityComparer<Symbol>
{
	public static readonly MemberSignatureComparer ExplicitImplementationComparer = new MemberSignatureComparer(considerName: false, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: true, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer CSharpImplicitImplementationComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: true, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: true, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer CSharpCloseImplicitImplementationComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: true, considerReturnType: false, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer DuplicateSourceComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: true, considerReturnType: false, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: false, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer PartialMethodsComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: true, considerReturnType: false, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer CSharpOverrideComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: false, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer CSharpWithTupleNamesComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: false, considerCustomModifiers: false, ignoreDynamic: true, ignoreTupleNames: false);

	public static readonly MemberSignatureComparer CSharpWithoutTupleNamesComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: false, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer CSharpAccessorOverrideComparer = new MemberSignatureComparer(considerName: false, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer CSharpCustomModifierOverrideComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: true, considerCustomModifiers: true);

	internal static readonly MemberSignatureComparer SloppyOverrideComparer = new MemberSignatureComparer(considerName: false, considerExplicitlyImplementedInterfaces: false, considerReturnType: false, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: false, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer RuntimeSignatureComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: true, considerRefKindDifferences: false, considerCustomModifiers: true);

	public static readonly MemberSignatureComparer RuntimePlusRefOutSignatureComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: true, considerRefKindDifferences: true, considerCustomModifiers: true);

	public static readonly MemberSignatureComparer RuntimeImplicitImplementationComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: true, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: true, considerRefKindDifferences: false, considerCustomModifiers: true);

	public static readonly MemberSignatureComparer CSharpSignatureAndConstraintsAndReturnTypeComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: true, considerReturnType: true, considerTypeConstraints: true, considerCallingConvention: true, considerRefKindDifferences: true, considerCustomModifiers: false);

	public static readonly MemberSignatureComparer RetargetedExplicitImplementationComparer = new MemberSignatureComparer(considerName: true, considerExplicitlyImplementedInterfaces: false, considerReturnType: true, considerTypeConstraints: false, considerCallingConvention: true, considerRefKindDifferences: true, considerCustomModifiers: true);

	public static readonly MemberSignatureComparer CrefComparer = new MemberSignatureComparer(considerName: false, considerExplicitlyImplementedInterfaces: false, considerReturnType: false, considerTypeConstraints: false, considerCallingConvention: false, considerRefKindDifferences: true, considerCustomModifiers: false);

	private readonly bool _considerName;

	private readonly bool _considerExplicitlyImplementedInterfaces;

	private readonly bool _considerReturnType;

	private readonly bool _considerTypeConstraints;

	private readonly bool _considerCallingConvention;

	private readonly bool _considerRefKindDifferences;

	private readonly bool _considerCustomModifiers;

	private readonly bool _ignoreDynamic;

	private readonly bool _ignoreTupleNames;

	private MemberSignatureComparer(bool considerName, bool considerExplicitlyImplementedInterfaces, bool considerReturnType, bool considerTypeConstraints, bool considerCallingConvention, bool considerRefKindDifferences, bool considerCustomModifiers, bool ignoreDynamic = true, bool ignoreTupleNames = true)
	{
		_considerName = considerName;
		_considerExplicitlyImplementedInterfaces = considerExplicitlyImplementedInterfaces;
		_considerReturnType = considerReturnType;
		_considerTypeConstraints = considerTypeConstraints;
		_considerCallingConvention = considerCallingConvention;
		_considerRefKindDifferences = considerRefKindDifferences;
		_considerCustomModifiers = considerCustomModifiers;
		_ignoreDynamic = ignoreDynamic;
		_ignoreTupleNames = ignoreTupleNames;
	}

	public bool Equals(Symbol member1, Symbol member2)
	{
		if ((object)member1 == member2)
		{
			return true;
		}
		if ((object)member1 == null || (object)member2 == null || member1.Kind != member2.Kind)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		if (_considerName)
		{
			string memberNameWithoutInterfaceName = ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(member1.Name);
			string memberNameWithoutInterfaceName2 = ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(member2.Name);
			flag = memberNameWithoutInterfaceName != member1.Name;
			flag2 = memberNameWithoutInterfaceName2 != member2.Name;
			if (memberNameWithoutInterfaceName != memberNameWithoutInterfaceName2)
			{
				return false;
			}
		}
		if (member1.GetMemberArity() != member2.GetMemberArity() || member1.GetParameterCount() != member2.GetParameterCount())
		{
			return false;
		}
		TypeMap typeMap = GetTypeMap(member1);
		TypeMap typeMap2 = GetTypeMap(member2);
		if (_considerReturnType && !HaveSameReturnTypes(member1, typeMap, member2, typeMap2, _considerCustomModifiers, _ignoreDynamic, _ignoreTupleNames))
		{
			return false;
		}
		if (member1.GetParameterCount() > 0 && !HaveSameParameterTypes(member1.GetParameters(), typeMap, member2.GetParameters(), typeMap2, _considerRefKindDifferences, _considerCustomModifiers, _ignoreDynamic, _ignoreTupleNames))
		{
			return false;
		}
		if (_considerCallingConvention)
		{
			if (GetCallingConvention(member1) != GetCallingConvention(member2))
			{
				return false;
			}
		}
		else if (IsVarargMethod(member1) != IsVarargMethod(member2))
		{
			return false;
		}
		if (_considerExplicitlyImplementedInterfaces)
		{
			if (flag != flag2)
			{
				return false;
			}
			if (flag)
			{
				if (member1.IsExplicitInterfaceImplementation() != member2.IsExplicitInterfaceImplementation())
				{
					return false;
				}
				ImmutableArray<Symbol> explicitInterfaceImplementations = member1.GetExplicitInterfaceImplementations();
				ImmutableArray<Symbol> explicitInterfaceImplementations2 = member2.GetExplicitInterfaceImplementations();
				if (!explicitInterfaceImplementations.SetEquals(explicitInterfaceImplementations2, EqualityComparer<Symbol>.Default))
				{
					return false;
				}
			}
		}
		if (_considerTypeConstraints)
		{
			return HaveSameConstraints(member1, typeMap, member2, typeMap2);
		}
		return true;
	}

	public int GetHashCode(Symbol member)
	{
		int num = 1;
		if ((object)member != null)
		{
			num = Hash.Combine((int)member.Kind, num);
			if (_considerName)
			{
				num = Hash.Combine(ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(member.Name), num);
			}
			if (_considerReturnType)
			{
				member.GetTypeOrReturnType(out var refKind, out var returnType, out ImmutableArray<CustomModifier> returnTypeCustomModifiers, out returnTypeCustomModifiers);
				num = Hash.Combine((int)refKind, num);
				if (member.GetMemberArity() == 0 && !_considerCustomModifiers)
				{
					num = Hash.Combine(returnType, num);
				}
			}
			num = Hash.Combine(member.GetMemberArity(), num);
			num = Hash.Combine(member.GetParameterCount(), num);
		}
		return num;
	}

	public static bool HaveSameReturnTypes(MethodSymbol member1, MethodSymbol member2, bool considerCustomModifiers)
	{
		return HaveSameReturnTypes(member1, GetTypeMap(member1), member2, GetTypeMap(member2), considerCustomModifiers, ignoreDynamic: true, ignoreTupleNames: true);
	}

	private static bool HaveSameReturnTypes(Symbol member1, TypeMap typeMap1, Symbol member2, TypeMap typeMap2, bool considerCustomModifiers, bool ignoreDynamic, bool ignoreTupleNames)
	{
		member1.GetTypeOrReturnType(out var refKind, out var returnType, out var returnTypeCustomModifiers, out var refCustomModifiers);
		member2.GetTypeOrReturnType(out var refKind2, out var returnType2, out var returnTypeCustomModifiers2, out var refCustomModifiers2);
		if (refKind != refKind2)
		{
			return false;
		}
		bool flag = returnType.SpecialType == SpecialType.System_Void;
		bool flag2 = returnType2.SpecialType == SpecialType.System_Void;
		if (flag != flag2)
		{
			return false;
		}
		TypeWithModifiers typeWithModifiers = SubstituteType(typeMap1, new TypeWithModifiers(returnType, returnTypeCustomModifiers));
		TypeWithModifiers other = SubstituteType(typeMap2, new TypeWithModifiers(returnType2, returnTypeCustomModifiers2));
		if (!considerCustomModifiers)
		{
			return typeWithModifiers.Type.Equals(other.Type, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds.AddIgnoreDynamic(ignoreDynamic).AddIgnoreTupleNames(ignoreTupleNames));
		}
		if (typeWithModifiers.Equals(other, TypeCompareKind.ConsiderEverything.AddIgnoreDynamic(ignoreDynamic).AddIgnoreTupleNames(ignoreTupleNames)))
		{
			return SubstituteModifiers(typeMap1, refCustomModifiers).SequenceEqual(SubstituteModifiers(typeMap2, refCustomModifiers2));
		}
		return false;
	}

	private static TypeMap GetTypeMap(Symbol member)
	{
		ImmutableArray<TypeParameterSymbol> memberTypeParameters = member.GetMemberTypeParameters();
		if (!memberTypeParameters.IsEmpty)
		{
			return new TypeMap(memberTypeParameters, IndexedTypeParameterSymbol.Take(member.GetMemberArity()), allowAlpha: true);
		}
		return null;
	}

	private static bool HaveSameConstraints(Symbol member1, TypeMap typeMap1, Symbol member2, TypeMap typeMap2)
	{
		if (member1.GetMemberArity() == 0)
		{
			return true;
		}
		ImmutableArray<TypeParameterSymbol> memberTypeParameters = member1.GetMemberTypeParameters();
		ImmutableArray<TypeParameterSymbol> memberTypeParameters2 = member2.GetMemberTypeParameters();
		return HaveSameConstraints(memberTypeParameters, typeMap1, memberTypeParameters2, typeMap2);
	}

	public static bool HaveSameConstraints(ImmutableArray<TypeParameterSymbol> typeParameters1, TypeMap typeMap1, ImmutableArray<TypeParameterSymbol> typeParameters2, TypeMap typeMap2)
	{
		int length = typeParameters1.Length;
		for (int i = 0; i < length; i++)
		{
			if (!HaveSameConstraints(typeParameters1[i], typeMap1, typeParameters2[i], typeMap2))
			{
				return false;
			}
		}
		return true;
	}

	public static bool HaveSameConstraints(TypeParameterSymbol typeParameter1, TypeMap typeMap1, TypeParameterSymbol typeParameter2, TypeMap typeMap2)
	{
		if (typeParameter1.HasConstructorConstraint != typeParameter2.HasConstructorConstraint || typeParameter1.HasReferenceTypeConstraint != typeParameter2.HasReferenceTypeConstraint || typeParameter1.HasValueTypeConstraint != typeParameter2.HasValueTypeConstraint || typeParameter1.HasUnmanagedTypeConstraint != typeParameter2.HasUnmanagedTypeConstraint || typeParameter1.Variance != typeParameter2.Variance)
		{
			return false;
		}
		ImmutableArray<TypeSymbol> constraintTypesNoUseSiteDiagnostics = typeParameter1.ConstraintTypesNoUseSiteDiagnostics;
		ImmutableArray<TypeSymbol> constraintTypesNoUseSiteDiagnostics2 = typeParameter2.ConstraintTypesNoUseSiteDiagnostics;
		if (constraintTypesNoUseSiteDiagnostics.Length == 0 && constraintTypesNoUseSiteDiagnostics2.Length == 0)
		{
			return true;
		}
		HashSet<TypeSymbol> hashSet = new HashSet<TypeSymbol>(TypeSymbol.EqualsIgnoringDynamicAndTupleNamesComparer);
		HashSet<TypeSymbol> hashSet2 = new HashSet<TypeSymbol>(TypeSymbol.EqualsIgnoringDynamicAndTupleNamesComparer);
		SubstituteConstraintTypes(constraintTypesNoUseSiteDiagnostics, typeMap1, hashSet);
		SubstituteConstraintTypes(constraintTypesNoUseSiteDiagnostics2, typeMap2, hashSet2);
		if (AreConstraintTypesSubset(hashSet, hashSet2, typeParameter2))
		{
			return AreConstraintTypesSubset(hashSet2, hashSet, typeParameter1);
		}
		return false;
	}

	private static bool AreConstraintTypesSubset(HashSet<TypeSymbol> constraintTypes1, HashSet<TypeSymbol> constraintTypes2, TypeParameterSymbol typeParameter2)
	{
		foreach (TypeSymbol item in constraintTypes1)
		{
			if (item.SpecialType != SpecialType.System_Object && !constraintTypes2.Contains(item) && (item.SpecialType != SpecialType.System_ValueType || !typeParameter2.HasValueTypeConstraint))
			{
				return false;
			}
		}
		return true;
	}

	private static void SubstituteConstraintTypes(ImmutableArray<TypeSymbol> types, TypeMap typeMap, HashSet<TypeSymbol> result)
	{
		foreach (TypeSymbol item in types)
		{
			result.Add(typeMap.SubstituteType(item).Type);
		}
	}

	private static bool HaveSameParameterTypes(ImmutableArray<ParameterSymbol> params1, TypeMap typeMap1, ImmutableArray<ParameterSymbol> params2, TypeMap typeMap2, bool considerRefKindDifferences, bool considerCustomModifiers, bool ignoreDynamic, bool ignoreTupleNames)
	{
		int length = params1.Length;
		for (int i = 0; i < length; i++)
		{
			ParameterSymbol parameterSymbol = params1[i];
			ParameterSymbol parameterSymbol2 = params2[i];
			TypeWithModifiers typeWithModifiers = SubstituteType(typeMap1, new TypeWithModifiers(parameterSymbol.Type, parameterSymbol.CustomModifiers));
			TypeWithModifiers other = SubstituteType(typeMap2, new TypeWithModifiers(parameterSymbol2.Type, parameterSymbol2.CustomModifiers));
			if (considerCustomModifiers)
			{
				if (!typeWithModifiers.Equals(other, TypeCompareKind.ConsiderEverything.AddIgnoreDynamic(ignoreDynamic).AddIgnoreTupleNames(ignoreTupleNames)) || !SubstituteModifiers(typeMap1, parameterSymbol.RefCustomModifiers).SequenceEqual(SubstituteModifiers(typeMap2, parameterSymbol2.RefCustomModifiers)))
				{
					return false;
				}
			}
			else if (!typeWithModifiers.Type.Equals(other.Type, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds.AddIgnoreDynamic(ignoreDynamic).AddIgnoreTupleNames(ignoreTupleNames)))
			{
				return false;
			}
			RefKind refKind = parameterSymbol.RefKind;
			RefKind refKind2 = parameterSymbol2.RefKind;
			if (considerRefKindDifferences)
			{
				if (refKind != refKind2)
				{
					return false;
				}
			}
			else if (refKind == RefKind.None != (refKind2 == RefKind.None))
			{
				return false;
			}
		}
		return true;
	}

	private static TypeWithModifiers SubstituteType(TypeMap typeMap, TypeWithModifiers typeSymbol)
	{
		if (typeMap != null)
		{
			return typeSymbol.SubstituteType(typeMap);
		}
		return typeSymbol;
	}

	private static ImmutableArray<CustomModifier> SubstituteModifiers(TypeMap typeMap, ImmutableArray<CustomModifier> customModifiers)
	{
		return typeMap?.SubstituteCustomModifiers(customModifiers) ?? customModifiers;
	}

	private static CallingConvention GetCallingConvention(Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)member).CallingConvention;
		case SymbolKind.Event:
		case SymbolKind.Property:
			if (!member.IsStatic)
			{
				return CallingConvention.HasThis;
			}
			return CallingConvention.Default;
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	private static bool IsVarargMethod(Symbol member)
	{
		if (member.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)member).IsVararg;
		}
		return false;
	}

	internal static bool ConsideringTupleNamesCreatesDifference(Symbol member1, Symbol member2)
	{
		if (!CSharpWithTupleNamesComparer.Equals(member1, member2))
		{
			return CSharpWithoutTupleNamesComparer.Equals(member1, member2);
		}
		return false;
	}
}
