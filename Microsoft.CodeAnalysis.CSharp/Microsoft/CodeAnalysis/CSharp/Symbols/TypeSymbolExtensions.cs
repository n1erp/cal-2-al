using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class TypeSymbolExtensions
{
	private static readonly string[] s_expressionsNamespaceName = new string[4] { "Expressions", "Linq", "System", "" };

	private static readonly Func<TypeSymbol, TypeParameterSymbol, bool, bool> s_containsTypeParameterPredicate = (TypeSymbol type, TypeParameterSymbol parameter, bool unused) => type.TypeKind == TypeKind.TypeParameter && ((object)parameter == null || type == parameter);

	private static readonly Func<TypeSymbol, Symbol, bool, bool> s_isTypeParameterWithSpecificContainerPredicate = (TypeSymbol type, Symbol parameterContainer, bool unused) => type.TypeKind == TypeKind.TypeParameter && (object)type.ContainingSymbol == parameterContainer;

	private static readonly Func<TypeSymbol, HashSet<TypeParameterSymbol>, bool, bool> s_containsTypeParametersPredicate = (TypeSymbol type, HashSet<TypeParameterSymbol> parameters, bool unused) => type.TypeKind == TypeKind.TypeParameter && parameters.Contains((TypeParameterSymbol)type);

	private static readonly Func<TypeSymbol, object, bool, bool> s_containsDynamicPredicate = (TypeSymbol type, object unused1, bool unused2) => type.TypeKind == TypeKind.Dynamic;

	public static bool ImplementsInterface(this TypeSymbol subType, TypeSymbol superInterface, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		foreach (NamedTypeSymbol item in subType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
		{
			if (item.IsInterface && item == superInterface)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanBeAssignedNull(this TypeSymbol type)
	{
		if (!type.IsReferenceType && !type.IsPointerType())
		{
			return type.IsNullableType();
		}
		return true;
	}

	public static bool CanContainNull(this TypeSymbol type)
	{
		if (type.IsValueType)
		{
			return type.IsNullableType();
		}
		return true;
	}

	public static bool CanBeConst(this TypeSymbol typeSymbol)
	{
		if (!typeSymbol.IsReferenceType && !typeSymbol.IsEnumType())
		{
			return typeSymbol.SpecialType.CanBeConst();
		}
		return true;
	}

	public static bool IsNonNullableValueType(this TypeSymbol typeArgument)
	{
		if (!typeArgument.IsValueType)
		{
			return false;
		}
		return !typeArgument.IsNullableTypeOrTypeParameter();
	}

	public static bool IsNullableTypeOrTypeParameter(this TypeSymbol type)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
		{
			foreach (TypeSymbol constraintTypesNoUseSiteDiagnostic in ((TypeParameterSymbol)type).ConstraintTypesNoUseSiteDiagnostics)
			{
				if (constraintTypesNoUseSiteDiagnostic.IsNullableTypeOrTypeParameter())
				{
					return true;
				}
			}
			return false;
		}
		return type.IsNullableType();
	}

	public static bool IsNullableType(this TypeSymbol type)
	{
		return type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
	}

	public static TypeSymbol GetNullableUnderlyingType(this TypeSymbol type)
	{
		return ((NamedTypeSymbol)type).TypeArgumentsNoUseSiteDiagnostics[0];
	}

	public static TypeSymbol StrippedType(this TypeSymbol type)
	{
		if (!type.IsNullableType())
		{
			return type;
		}
		return type.GetNullableUnderlyingType();
	}

	public static TypeSymbol TupleUnderlyingTypeOrSelf(this TypeSymbol type)
	{
		return type.TupleUnderlyingType ?? type;
	}

	public static TypeSymbol EnumUnderlyingType(this TypeSymbol type)
	{
		if (!type.IsEnumType())
		{
			return type;
		}
		return type.GetEnumUnderlyingType();
	}

	public static bool IsObjectType(this TypeSymbol type)
	{
		return type.SpecialType == SpecialType.System_Object;
	}

	public static bool IsStringType(this TypeSymbol type)
	{
		return type.SpecialType == SpecialType.System_String;
	}

	public static bool IsCharType(this TypeSymbol type)
	{
		return type.SpecialType == SpecialType.System_Char;
	}

	public static bool IsIntegralType(this TypeSymbol type)
	{
		return type.SpecialType.IsIntegralType();
	}

	public static NamedTypeSymbol GetEnumUnderlyingType(this TypeSymbol type)
	{
		if (!(type is NamedTypeSymbol namedTypeSymbol))
		{
			return null;
		}
		return namedTypeSymbol.EnumUnderlyingType;
	}

	public static bool IsEnumType(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.Enum;
	}

	public static bool IsValidEnumType(this TypeSymbol type)
	{
		NamedTypeSymbol enumUnderlyingType = type.GetEnumUnderlyingType();
		if ((object)enumUnderlyingType != null)
		{
			return enumUnderlyingType.SpecialType != SpecialType.None;
		}
		return false;
	}

	public static bool IsValidAttributeParameterType(this TypeSymbol type, CSharpCompilation compilation)
	{
		return type.GetAttributeParameterTypedConstantKind(compilation) != TypedConstantKind.Error;
	}

	public static TypedConstantKind GetAttributeParameterTypedConstantKind(this TypeSymbol type, CSharpCompilation compilation)
	{
		TypedConstantKind typedConstantKind = TypedConstantKind.Error;
		if ((object)type == null)
		{
			return TypedConstantKind.Error;
		}
		if (type.Kind == SymbolKind.ArrayType)
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
			if (!arrayTypeSymbol.IsSZArray)
			{
				return TypedConstantKind.Error;
			}
			typedConstantKind = TypedConstantKind.Array;
			type = arrayTypeSymbol.ElementType;
		}
		if (type.IsEnumType())
		{
			if (typedConstantKind == TypedConstantKind.Error)
			{
				typedConstantKind = TypedConstantKind.Enum;
			}
			type = type.GetEnumUnderlyingType();
		}
		TypedConstantKind typedConstantKind2 = TypedConstant.GetTypedConstantKind(type, compilation);
		switch (typedConstantKind2)
		{
		case TypedConstantKind.Error:
		case TypedConstantKind.Enum:
		case TypedConstantKind.Array:
			return TypedConstantKind.Error;
		default:
			if (typedConstantKind == TypedConstantKind.Array || typedConstantKind == TypedConstantKind.Enum)
			{
				return typedConstantKind;
			}
			return typedConstantKind2;
		}
	}

	public static bool IsValidExtensionParameterType(this TypeSymbol type)
	{
		TypeKind typeKind = type.TypeKind;
		if (typeKind == TypeKind.Dynamic || typeKind == TypeKind.Pointer)
		{
			return false;
		}
		return true;
	}

	public static bool IsInterfaceType(this TypeSymbol type)
	{
		if (type.Kind == SymbolKind.NamedType)
		{
			return ((NamedTypeSymbol)type).IsInterface;
		}
		return false;
	}

	public static bool IsClassType(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.Class;
	}

	public static bool IsStructType(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.Struct;
	}

	public static bool IsErrorType(this TypeSymbol type)
	{
		return type.Kind == SymbolKind.ErrorType;
	}

	public static bool IsMethodTypeParameter(this TypeParameterSymbol p)
	{
		return p.ContainingSymbol.Kind == SymbolKind.Method;
	}

	public static bool IsDynamic(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.Dynamic;
	}

	public static bool IsTypeParameter(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.TypeParameter;
	}

	public static bool IsArray(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.Array;
	}

	public static bool IsSZArray(this TypeSymbol type)
	{
		if (type.TypeKind == TypeKind.Array)
		{
			return ((ArrayTypeSymbol)type).IsSZArray;
		}
		return false;
	}

	public static NamedTypeSymbol GetDelegateType(this TypeSymbol type)
	{
		if ((object)type == null)
		{
			return null;
		}
		if (type.IsExpressionTree())
		{
			type = ((NamedTypeSymbol)type).TypeArgumentsNoUseSiteDiagnostics[0];
		}
		if (!type.IsDelegateType())
		{
			return null;
		}
		return (NamedTypeSymbol)type;
	}

	public static bool IsExpressionTree(this TypeSymbol _type)
	{
		if (_type.OriginalDefinition is NamedTypeSymbol { Arity: 1, MangleName: not false, Name: "Expression" } namedTypeSymbol)
		{
			return CheckFullName(namedTypeSymbol.ContainingSymbol, s_expressionsNamespaceName);
		}
		return false;
	}

	public static bool IsPossibleArrayGenericInterface(this TypeSymbol _type)
	{
		if (!(_type is NamedTypeSymbol { OriginalDefinition: var originalDefinition }))
		{
			return false;
		}
		SpecialType specialType = originalDefinition.SpecialType;
		if (specialType == SpecialType.System_Collections_Generic_IList_T || specialType == SpecialType.System_Collections_Generic_ICollection_T || specialType == SpecialType.System_Collections_Generic_IEnumerable_T || specialType == SpecialType.System_Collections_Generic_IReadOnlyList_T || specialType == SpecialType.System_Collections_Generic_IReadOnlyCollection_T)
		{
			return true;
		}
		return false;
	}

	private static bool CheckFullName(Symbol symbol, string[] names)
	{
		for (int i = 0; i < names.Length; i++)
		{
			if ((object)symbol == null || symbol.Name != names[i])
			{
				return false;
			}
			symbol = symbol.ContainingSymbol;
		}
		return true;
	}

	public static bool IsDelegateType(this TypeSymbol type)
	{
		return type.TypeKind == TypeKind.Delegate;
	}

	public static ImmutableArray<ParameterSymbol> DelegateParameters(this TypeSymbol type)
	{
		return type.DelegateInvokeMethod()?.Parameters ?? default(ImmutableArray<ParameterSymbol>);
	}

	public static bool TryGetElementTypesIfTupleOrCompatible(this TypeSymbol type, out ImmutableArray<TypeSymbol> elementTypes)
	{
		if (type.IsTupleType)
		{
			elementTypes = ((TupleTypeSymbol)type).TupleElementTypes;
			return true;
		}
		if (!type.IsTupleCompatible(out var tupleCardinality))
		{
			elementTypes = default;
			return false;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(tupleCardinality);
		TupleTypeSymbol.AddElementTypes((NamedTypeSymbol)type, instance);
		elementTypes = instance.ToImmutableAndFree();
		return true;
	}

	public static ImmutableArray<TypeSymbol> GetElementTypesOfTupleOrCompatible(this TypeSymbol type)
	{
		if (type.IsTupleType)
		{
			return ((TupleTypeSymbol)type).TupleElementTypes;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		TupleTypeSymbol.AddElementTypes((NamedTypeSymbol)type, instance);
		return instance.ToImmutableAndFree();
	}

	public static MethodSymbol DelegateInvokeMethod(this TypeSymbol type)
	{
		return type.GetDelegateType().DelegateInvokeMethod;
	}

	public static ConstantValue GetDefaultValue(this TypeSymbol type)
	{
		if (type.IsErrorType())
		{
			return null;
		}
		if (type.IsReferenceType)
		{
			return ConstantValue.Null;
		}
		if (type.IsValueType)
		{
			if (type.IsEnumType())
			{
				type = type.GetEnumUnderlyingType();
			}
			SpecialType specialType = type.SpecialType;
			if ((uint)(specialType - 7) <= 12u)
			{
				return ConstantValue.Default(type.SpecialType);
			}
		}
		return null;
	}

	public static SpecialType GetSpecialTypeSafe(this TypeSymbol type)
	{
		return type?.SpecialType ?? SpecialType.None;
	}

	public static bool IsAtLeastAsVisibleAs(this TypeSymbol type, Symbol sym, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		HashSet<DiagnosticInfo> localUseSiteDiagnostics = useSiteDiagnostics;
		TypeSymbol typeSymbol = type.VisitType((TypeSymbol type2, Symbol symbol, bool unused) => IsTypeLessVisibleThan(type2, symbol, ref localUseSiteDiagnostics), sym);
		useSiteDiagnostics = localUseSiteDiagnostics;
		return (object)typeSymbol == null;
	}

	private static bool IsTypeLessVisibleThan(TypeSymbol type, Symbol sym, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		switch (type.TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.Submission:
			return !IsAsRestrictive((NamedTypeSymbol)type, sym, ref useSiteDiagnostics);
		default:
			return false;
		}
	}

	public static TypeSymbol VisitType<T>(this TypeSymbol type, Func<TypeSymbol, T, bool, bool> predicate, T arg)
	{
		TypeSymbol typeSymbol = type;
		while (true)
		{
			bool arg2 = false;
			switch (typeSymbol.TypeKind)
			{
			case TypeKind.Class:
			case TypeKind.Delegate:
			case TypeKind.Enum:
			case TypeKind.Interface:
			case TypeKind.Struct:
			{
				NamedTypeSymbol containingType = typeSymbol.ContainingType;
				if ((object)containingType != null)
				{
					arg2 = true;
					TypeSymbol typeSymbol2 = containingType.VisitType(predicate, arg);
					if ((object)typeSymbol2 != null)
					{
						return typeSymbol2;
					}
				}
				break;
			}
			}
			if (predicate(typeSymbol, arg, arg2))
			{
				break;
			}
			switch (typeSymbol.TypeKind)
			{
			case TypeKind.Dynamic:
			case TypeKind.Enum:
			case TypeKind.Error:
			case TypeKind.TypeParameter:
			case TypeKind.Submission:
				return null;
			case TypeKind.Class:
			case TypeKind.Delegate:
			case TypeKind.Interface:
			case TypeKind.Struct:
				if (typeSymbol.IsTupleType)
				{
					typeSymbol = typeSymbol.TupleUnderlyingType;
				}
				foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in ((NamedTypeSymbol)typeSymbol).TypeArgumentsNoUseSiteDiagnostics)
				{
					TypeSymbol typeSymbol3 = typeArgumentsNoUseSiteDiagnostic.VisitType(predicate, arg);
					if ((object)typeSymbol3 != null)
					{
						return typeSymbol3;
					}
				}
				return null;
			case TypeKind.Array:
				typeSymbol = ((ArrayTypeSymbol)typeSymbol).ElementType;
				break;
			case TypeKind.Pointer:
				typeSymbol = ((PointerTypeSymbol)typeSymbol).PointedAtType;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(typeSymbol.TypeKind);
			}
		}
		return typeSymbol;
	}

	private static bool IsAsRestrictive(NamedTypeSymbol s1, Symbol sym2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		Accessibility declaredAccessibility = s1.DeclaredAccessibility;
		if (declaredAccessibility == Accessibility.Public)
		{
			return true;
		}
		Symbol symbol = sym2;
		while (symbol.Kind != SymbolKind.Namespace)
		{
			Accessibility declaredAccessibility2 = symbol.DeclaredAccessibility;
			switch (declaredAccessibility)
			{
			case Accessibility.Internal:
				if ((declaredAccessibility2 == Accessibility.Private || declaredAccessibility2 == Accessibility.Internal || declaredAccessibility2 == Accessibility.ProtectedAndInternal) && symbol.ContainingAssembly.HasInternalAccessTo(s1.ContainingAssembly))
				{
					return true;
				}
				break;
			case Accessibility.ProtectedAndInternal:
				if ((declaredAccessibility2 != Accessibility.Private && declaredAccessibility2 != Accessibility.Internal && declaredAccessibility2 != Accessibility.ProtectedAndInternal) || !symbol.ContainingAssembly.HasInternalAccessTo(s1.ContainingAssembly))
				{
					break;
				}
				goto case Accessibility.Protected;
			case Accessibility.Protected:
			{
				NamedTypeSymbol containingType3 = s1.ContainingType;
				if ((object)containingType3 == null)
				{
					break;
				}
				switch (declaredAccessibility2)
				{
				case Accessibility.Private:
				{
					NamedTypeSymbol containingType5 = symbol.ContainingType;
					while ((object)containingType5 != null)
					{
						if (containingType3.IsAccessibleViaInheritance(containingType5, ref useSiteDiagnostics))
						{
							return true;
						}
						containingType5 = containingType5.ContainingType;
					}
					break;
				}
				case Accessibility.ProtectedAndInternal:
				case Accessibility.Protected:
				{
					NamedTypeSymbol containingType4 = symbol.ContainingType;
					if ((object)containingType4 != null && containingType3.IsAccessibleViaInheritance(containingType4, ref useSiteDiagnostics))
					{
						return true;
					}
					break;
				}
				}
				break;
			}
			case Accessibility.ProtectedOrInternal:
			{
				NamedTypeSymbol containingType6 = s1.ContainingType;
				if ((object)containingType6 == null)
				{
					break;
				}
				switch (declaredAccessibility2)
				{
				case Accessibility.Private:
				{
					if (symbol.ContainingAssembly.HasInternalAccessTo(s1.ContainingAssembly))
					{
						return true;
					}
					NamedTypeSymbol containingType7 = symbol.ContainingType;
					while ((object)containingType7 != null)
					{
						if (containingType6.IsAccessibleViaInheritance(containingType7, ref useSiteDiagnostics))
						{
							return true;
						}
						containingType7 = containingType7.ContainingType;
					}
					break;
				}
				case Accessibility.Internal:
					if (symbol.ContainingAssembly.HasInternalAccessTo(s1.ContainingAssembly))
					{
						return true;
					}
					break;
				case Accessibility.Protected:
					if (containingType6.IsAccessibleViaInheritance(symbol.ContainingType, ref useSiteDiagnostics))
					{
						return true;
					}
					break;
				case Accessibility.ProtectedAndInternal:
					if (symbol.ContainingAssembly.HasInternalAccessTo(s1.ContainingAssembly) || containingType6.IsAccessibleViaInheritance(symbol.ContainingType, ref useSiteDiagnostics))
					{
						return true;
					}
					break;
				case Accessibility.ProtectedOrInternal:
					if (symbol.ContainingAssembly.HasInternalAccessTo(s1.ContainingAssembly) && containingType6.IsAccessibleViaInheritance(symbol.ContainingType, ref useSiteDiagnostics))
					{
						return true;
					}
					break;
				}
				break;
			}
			case Accessibility.Private:
			{
				if (declaredAccessibility2 != Accessibility.Private)
				{
					break;
				}
				NamedTypeSymbol containingType = s1.ContainingType;
				if ((object)containingType == null)
				{
					break;
				}
				NamedTypeSymbol originalDefinition = containingType.OriginalDefinition;
				NamedTypeSymbol containingType2 = symbol.ContainingType;
				while ((object)containingType2 != null)
				{
					if ((object)containingType2.OriginalDefinition == originalDefinition || (originalDefinition.TypeKind == TypeKind.Submission && containingType2.TypeKind == TypeKind.Submission))
					{
						return true;
					}
					containingType2 = containingType2.ContainingType;
				}
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(declaredAccessibility);
			}
			symbol = symbol.ContainingSymbol;
		}
		return false;
	}

	public static bool IsUnboundGenericType(this TypeSymbol type)
	{
		if (type is NamedTypeSymbol namedTypeSymbol)
		{
			return namedTypeSymbol.IsUnboundGenericType;
		}
		return false;
	}

	public static bool IsTopLevelType(this NamedTypeSymbol type)
	{
		return (object)type.ContainingType == null;
	}

	public static bool ContainsTypeParameter(this TypeSymbol type, TypeParameterSymbol parameter = null)
	{
		return (object)type.VisitType(s_containsTypeParameterPredicate, parameter) != null;
	}

	public static bool ContainsTypeParameter(this TypeSymbol type, MethodSymbol parameterContainer)
	{
		return (object)type.VisitType(s_isTypeParameterWithSpecificContainerPredicate, parameterContainer) != null;
	}

	public static bool ContainsTypeParameters(this TypeSymbol type, HashSet<TypeParameterSymbol> parameters)
	{
		return (object)type.VisitType(s_containsTypeParametersPredicate, parameters) != null;
	}

	public static bool ContainsDynamic(this TypeSymbol type)
	{
		return (object)type.VisitType(s_containsDynamicPredicate, null) != null;
	}

	internal static bool ContainsTuple(this TypeSymbol type)
	{
		return (object)type.VisitType((TypeSymbol t, object _1, bool _2) => t.IsTupleType, null) != null;
	}

	internal static bool ContainsTupleNames(this TypeSymbol type)
	{
		return (object)type.VisitType((TypeSymbol t, object _1, bool _2) => !t.TupleElementNames.IsDefault, null) != null;
	}

	internal static TypeSymbol GetNonErrorGuess(this TypeSymbol type)
	{
		return ExtendedErrorTypeSymbol.ExtractNonErrorType(type);
	}

	internal static TypeKind GetNonErrorTypeKindGuess(this TypeSymbol type)
	{
		return ExtendedErrorTypeSymbol.ExtractNonErrorTypeKind(type);
	}

	internal static bool IsValidV6SwitchGoverningType(this TypeSymbol type, bool isTargetTypeOfUserDefinedOp = false)
	{
		if (type.IsNullableType())
		{
			type = type.GetNullableUnderlyingType();
		}
		if (!isTargetTypeOfUserDefinedOp && type.IsEnumType())
		{
			type = type.GetEnumUnderlyingType();
		}
		switch (type.SpecialType)
		{
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_String:
			return true;
		case SpecialType.System_Boolean:
			return !isTargetTypeOfUserDefinedOp;
		default:
			return false;
		}
	}

	internal static bool IsRestrictedType(this TypeSymbol type, bool ignoreSpanLikeTypes = false)
	{
		SpecialType specialType = type.SpecialType;
		if ((uint)(specialType - 36) <= 2u)
		{
			return true;
		}
		if (!ignoreSpanLikeTypes)
		{
			return type.IsByRefLikeType;
		}
		return false;
	}

	public static bool IsIntrinsicType(this TypeSymbol type)
	{
		return type.SpecialType.IsIntrinsicType();
	}

	public static bool IsPartial(this TypeSymbol type)
	{
		if (type is SourceNamedTypeSymbol sourceNamedTypeSymbol)
		{
			return sourceNamedTypeSymbol.IsPartial;
		}
		return false;
	}

	public static bool IsPointerType(this TypeSymbol type)
	{
		return type is PointerTypeSymbol;
	}

	internal static int FixedBufferElementSizeInBytes(this TypeSymbol type)
	{
		return type.SpecialType.FixedBufferElementSizeInBytes();
	}

	internal static bool IsValidVolatileFieldType(this TypeSymbol type)
	{
		switch (type.TypeKind)
		{
		case TypeKind.Struct:
			return type.SpecialType.IsValidVolatileFieldType();
		case TypeKind.Array:
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Dynamic:
		case TypeKind.Error:
		case TypeKind.Interface:
		case TypeKind.Pointer:
			return true;
		case TypeKind.Enum:
			return ((NamedTypeSymbol)type).EnumUnderlyingType.SpecialType.IsValidVolatileFieldType();
		case TypeKind.TypeParameter:
			return type.IsReferenceType;
		case TypeKind.Submission:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		default:
			return false;
		}
	}

	public static bool MarkCheckedIfNecessary(this TypeSymbol type, ref HashSet<TypeSymbol> checkedTypes)
	{
		if (checkedTypes == null)
		{
			checkedTypes = new HashSet<TypeSymbol>();
		}
		return checkedTypes.Add(type);
	}

	internal static bool IsUnsafe(this TypeSymbol type)
	{
		while (true)
		{
			switch (type.TypeKind)
			{
			case TypeKind.Pointer:
				return true;
			case TypeKind.Array:
				break;
			default:
				return false;
			}
			type = ((ArrayTypeSymbol)type).ElementType;
		}
	}

	internal static bool IsVoidPointer(this TypeSymbol type)
	{
		if (type.IsPointerType())
		{
			return ((PointerTypeSymbol)type).PointedAtType.SpecialType == SpecialType.System_Void;
		}
		return false;
	}

	internal static bool IsPrimitiveRecursiveStruct(this TypeSymbol t)
	{
		return t.SpecialType.IsPrimitiveRecursiveStruct();
	}

	internal static int ComputeHashCode(this NamedTypeSymbol type)
	{
		int hashCode = type.OriginalDefinition.GetHashCode();
		hashCode = Hash.Combine(type.ContainingType, hashCode);
		if ((object)type.ConstructedFrom != type)
		{
			foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in type.TypeArgumentsNoUseSiteDiagnostics)
			{
				hashCode = Hash.Combine(typeArgumentsNoUseSiteDiagnostic, hashCode);
			}
		}
		if (hashCode == 0)
		{
			hashCode++;
		}
		return hashCode;
	}

	public static TypeSymbol AsDynamicIfNoPia(this TypeSymbol type, NamedTypeSymbol containingType)
	{
		if (type.SpecialType == SpecialType.System_Object)
		{
			AssemblySymbol containingAssembly = containingType.ContainingAssembly;
			if ((object)containingAssembly != null && containingAssembly.IsLinked && containingType.IsComImport)
			{
				return DynamicTypeSymbol.Instance;
			}
		}
		return type;
	}

	internal static bool IsVerifierReference(this TypeSymbol type)
	{
		if (type.IsReferenceType)
		{
			return type.TypeKind != TypeKind.TypeParameter;
		}
		return false;
	}

	internal static bool IsVerifierValue(this TypeSymbol type)
	{
		if (type.IsValueType)
		{
			return type.TypeKind != TypeKind.TypeParameter;
		}
		return false;
	}

	internal static void AddUseSiteDiagnostics(this TypeSymbol type, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DiagnosticInfo useSiteDiagnostic = type.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			if (useSiteDiagnostics == null)
			{
				useSiteDiagnostics = new HashSet<DiagnosticInfo>();
			}
			useSiteDiagnostics.Add(useSiteDiagnostic);
		}
	}

	internal static ImmutableArray<TypeParameterSymbol> GetAllTypeParameters(this NamedTypeSymbol type)
	{
		if ((object)type.ContainingType == null)
		{
			return type.TypeParameters;
		}
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		type.GetAllTypeParameters(instance);
		return instance.ToImmutableAndFree();
	}

	internal static void GetAllTypeParameters(this NamedTypeSymbol type, ArrayBuilder<TypeParameterSymbol> result)
	{
		type.ContainingType?.GetAllTypeParameters(result);
		result.AddRange(type.TypeParameters);
	}

	internal static TypeParameterSymbol FindEnclosingTypeParameter(this NamedTypeSymbol type, string name)
	{
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		type.GetAllTypeParameters(instance);
		TypeParameterSymbol result = null;
		foreach (TypeParameterSymbol item in instance)
		{
			if (name == item.Name)
			{
				result = item;
				break;
			}
		}
		instance.Free();
		return result;
	}

	internal static TypeParameterSymbol FindEnclosingTypeParameter(this Symbol methodOrType, string name)
	{
		while (methodOrType != null)
		{
			switch (methodOrType.Kind)
			{
			default:
				return null;
			case SymbolKind.ErrorType:
			case SymbolKind.Event:
			case SymbolKind.Field:
			case SymbolKind.Method:
			case SymbolKind.NamedType:
			case SymbolKind.Property:
				foreach (TypeParameterSymbol memberTypeParameter in methodOrType.GetMemberTypeParameters())
				{
					if (memberTypeParameter.Name == name)
					{
						return memberTypeParameter;
					}
				}
				methodOrType = methodOrType.ContainingSymbol;
				break;
			}
		}
		return null;
	}

	internal static bool HasNameQualifier(this NamedTypeSymbol type, string qualifiedName)
	{
		Symbol containingSymbol = type.ContainingSymbol;
		if (containingSymbol.Kind != SymbolKind.Namespace)
		{
			return string.Equals(containingSymbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat), qualifiedName, StringComparison.Ordinal);
		}
		NamespaceSymbol namespaceSymbol = (NamespaceSymbol)containingSymbol;
		if (namespaceSymbol.IsGlobalNamespace)
		{
			return qualifiedName.Length == 0;
		}
		return HasNamespaceName(namespaceSymbol, qualifiedName, StringComparison.Ordinal, qualifiedName.Length);
	}

	private static bool HasNamespaceName(NamespaceSymbol @namespace, string namespaceName, StringComparison comparison, int length)
	{
		if (length == 0)
		{
			return false;
		}
		NamespaceSymbol containingNamespace = @namespace.ContainingNamespace;
		int num = namespaceName.LastIndexOf('.', length - 1, length);
		int indexB = 0;
		if (num >= 0)
		{
			if (containingNamespace.IsGlobalNamespace)
			{
				return false;
			}
			if (!HasNamespaceName(containingNamespace, namespaceName, comparison, num))
			{
				return false;
			}
			int num2 = num + 1;
			indexB = num2;
			length -= num2;
		}
		else if (!containingNamespace.IsGlobalNamespace)
		{
			return false;
		}
		string name = @namespace.Name;
		if (name.Length == length)
		{
			return string.Compare(name, 0, namespaceName, indexB, length, comparison) == 0;
		}
		return false;
	}

	internal static bool IsNonGenericTaskType(this TypeSymbol type, CSharpCompilation compilation)
	{
		if (!(type is NamedTypeSymbol { Arity: 0 } namedTypeSymbol))
		{
			return false;
		}
		if ((object)namedTypeSymbol == compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task))
		{
			return true;
		}
		if (namedTypeSymbol.SpecialType == SpecialType.System_Void)
		{
			return false;
		}
		object builderArgument;
		return namedTypeSymbol.IsCustomTaskType(out builderArgument);
	}

	internal static bool IsGenericTaskType(this TypeSymbol type, CSharpCompilation compilation)
	{
		if (!(type is NamedTypeSymbol { Arity: 1 } namedTypeSymbol))
		{
			return false;
		}
		if ((object)namedTypeSymbol.ConstructedFrom == compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task_T))
		{
			return true;
		}
		object builderArgument;
		return namedTypeSymbol.IsCustomTaskType(out builderArgument);
	}

	internal static bool IsCustomTaskType(this NamedTypeSymbol type, out object builderArgument)
	{
		if (type.Arity < 2)
		{
			foreach (CSharpAttributeData attribute in type.GetAttributes())
			{
				if (attribute.IsTargetAttribute(type, AttributeDescription.AsyncMethodBuilderAttribute) && attribute.CommonConstructorArguments.Length == 1 && attribute.CommonConstructorArguments[0].Kind == TypedConstantKind.Type)
				{
					builderArgument = attribute.CommonConstructorArguments[0].Value;
					return true;
				}
			}
		}
		builderArgument = null;
		return false;
	}

	internal static TypeSymbol NormalizeTaskTypes(this TypeSymbol type, CSharpCompilation compilation)
	{
		NormalizeTaskTypesInType(compilation, ref type);
		return type;
	}

	private static bool NormalizeTaskTypesInType(CSharpCompilation compilation, ref TypeSymbol type)
	{
		switch (type.Kind)
		{
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		{
			NamedTypeSymbol type2 = (NamedTypeSymbol)type;
			bool result3 = (type.IsTupleType ? NormalizeTaskTypesInTuple(compilation, ref type2) : NormalizeTaskTypesInNamedType(compilation, ref type2));
			type = type2;
			return result3;
		}
		case SymbolKind.ArrayType:
		{
			ArrayTypeSymbol arrayType = (ArrayTypeSymbol)type;
			bool result2 = NormalizeTaskTypesInArray(compilation, ref arrayType);
			type = arrayType;
			return result2;
		}
		case SymbolKind.PointerType:
		{
			PointerTypeSymbol pointerType = (PointerTypeSymbol)type;
			bool result = NormalizeTaskTypesInPointer(compilation, ref pointerType);
			type = pointerType;
			return result;
		}
		default:
			return false;
		}
	}

	private static bool NormalizeTaskTypesInNamedType(CSharpCompilation compilation, ref NamedTypeSymbol type)
	{
		bool flag = false;
		if (!type.IsDefinition)
		{
			ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance();
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			type.GetAllTypeArguments(instance, ref useSiteDiagnostics);
			for (int i = 0; i < instance.Count; i++)
			{
				TypeWithModifiers typeWithModifiers = instance[i];
				TypeSymbol type2 = typeWithModifiers.Type;
				if (NormalizeTaskTypesInType(compilation, ref type2))
				{
					flag = true;
					instance[i] = new TypeWithModifiers(type2, typeWithModifiers.CustomModifiers);
				}
			}
			if (flag)
			{
				NamedTypeSymbol originalDefinition = type.OriginalDefinition;
				TypeMap typeMap = new TypeMap(originalDefinition.GetAllTypeParameters(), instance.ToImmutable(), allowAlpha: true);
				type = typeMap.SubstituteNamedType(originalDefinition);
			}
			instance.Free();
		}
		if (type.OriginalDefinition.IsCustomTaskType(out var _))
		{
			int arity = type.Arity;
			NamedTypeSymbol wellKnownType = compilation.GetWellKnownType((arity == 0) ? WellKnownType.System_Threading_Tasks_Task : WellKnownType.System_Threading_Tasks_Task_T);
			if (wellKnownType.TypeKind == TypeKind.Error)
			{
				return false;
			}
			type = ((arity == 0) ? wellKnownType : wellKnownType.Construct(ImmutableArray.Create(new TypeWithModifiers(type.TypeArgumentsNoUseSiteDiagnostics[0], type.HasTypeArgumentsCustomModifiers ? type.GetTypeArgumentCustomModifiers(0) : default(ImmutableArray<CustomModifier>))), unbound: false));
			flag = true;
		}
		return flag;
	}

	private static bool NormalizeTaskTypesInTuple(CSharpCompilation compilation, ref NamedTypeSymbol type)
	{
		NamedTypeSymbol type2 = type.TupleUnderlyingType;
		if (!NormalizeTaskTypesInNamedType(compilation, ref type2))
		{
			return false;
		}
		type = TupleTypeSymbol.Create(type2, type.TupleElementNames);
		return true;
	}

	private static bool NormalizeTaskTypesInArray(CSharpCompilation compilation, ref ArrayTypeSymbol arrayType)
	{
		TypeSymbol type = arrayType.ElementType;
		if (!NormalizeTaskTypesInType(compilation, ref type))
		{
			return false;
		}
		arrayType = arrayType.WithElementType(type);
		return true;
	}

	private static bool NormalizeTaskTypesInPointer(CSharpCompilation compilation, ref PointerTypeSymbol pointerType)
	{
		TypeSymbol type = pointerType.PointedAtType;
		if (!NormalizeTaskTypesInType(compilation, ref type))
		{
			return false;
		}
		pointerType = new PointerTypeSymbol(type, pointerType.CustomModifiers);
		return true;
	}

	internal static TypeReferenceWithAttributes GetTypeRefWithAttributes(this TypeSymbol type, CSharpCompilation declaringCompilation, ITypeReference typeRef)
	{
		if (type.ContainsTupleNames())
		{
			SynthesizedAttributeData synthesizedAttributeData = declaringCompilation.SynthesizeTupleNamesAttribute(type);
			if (synthesizedAttributeData != null)
			{
				return new TypeReferenceWithAttributes(typeRef, ImmutableArray.Create((ICustomAttribute)synthesizedAttributeData));
			}
		}
		return new TypeReferenceWithAttributes(typeRef);
	}

	internal static bool IsWellKnownTypeInAttribute(this ITypeSymbol typeSymbol)
	{
		return typeSymbol.IsWellKnownInteropServicesTopLevelType("InAttribute");
	}

	internal static bool IsWellKnownTypeUnmanagedType(this ITypeSymbol typeSymbol)
	{
		return typeSymbol.IsWellKnownInteropServicesTopLevelType("UnmanagedType");
	}

	private static bool IsWellKnownInteropServicesTopLevelType(this ITypeSymbol typeSymbol, string name)
	{
		if (typeSymbol.Name != name || typeSymbol.ContainingType != null)
		{
			return false;
		}
		INamespaceSymbol containingNamespace = typeSymbol.ContainingNamespace;
		if (containingNamespace?.Name != "InteropServices")
		{
			return false;
		}
		INamespaceSymbol containingNamespace2 = containingNamespace.ContainingNamespace;
		if (containingNamespace2?.Name != "Runtime")
		{
			return false;
		}
		INamespaceSymbol containingNamespace3 = containingNamespace2.ContainingNamespace;
		if (containingNamespace3?.Name != "System")
		{
			return false;
		}
		return containingNamespace3.ContainingNamespace?.IsGlobalNamespace ?? false;
	}

	public static NamedTypeSymbol AsUnboundGenericType(this NamedTypeSymbol type)
	{
		if (!type.IsGenericType)
		{
			throw new InvalidOperationException();
		}
		NamedTypeSymbol originalDefinition = type.OriginalDefinition;
		int arity = originalDefinition.Arity;
		NamedTypeSymbol containingType = originalDefinition.ContainingType;
		NamedTypeSymbol namedTypeSymbol = (((object)containingType == null) ? originalDefinition : originalDefinition.AsMember(containingType.IsGenericType ? containingType.AsUnboundGenericType() : containingType));
		if (arity == 0)
		{
			return namedTypeSymbol;
		}
		ImmutableArray<TypeWithModifiers> arguments = UnboundArgumentErrorTypeSymbol.CreateTypeArguments(namedTypeSymbol.TypeParameters, arity, new CSDiagnosticInfo(ErrorCode.ERR_UnexpectedUnboundGenericName));
		return namedTypeSymbol.Construct(arguments, unbound: true);
	}

	public static int CustomModifierCount(this TypeSymbol type)
	{
		if ((object)type == null)
		{
			return 0;
		}
		switch (type.Kind)
		{
		case SymbolKind.ArrayType:
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
			return arrayTypeSymbol.CustomModifiers.Length + arrayTypeSymbol.ElementType.CustomModifierCount();
		}
		case SymbolKind.PointerType:
		{
			PointerTypeSymbol pointerTypeSymbol = (PointerTypeSymbol)type;
			return pointerTypeSymbol.CustomModifiers.Length + pointerTypeSymbol.PointedAtType.CustomModifierCount();
		}
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		{
			if (type.IsTupleType)
			{
				return type.TupleUnderlyingType.CustomModifierCount();
			}
			if (type.IsDefinition)
			{
				break;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			int num = 0;
			while ((object)namedTypeSymbol != null)
			{
				foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics)
				{
					num += typeArgumentsNoUseSiteDiagnostic.CustomModifierCount();
				}
				if (namedTypeSymbol.HasTypeArgumentsCustomModifiers)
				{
					for (int i = 0; i < namedTypeSymbol.Arity; i++)
					{
						num += namedTypeSymbol.GetTypeArgumentCustomModifiers(i).Length;
					}
				}
				namedTypeSymbol = namedTypeSymbol.ContainingType;
			}
			return num;
		}
		}
		return 0;
	}

	public static bool HasCustomModifiers(this TypeSymbol type, bool flagNonDefaultArraySizesOrLowerBounds)
	{
		if ((object)type == null)
		{
			return false;
		}
		switch (type.Kind)
		{
		case SymbolKind.ArrayType:
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)type;
			if (!arrayTypeSymbol.CustomModifiers.Any() && !arrayTypeSymbol.ElementType.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds))
			{
				if (flagNonDefaultArraySizesOrLowerBounds)
				{
					return !arrayTypeSymbol.HasDefaultSizesAndLowerBounds;
				}
				return false;
			}
			return true;
		}
		case SymbolKind.PointerType:
		{
			PointerTypeSymbol pointerTypeSymbol = (PointerTypeSymbol)type;
			if (!pointerTypeSymbol.CustomModifiers.Any())
			{
				return pointerTypeSymbol.PointedAtType.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds);
			}
			return true;
		}
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		{
			if (type.IsTupleType)
			{
				return type.TupleUnderlyingType.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds);
			}
			if (type.IsDefinition)
			{
				break;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
			while ((object)namedTypeSymbol != null)
			{
				if (namedTypeSymbol.HasTypeArgumentsCustomModifiers)
				{
					return true;
				}
				foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics)
				{
					if (typeArgumentsNoUseSiteDiagnostic.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds))
					{
						return true;
					}
				}
				namedTypeSymbol = namedTypeSymbol.ContainingType;
			}
			break;
		}
		}
		return false;
	}

	public static bool CanUnifyWith(this TypeSymbol thisType, TypeSymbol otherType)
	{
		return TypeUnification.CanUnify(thisType, otherType);
	}

	internal static TypeSymbol GetNextBaseTypeNoUseSiteDiagnostics(this TypeSymbol type, ConsList<Symbol> basesBeingResolved, CSharpCompilation compilation, ref PooledHashSet<NamedTypeSymbol> visited)
	{
		switch (type.TypeKind)
		{
		case TypeKind.TypeParameter:
			return ((TypeParameterSymbol)type).EffectiveBaseClassNoUseSiteDiagnostics;
		case TypeKind.Class:
		case TypeKind.Error:
		case TypeKind.Interface:
		case TypeKind.Struct:
			return GetNextDeclaredBase((NamedTypeSymbol)type, basesBeingResolved, compilation, ref visited);
		default:
			return type.BaseTypeNoUseSiteDiagnostics;
		}
	}

	private static TypeSymbol GetNextDeclaredBase(NamedTypeSymbol type, ConsList<Symbol> basesBeingResolved, CSharpCompilation compilation, ref PooledHashSet<NamedTypeSymbol> visited)
	{
		if (basesBeingResolved != null && basesBeingResolved.ContainsReference(type.OriginalDefinition))
		{
			return null;
		}
		if (type.SpecialType == SpecialType.System_Object)
		{
			type.SetKnownToHaveNoDeclaredBaseCycles();
			return null;
		}
		NamedTypeSymbol declaredBaseType = type.GetDeclaredBaseType(basesBeingResolved);
		if ((object)declaredBaseType == null)
		{
			SetKnownToHaveNoDeclaredBaseCycles(ref visited);
			return GetDefaultBaseOrNull(type, compilation);
		}
		NamedTypeSymbol originalDefinition = type.OriginalDefinition;
		if (declaredBaseType.KnownToHaveNoDeclaredBaseCycles)
		{
			originalDefinition.SetKnownToHaveNoDeclaredBaseCycles();
			SetKnownToHaveNoDeclaredBaseCycles(ref visited);
		}
		else
		{
			visited = visited ?? PooledHashSet<NamedTypeSymbol>.GetInstance();
			visited.Add(originalDefinition);
			if (visited.Contains(declaredBaseType.OriginalDefinition))
			{
				return GetDefaultBaseOrNull(type, compilation);
			}
		}
		return declaredBaseType;
	}

	private static void SetKnownToHaveNoDeclaredBaseCycles(ref PooledHashSet<NamedTypeSymbol> visited)
	{
		if (visited == null)
		{
			return;
		}
		foreach (NamedTypeSymbol item in visited)
		{
			item.SetKnownToHaveNoDeclaredBaseCycles();
		}
		visited.Free();
		visited = null;
	}

	private static NamedTypeSymbol GetDefaultBaseOrNull(NamedTypeSymbol type, CSharpCompilation compilation)
	{
		if (compilation == null)
		{
			return null;
		}
		switch (type.TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Error:
		case TypeKind.Interface:
			return compilation.Assembly.GetSpecialType(SpecialType.System_Object);
		case TypeKind.Struct:
			return compilation.Assembly.GetSpecialType(SpecialType.System_ValueType);
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeKind);
		}
	}
}
