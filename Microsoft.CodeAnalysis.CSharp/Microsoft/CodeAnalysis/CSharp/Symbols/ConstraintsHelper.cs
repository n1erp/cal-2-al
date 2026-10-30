using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ConstraintsHelper
{
	private struct CheckConstraintsArgs(CSharpCompilation currentCompilation, ConversionsBase conversions, Location location, DiagnosticBag diagnostics)
	{
		public readonly CSharpCompilation CurrentCompilation = currentCompilation;

		public readonly ConversionsBase Conversions = conversions;

		public readonly Location Location = location;

		public readonly DiagnosticBag Diagnostics = diagnostics;
	}

	private static readonly Func<TypeSymbol, CheckConstraintsArgs, bool, bool> s_checkConstraintsSingleTypeFunc = (TypeSymbol type, CheckConstraintsArgs arg, bool unused) => CheckConstraintsSingleType(type, arg);

	public static TypeParameterBounds ResolveBounds(this TypeParameterSymbol typeParameter, AssemblySymbol corLibrary, ConsList<TypeParameterSymbol> inProgress, ImmutableArray<TypeSymbol> constraintTypes, bool inherited, CSharpCompilation currentCompilation, DiagnosticBag diagnostics)
	{
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		TypeParameterBounds result = typeParameter.ResolveBounds(corLibrary, inProgress, constraintTypes, inherited, currentCompilation, instance, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			diagnostics.Add(new CSDiagnostic(item.DiagnosticInfo, item.TypeParameter.Locations[0]));
		}
		instance.Free();
		return result;
	}

	public static TypeParameterBounds ResolveBounds(this TypeParameterSymbol typeParameter, AssemblySymbol corLibrary, ConsList<TypeParameterSymbol> inProgress, ImmutableArray<TypeSymbol> constraintTypes, bool inherited, CSharpCompilation currentCompilation, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder)
	{
		NamedTypeSymbol namedTypeSymbol = corLibrary.GetSpecialType((!typeParameter.HasValueTypeConstraint) ? SpecialType.System_Object : SpecialType.System_ValueType);
		TypeSymbol typeSymbol = namedTypeSymbol;
		DynamicTypeEraser dynamicTypeEraser = null;
		ImmutableArray<NamedTypeSymbol> interfaces;
		if (constraintTypes.Length == 0)
		{
			interfaces = ImmutableArray<NamedTypeSymbol>.Empty;
		}
		else
		{
			ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
			ArrayBuilder<NamedTypeSymbol> instance2 = ArrayBuilder<NamedTypeSymbol>.GetInstance();
			TypeConversions conversions = new TypeConversions(corLibrary);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			foreach (TypeSymbol item in constraintTypes)
			{
				NamedTypeSymbol namedTypeSymbol2;
				TypeSymbol typeSymbol2;
				switch (item.TypeKind)
				{
				case TypeKind.TypeParameter:
				{
					Symbol containingSymbol = typeParameter.ContainingSymbol;
					TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)item;
					ConsList<TypeParameterSymbol> inProgress2;
					if (typeParameterSymbol.ContainingSymbol == containingSymbol)
					{
						if (inProgress.ContainsReference(typeParameterSymbol))
						{
							diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameterSymbol, new CSDiagnosticInfo(ErrorCode.ERR_CircularConstraint, typeParameterSymbol, typeParameter)));
							continue;
						}
						inProgress2 = inProgress;
					}
					else
					{
						inProgress2 = ConsList<TypeParameterSymbol>.Empty;
					}
					namedTypeSymbol2 = typeParameterSymbol.GetEffectiveBaseClass(inProgress2);
					typeSymbol2 = typeParameterSymbol.GetDeducedBaseType(inProgress2);
					AddInterfaces(instance2, typeParameterSymbol.GetInterfaces(inProgress2));
					if (inherited || currentCompilation == null || !typeParameterSymbol.IsFromCompilation(currentCompilation))
					{
						break;
					}
					ErrorCode code;
					if (typeParameterSymbol.HasUnmanagedTypeConstraint)
					{
						code = ErrorCode.ERR_ConWithUnmanagedCon;
					}
					else
					{
						if (!typeParameterSymbol.HasValueTypeConstraint)
						{
							break;
						}
						code = ErrorCode.ERR_ConWithValCon;
					}
					diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(code, typeParameter, typeParameterSymbol)));
					continue;
				}
				case TypeKind.Class:
				case TypeKind.Delegate:
				case TypeKind.Interface:
				{
					NamedTypeSymbol namedTypeSymbol3;
					if (inherited || currentCompilation == null)
					{
						if (dynamicTypeEraser == null)
						{
							dynamicTypeEraser = new DynamicTypeEraser(corLibrary.GetSpecialType(SpecialType.System_Object));
						}
						namedTypeSymbol3 = (NamedTypeSymbol)dynamicTypeEraser.EraseDynamic(item);
					}
					else
					{
						namedTypeSymbol3 = (NamedTypeSymbol)item;
					}
					if (item.IsInterfaceType())
					{
						AddInterface(instance2, namedTypeSymbol3);
						instance.Add(item);
						continue;
					}
					namedTypeSymbol2 = namedTypeSymbol3;
					typeSymbol2 = item;
					break;
				}
				case TypeKind.Struct:
					namedTypeSymbol2 = corLibrary.GetSpecialType(SpecialType.System_ValueType);
					typeSymbol2 = item;
					break;
				case TypeKind.Enum:
					namedTypeSymbol2 = corLibrary.GetSpecialType(SpecialType.System_Enum);
					typeSymbol2 = item;
					break;
				case TypeKind.Array:
					namedTypeSymbol2 = corLibrary.GetSpecialType(SpecialType.System_Array);
					typeSymbol2 = item;
					break;
				case TypeKind.Error:
					namedTypeSymbol2 = (NamedTypeSymbol)item;
					typeSymbol2 = item;
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(item.TypeKind);
				case TypeKind.Dynamic:
				case TypeKind.Pointer:
					continue;
				}
				instance.Add(item);
				if (!typeSymbol.IsErrorType() && !typeSymbol2.IsErrorType() && !IsEncompassedBy(conversions, typeSymbol, typeSymbol2, ref useSiteDiagnostics))
				{
					if (!IsEncompassedBy(conversions, typeSymbol2, typeSymbol, ref useSiteDiagnostics))
					{
						diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_BaseConstraintConflict, typeParameter, typeSymbol2, typeSymbol)));
					}
					else
					{
						typeSymbol = typeSymbol2;
						namedTypeSymbol = namedTypeSymbol2;
					}
				}
			}
			AppendUseSiteDiagnostics(useSiteDiagnostics, typeParameter, ref useSiteDiagnosticsBuilder);
			constraintTypes = instance.ToImmutableAndFree();
			interfaces = instance2.ToImmutableAndFree();
		}
		if (constraintTypes.Length == 0 && typeSymbol.SpecialType == SpecialType.System_Object)
		{
			return null;
		}
		TypeParameterBounds typeParameterBounds = new TypeParameterBounds(constraintTypes, interfaces, namedTypeSymbol, typeSymbol);
		if (inherited)
		{
			CheckOverrideConstraints(typeParameter, typeParameterBounds, diagnosticsBuilder);
		}
		return typeParameterBounds;
	}

	internal static void CheckConstraintTypesVisibility(this Symbol containingSymbol, Location location, ImmutableArray<TypeParameterConstraintClause> constraintClauses, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		foreach (TypeParameterConstraintClause item in constraintClauses)
		{
			if (item == null)
			{
				continue;
			}
			foreach (TypeSymbol constraintType in item.ConstraintTypes)
			{
				if (!containingSymbol.IsNoMoreVisibleThan(constraintType, ref useSiteDiagnostics))
				{
					diagnostics.Add(ErrorCode.ERR_BadVisBound, location, containingSymbol, constraintType);
				}
			}
		}
		diagnostics.Add(location, useSiteDiagnostics);
	}

	internal static ImmutableArray<TypeParameterConstraintClause> MakeTypeParameterConstraints(this SourceMethodSymbol containingSymbol, Binder binder, ImmutableArray<TypeParameterSymbol> typeParameters, SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, Location location, DiagnosticBag diagnostics)
	{
		if (typeParameters.Length == 0 || constraintClauses.Count == 0)
		{
			return ImmutableArray<TypeParameterConstraintClause>.Empty;
		}
		binder = binder.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks | BinderFlags.GenericConstraintsClause);
		ImmutableArray<TypeParameterConstraintClause> immutableArray = binder.BindTypeParameterConstraintClauses(containingSymbol, typeParameters, constraintClauses, diagnostics);
		containingSymbol.CheckConstraintTypesVisibility(location, immutableArray, diagnostics);
		return immutableArray;
	}

	private static void CheckOverrideConstraints(TypeParameterSymbol typeParameter, TypeParameterBounds bounds, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder)
	{
		TypeSymbol deducedBaseType = bounds.DeducedBaseType;
		ImmutableArray<TypeSymbol> constraintTypes = bounds.ConstraintTypes;
		if (IsValueType(typeParameter, constraintTypes) && IsReferenceType(typeParameter, constraintTypes))
		{
			diagnosticsBuilder.Add(GenerateConflictingConstraintsError(typeParameter, deducedBaseType, deducedBaseType.IsValueType));
		}
		else if (deducedBaseType.IsNullableType() && (typeParameter.HasValueTypeConstraint || typeParameter.HasReferenceTypeConstraint))
		{
			diagnosticsBuilder.Add(GenerateConflictingConstraintsError(typeParameter, deducedBaseType, typeParameter.HasReferenceTypeConstraint));
		}
	}

	public static void CheckAllConstraints(this TypeSymbol type, ConversionsBase conversions, Location location, DiagnosticBag diagnostics)
	{
		type.VisitType(s_checkConstraintsSingleTypeFunc, new CheckConstraintsArgs(type.DeclaringCompilation, conversions, location, diagnostics));
	}

	public static bool CheckAllConstraints(this TypeSymbol type, ConversionsBase conversions)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		type.CheckAllConstraints(conversions, NoLocation.Singleton, instance);
		bool result = !instance.HasAnyErrors();
		instance.Free();
		return result;
	}

	private static bool CheckConstraintsSingleType(TypeSymbol type, CheckConstraintsArgs args)
	{
		if (type.Kind == SymbolKind.NamedType)
		{
			((NamedTypeSymbol)type).CheckConstraints(args.CurrentCompilation, args.Conversions, args.Location, args.Diagnostics);
		}
		return false;
	}

	public static void CheckConstraints(this TupleTypeSymbol tuple, ConversionsBase conversions, SyntaxNode typeSyntax, ImmutableArray<Location> elementLocations, Compilation currentCompilation, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol tupleUnderlyingType = tuple.TupleUnderlyingType;
		if (!RequiresChecking(tupleUnderlyingType) || typeSyntax.HasErrors)
		{
			return;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<NamedTypeSymbol> instance2 = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		TupleTypeSymbol.GetUnderlyingTypeChain(tupleUnderlyingType, instance2);
		int num = 0;
		foreach (NamedTypeSymbol item in instance2)
		{
			ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
			CheckTypeConstraints(item, conversions, currentCompilation, instance, ref useSiteDiagnosticsBuilder);
			if (useSiteDiagnosticsBuilder != null)
			{
				instance.AddRange(useSiteDiagnosticsBuilder);
			}
			foreach (TypeParameterDiagnosticInfo item2 in instance)
			{
				int ordinal = item2.TypeParameter.Ordinal;
				Location location = ((ordinal == 7) ? typeSyntax.Location : elementLocations[ordinal + num]);
				diagnostics.Add(new CSDiagnostic(item2.DiagnosticInfo, location));
			}
			instance.Clear();
			num += 7;
		}
		instance2.Free();
		instance.Free();
	}

	public static bool CheckConstraintsForNonTuple(this NamedTypeSymbol type, ConversionsBase conversions, SyntaxNode typeSyntax, SeparatedSyntaxList<TypeSyntax> typeArgumentsSyntax, Compilation currentCompilation, ConsList<Symbol> basesBeingResolved, DiagnosticBag diagnostics)
	{
		if (!RequiresChecking(type))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = !typeSyntax.HasErrors && CheckTypeConstraints(type, conversions, currentCompilation, instance, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			int ordinal = item.TypeParameter.Ordinal;
			SourceLocation location = new SourceLocation((ordinal < typeArgumentsSyntax.Count) ? typeArgumentsSyntax[ordinal] : typeSyntax);
			diagnostics.Add(new CSDiagnostic(item.DiagnosticInfo, location));
		}
		instance.Free();
		if (HasDuplicateInterfaces(type, basesBeingResolved))
		{
			result = false;
			diagnostics.Add(ErrorCode.ERR_BogusType, typeSyntax.Location, type);
		}
		return result;
	}

	public static bool CheckConstraints(this NamedTypeSymbol type, CSharpCompilation currentCompilation, ConversionsBase conversions, Location location, DiagnosticBag diagnostics)
	{
		type = (NamedTypeSymbol)type.TupleUnderlyingTypeOrSelf();
		if (!RequiresChecking(type))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = CheckTypeConstraints(type, conversions, currentCompilation, instance, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			diagnostics.Add(new CSDiagnostic(item.DiagnosticInfo, location));
		}
		instance.Free();
		if ((currentCompilation == null || !type.IsFromCompilation(currentCompilation)) && HasDuplicateInterfaces(type, null))
		{
			result = false;
			diagnostics.Add(ErrorCode.ERR_BogusType, location, type);
		}
		return result;
	}

	private static bool HasDuplicateInterfaces(NamedTypeSymbol type, ConsList<Symbol> basesBeingResolved)
	{
		ImmutableArray<NamedTypeSymbol> immutableArray = type.OriginalDefinition.InterfacesNoUseSiteDiagnostics(basesBeingResolved);
		switch (immutableArray.Length)
		{
		case 0:
		case 1:
			return false;
		case 2:
			if ((object)immutableArray[0].OriginalDefinition != immutableArray[1].OriginalDefinition)
			{
				return false;
			}
			break;
		default:
		{
			PooledHashSet<object> instance = PooledHashSet<object>.GetInstance();
			ImmutableArray<NamedTypeSymbol>.Enumerator enumerator = immutableArray.GetEnumerator();
			NamedTypeSymbol current;
			do
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
					continue;
				}
				instance.Free();
				return false;
			}
			while (instance.Add(current.OriginalDefinition));
			instance.Free();
			break;
		}
		}
		return type.InterfacesNoUseSiteDiagnostics(basesBeingResolved).HasDuplicates(TypeSymbol.EqualsIgnoringDynamicAndTupleNamesComparer);
	}

	public static bool CheckConstraints(this MethodSymbol method, ConversionsBase conversions, SyntaxNode syntaxNode, Compilation currentCompilation, DiagnosticBag diagnostics)
	{
		if (!RequiresChecking(method))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = CheckMethodConstraints(method, conversions, currentCompilation, instance, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			SourceLocation location = new SourceLocation(syntaxNode);
			diagnostics.Add(new CSDiagnostic(item.DiagnosticInfo, location));
		}
		instance.Free();
		return result;
	}

	public static bool CheckConstraints(this MethodSymbol method, ConversionsBase conversions, Location location, Compilation currentCompilation, DiagnosticBag diagnostics)
	{
		if (!RequiresChecking(method))
		{
			return true;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool result = CheckMethodConstraints(method, conversions, currentCompilation, instance, ref useSiteDiagnosticsBuilder);
		if (useSiteDiagnosticsBuilder != null)
		{
			instance.AddRange(useSiteDiagnosticsBuilder);
		}
		foreach (TypeParameterDiagnosticInfo item in instance)
		{
			diagnostics.Add(new CSDiagnostic(item.DiagnosticInfo, location));
		}
		instance.Free();
		return result;
	}

	private static bool CheckTypeConstraints(NamedTypeSymbol type, ConversionsBase conversions, Compilation currentCompilation, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder)
	{
		return type.CheckConstraints(conversions, type.TypeSubstitution, type.OriginalDefinition.TypeParameters, type.TypeArgumentsNoUseSiteDiagnostics, currentCompilation, diagnosticsBuilder, ref useSiteDiagnosticsBuilder);
	}

	public static bool CheckMethodConstraints(MethodSymbol method, ConversionsBase conversions, Compilation currentCompilation, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, BitVector skipParameters = default(BitVector))
	{
		return method.CheckConstraints(conversions, method.TypeSubstitution, method.OriginalDefinition.TypeParameters, method.TypeArguments, currentCompilation, diagnosticsBuilder, ref useSiteDiagnosticsBuilder, skipParameters);
	}

	public static bool CheckConstraints(this Symbol containingSymbol, ConversionsBase conversions, TypeMap substitution, ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<TypeSymbol> typeArguments, Compilation currentCompilation, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, BitVector skipParameters = default(BitVector), HashSet<TypeParameterSymbol> ignoreTypeConstraintsDependentOnTypeParametersOpt = null)
	{
		int length = typeParameters.Length;
		bool result = true;
		for (int i = 0; i < length; i++)
		{
			if (!skipParameters[i])
			{
				TypeSymbol typeArgument = typeArguments[i];
				TypeParameterSymbol typeParameter = typeParameters[i];
				if (!CheckConstraints(containingSymbol, conversions, substitution, typeParameter, typeArgument, currentCompilation, diagnosticsBuilder, ref useSiteDiagnosticsBuilder, ignoreTypeConstraintsDependentOnTypeParametersOpt))
				{
					result = false;
				}
			}
		}
		return result;
	}

	private static bool CheckConstraints(Symbol containingSymbol, ConversionsBase conversions, TypeMap substitution, TypeParameterSymbol typeParameter, TypeSymbol typeArgument, Compilation currentCompilation, ArrayBuilder<TypeParameterDiagnosticInfo> diagnosticsBuilder, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder, HashSet<TypeParameterSymbol> ignoreTypeConstraintsDependentOnTypeParametersOpt)
	{
		if (typeArgument.IsErrorType())
		{
			return true;
		}
		if (typeArgument.IsPointerType() || typeArgument.IsRestrictedType() || typeArgument.SpecialType == SpecialType.System_Void)
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_BadTypeArgument, typeArgument)));
			return false;
		}
		if (typeArgument.IsStatic)
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_GenericArgIsStaticClass, typeArgument)));
			return false;
		}
		if (typeParameter.HasReferenceTypeConstraint && !typeArgument.IsReferenceType)
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_RefConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument)));
			return false;
		}
		if (typeParameter.HasUnmanagedTypeConstraint && (typeArgument.IsManagedType || !typeArgument.IsNonNullableValueType()))
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_UnmanagedConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument)));
			return false;
		}
		if (typeParameter.HasValueTypeConstraint && !typeArgument.IsNonNullableValueType())
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_ValConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument)));
			return false;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		substitution.SubstituteTypesDistinctWithoutModifiers(typeParameter.ConstraintTypesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), instance, ignoreTypeConstraintsDependentOnTypeParametersOpt);
		bool flag = false;
		foreach (TypeSymbol item in instance)
		{
			if (!SatisfiesConstraintType(conversions, typeArgument, item, ref useSiteDiagnostics))
			{
				ErrorCode code;
				if (typeArgument.IsReferenceType)
				{
					code = ErrorCode.ERR_GenericConstraintNotSatisfiedRefType;
				}
				else if (typeArgument.IsNullableType())
				{
					code = (item.IsInterfaceType() ? ErrorCode.ERR_GenericConstraintNotSatisfiedNullableInterface : ErrorCode.ERR_GenericConstraintNotSatisfiedNullableEnum);
				}
				else
				{
					code = ((typeArgument.TypeKind != TypeKind.TypeParameter) ? ErrorCode.ERR_GenericConstraintNotSatisfiedValType : ErrorCode.ERR_GenericConstraintNotSatisfiedTyVar);
				}
				SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(currentCompilation, item, typeArgument);
				diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(code, containingSymbol.ConstructedFrom(), symbolDistinguisher.First, typeParameter, symbolDistinguisher.Second)));
				flag = true;
			}
		}
		if (AppendUseSiteDiagnostics(useSiteDiagnostics, typeParameter, ref useSiteDiagnosticsBuilder))
		{
			flag = true;
		}
		instance.Free();
		if (typeParameter.HasConstructorConstraint && !SatisfiesConstructorConstraint(typeArgument))
		{
			diagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_NewConstraintNotSatisfied, containingSymbol.ConstructedFrom(), typeParameter, typeArgument)));
			return false;
		}
		return !flag;
	}

	private static bool AppendUseSiteDiagnostics(HashSet<DiagnosticInfo> useSiteDiagnostics, TypeParameterSymbol typeParameter, ref ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder)
	{
		if (useSiteDiagnostics.IsNullOrEmpty())
		{
			return false;
		}
		if (useSiteDiagnosticsBuilder == null)
		{
			useSiteDiagnosticsBuilder = new ArrayBuilder<TypeParameterDiagnosticInfo>();
		}
		bool result = false;
		foreach (DiagnosticInfo useSiteDiagnostic in useSiteDiagnostics)
		{
			if (useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
			{
				result = true;
			}
			useSiteDiagnosticsBuilder.Add(new TypeParameterDiagnosticInfo(typeParameter, useSiteDiagnostic));
		}
		return result;
	}

	private static bool SatisfiesConstraintType(ConversionsBase conversions, TypeSymbol typeArgument, TypeSymbol constraintType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (constraintType.IsErrorType())
		{
			return false;
		}
		if (conversions.HasIdentityOrImplicitReferenceConversion(typeArgument, constraintType, ref useSiteDiagnostics))
		{
			return true;
		}
		if (typeArgument.IsValueType && conversions.HasBoxingConversion(typeArgument.IsNullableType() ? ((NamedTypeSymbol)typeArgument).ConstructedFrom : typeArgument, constraintType, ref useSiteDiagnostics))
		{
			return true;
		}
		if (typeArgument.TypeKind == TypeKind.TypeParameter)
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)typeArgument;
			if (conversions.HasImplicitTypeParameterConversion(typeParameterSymbol, constraintType, ref useSiteDiagnostics))
			{
				return true;
			}
			foreach (TypeSymbol item in typeParameterSymbol.ConstraintTypesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics))
			{
				if (SatisfiesConstraintType(conversions, item, constraintType, ref useSiteDiagnostics))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsReferenceType(TypeParameterSymbol typeParameter, ImmutableArray<TypeSymbol> constraintTypes)
	{
		if (!typeParameter.HasReferenceTypeConstraint)
		{
			return TypeParameterSymbol.IsReferenceTypeFromConstraintTypes(constraintTypes);
		}
		return true;
	}

	private static bool IsValueType(TypeParameterSymbol typeParameter, ImmutableArray<TypeSymbol> constraintTypes)
	{
		if (!typeParameter.HasValueTypeConstraint)
		{
			return TypeParameterSymbol.IsValueTypeFromConstraintTypes(constraintTypes);
		}
		return true;
	}

	private static TypeParameterDiagnosticInfo GenerateConflictingConstraintsError(TypeParameterSymbol typeParameter, TypeSymbol deducedBase, bool classConflict)
	{
		return new TypeParameterDiagnosticInfo(typeParameter, new CSDiagnosticInfo(ErrorCode.ERR_BaseConstraintConflict, typeParameter, deducedBase, classConflict ? "class" : "struct"));
	}

	private static void AddInterfaces(ArrayBuilder<NamedTypeSymbol> builder, ImmutableArray<NamedTypeSymbol> interfaces)
	{
		foreach (NamedTypeSymbol item in interfaces)
		{
			AddInterface(builder, item);
		}
	}

	private static void AddInterface(ArrayBuilder<NamedTypeSymbol> builder, NamedTypeSymbol @interface)
	{
		if (!builder.Contains(@interface))
		{
			builder.Add(@interface);
		}
	}

	private static bool SatisfiesConstructorConstraint(TypeSymbol typeArgument)
	{
		switch (typeArgument.TypeKind)
		{
		case TypeKind.Dynamic:
		case TypeKind.Enum:
		case TypeKind.Struct:
			return true;
		case TypeKind.Class:
			if (HasPublicParameterlessConstructor((NamedTypeSymbol)typeArgument))
			{
				return !typeArgument.IsAbstract;
			}
			return false;
		case TypeKind.TypeParameter:
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)typeArgument;
			if (!typeParameterSymbol.HasConstructorConstraint)
			{
				return typeParameterSymbol.IsValueType;
			}
			return true;
		}
		case TypeKind.Submission:
			throw ExceptionUtilities.UnexpectedValue(typeArgument.TypeKind);
		default:
			return false;
		}
	}

	private static bool HasPublicParameterlessConstructor(NamedTypeSymbol type)
	{
		foreach (MethodSymbol instanceConstructor in type.InstanceConstructors)
		{
			if (instanceConstructor.ParameterCount == 0)
			{
				return instanceConstructor.DeclaredAccessibility == Accessibility.Public;
			}
		}
		return false;
	}

	private static bool IsEncompassedBy(ConversionsBase conversions, TypeSymbol a, TypeSymbol b, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!conversions.HasIdentityOrImplicitReferenceConversion(a, b, ref useSiteDiagnostics))
		{
			return conversions.HasBoxingConversion(a, b, ref useSiteDiagnostics);
		}
		return true;
	}

	private static bool IsValidEncompassedByArgument(TypeSymbol type)
	{
		TypeKind typeKind = type.TypeKind;
		if (typeKind - 1 <= TypeKind.Class || typeKind == TypeKind.Enum || typeKind == TypeKind.Struct)
		{
			return true;
		}
		return false;
	}

	private static bool RequiresChecking(NamedTypeSymbol type)
	{
		if (type.Arity == 0)
		{
			return false;
		}
		if ((object)type.OriginalDefinition == type)
		{
			return false;
		}
		return true;
	}

	private static bool RequiresChecking(MethodSymbol method)
	{
		if (!method.IsGenericMethod)
		{
			return false;
		}
		if ((object)method.OriginalDefinition == method)
		{
			return false;
		}
		return true;
	}

	[Conditional("DEBUG")]
	private static void CheckEffectiveAndDeducedBaseTypes(ConversionsBase conversions, TypeSymbol effectiveBase, TypeSymbol deducedBase)
	{
	}
}
