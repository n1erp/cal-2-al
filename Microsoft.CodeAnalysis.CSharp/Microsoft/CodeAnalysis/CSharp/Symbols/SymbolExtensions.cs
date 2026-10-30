using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class SymbolExtensions
{
	private static readonly Func<TypeSymbol, Symbol, bool, bool> s_hasInvalidTypeParameterFunc = (TypeSymbol type, Symbol containingSymbol, bool unused) => HasInvalidTypeParameter(type, containingSymbol);

	internal static bool HasParamsParameter(this Symbol member)
	{
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		if (!parameters.IsEmpty)
		{
			return parameters.Last().IsParams;
		}
		return false;
	}

	internal static ImmutableArray<ParameterSymbol> GetParameters(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).Parameters, 
			SymbolKind.Property => ((PropertySymbol)member).Parameters, 
			SymbolKind.Event => ImmutableArray<ParameterSymbol>.Empty, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static ImmutableArray<TypeSymbol> GetParameterTypes(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ParameterTypes, 
			SymbolKind.Property => ((PropertySymbol)member).ParameterTypes, 
			SymbolKind.Event => ImmutableArray<TypeSymbol>.Empty, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static bool GetIsVararg(this Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)member).IsVararg;
		case SymbolKind.Event:
		case SymbolKind.Property:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	internal static ImmutableArray<RefKind> GetParameterRefKinds(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ParameterRefKinds, 
			SymbolKind.Property => ((PropertySymbol)member).ParameterRefKinds, 
			SymbolKind.Event => default, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static int GetParameterCount(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ParameterCount, 
			SymbolKind.Property => ((PropertySymbol)member).ParameterCount, 
			SymbolKind.Event => 0, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static bool HasUnsafeParameter(this Symbol member)
	{
		foreach (TypeSymbol parameterType in member.GetParameterTypes())
		{
			if (parameterType.IsUnsafe())
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAccessor(this MethodSymbol methodSymbol)
	{
		return (object)methodSymbol.AssociatedSymbol != null;
	}

	public static bool IsAccessor(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)symbol).IsAccessor();
		}
		return false;
	}

	public static bool IsIndexedPropertyAccessor(this MethodSymbol methodSymbol)
	{
		return methodSymbol.AssociatedSymbol?.IsIndexedProperty() ?? false;
	}

	public static bool IsOperator(this MethodSymbol methodSymbol)
	{
		if (methodSymbol.MethodKind != MethodKind.UserDefinedOperator)
		{
			return methodSymbol.MethodKind == MethodKind.Conversion;
		}
		return true;
	}

	public static bool IsOperator(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)symbol).IsOperator();
		}
		return false;
	}

	public static bool IsIndexer(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Property)
		{
			return ((PropertySymbol)symbol).IsIndexer;
		}
		return false;
	}

	public static bool IsIndexedProperty(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Property)
		{
			return ((PropertySymbol)symbol).IsIndexedProperty;
		}
		return false;
	}

	public static bool IsUserDefinedConversion(this Symbol symbol)
	{
		if (symbol.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)symbol).MethodKind == MethodKind.Conversion;
		}
		return false;
	}

	public static int CustomModifierCount(this MethodSymbol method)
	{
		int num = 0;
		num += method.ReturnTypeCustomModifiers.Length + method.RefCustomModifiers.Length;
		num += method.ReturnType.CustomModifierCount();
		foreach (ParameterSymbol parameter in method.Parameters)
		{
			num += parameter.CustomModifiers.Length + parameter.RefCustomModifiers.Length;
			num += parameter.Type.CustomModifierCount();
		}
		return num;
	}

	public static int CustomModifierCount(this Symbol m)
	{
		switch (m.Kind)
		{
		case SymbolKind.ArrayType:
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		case SymbolKind.PointerType:
		case SymbolKind.TypeParameter:
			return ((TypeSymbol)m).CustomModifierCount();
		case SymbolKind.Event:
			return ((EventSymbol)m).CustomModifierCount();
		case SymbolKind.Method:
			return ((MethodSymbol)m).CustomModifierCount();
		case SymbolKind.Property:
			return ((PropertySymbol)m).CustomModifierCount();
		default:
			throw ExceptionUtilities.UnexpectedValue(m.Kind);
		}
	}

	public static int CustomModifierCount(this EventSymbol e)
	{
		return e.Type.CustomModifierCount();
	}

	public static int CustomModifierCount(this PropertySymbol property)
	{
		int num = 0;
		num += property.TypeCustomModifiers.Length + property.RefCustomModifiers.Length;
		num += property.Type.CustomModifierCount();
		foreach (ParameterSymbol parameter in property.Parameters)
		{
			num += parameter.CustomModifiers.Length + parameter.RefCustomModifiers.Length;
			num += parameter.Type.CustomModifierCount();
		}
		return num;
	}

	internal static Symbol SymbolAsMember(this Symbol s, NamedTypeSymbol newOwner)
	{
		return s.Kind switch
		{
			SymbolKind.Field => (Symbol)((FieldSymbol)s).AsMember(newOwner), 
			SymbolKind.Method => ((MethodSymbol)s).AsMember(newOwner), 
			SymbolKind.NamedType => ((NamedTypeSymbol)s).AsMember(newOwner), 
			SymbolKind.Property => ((PropertySymbol)s).AsMember(newOwner), 
			SymbolKind.Event => ((EventSymbol)s).AsMember(newOwner), 
			_ => throw ExceptionUtilities.UnexpectedValue(s.Kind), 
		};
	}

	internal static int GetMemberArity(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).Arity;
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).Arity;
		default:
			return 0;
		}
	}

	internal static NamespaceOrTypeSymbol OfMinimalArity(this IEnumerable<NamespaceOrTypeSymbol> symbols)
	{
		NamespaceOrTypeSymbol result = null;
		int num = int.MaxValue;
		foreach (NamespaceOrTypeSymbol symbol in symbols)
		{
			int memberArity = symbol.GetMemberArity();
			if (memberArity < num)
			{
				num = memberArity;
				result = symbol;
			}
		}
		return result;
	}

	internal static ImmutableArray<TypeParameterSymbol> GetMemberTypeParameters(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).TypeParameters;
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).TypeParameters;
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
			return ImmutableArray<TypeParameterSymbol>.Empty;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	internal static ImmutableArray<TypeSymbol> GetMemberTypeArgumentsNoUseSiteDiagnostics(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).TypeArguments;
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).TypeArgumentsNoUseSiteDiagnostics;
		case SymbolKind.Event:
		case SymbolKind.Field:
		case SymbolKind.Property:
			return ImmutableArray<TypeSymbol>.Empty;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	internal static bool IsParameterlessConstructor(this MethodSymbol method)
	{
		if (method.MethodKind == MethodKind.Constructor)
		{
			return method.ParameterCount == 0;
		}
		return false;
	}

	internal static bool IsDefaultValueTypeConstructor(this MethodSymbol method)
	{
		if (method.IsImplicitlyDeclared && method.ContainingType.IsValueType)
		{
			return method.IsParameterlessConstructor();
		}
		return false;
	}

	internal static MethodSymbol GetOwnOrInheritedAddMethod(this EventSymbol @event)
	{
		while ((object)@event != null)
		{
			MethodSymbol addMethod = @event.AddMethod;
			if ((object)addMethod != null)
			{
				return addMethod;
			}
			@event = (@event.IsOverride ? @event.OverriddenEvent : null);
		}
		return null;
	}

	internal static MethodSymbol GetOwnOrInheritedRemoveMethod(this EventSymbol @event)
	{
		while ((object)@event != null)
		{
			MethodSymbol removeMethod = @event.RemoveMethod;
			if ((object)removeMethod != null)
			{
				return removeMethod;
			}
			@event = (@event.IsOverride ? @event.OverriddenEvent : null);
		}
		return null;
	}

	internal static bool IsExplicitInterfaceImplementation(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).IsExplicitInterfaceImplementation, 
			SymbolKind.Property => ((PropertySymbol)member).IsExplicitInterfaceImplementation, 
			SymbolKind.Event => ((EventSymbol)member).IsExplicitInterfaceImplementation, 
			_ => false, 
		};
	}

	internal static bool IsPartialMethod(this Symbol member)
	{
		return (member as SourceMemberMethodSymbol)?.IsPartial ?? false;
	}

	internal static bool IsPartialImplementation(this Symbol member)
	{
		return (member as SourceOrdinaryMethodSymbol)?.IsPartialImplementation ?? false;
	}

	internal static bool IsPartialDefinition(this Symbol member)
	{
		return (member as SourceOrdinaryMethodSymbol)?.IsPartialDefinition ?? false;
	}

	internal static bool ContainsTupleNames(this Symbol member)
	{
		switch (member.Kind)
		{
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (!methodSymbol.ReturnType.ContainsTupleNames())
			{
				return methodSymbol.Parameters.Any((ParameterSymbol p) => p.Type.ContainsTupleNames());
			}
			return true;
		}
		case SymbolKind.Property:
			return ((PropertySymbol)member).Type.ContainsTupleNames();
		case SymbolKind.Event:
			return ((EventSymbol)member).Type.ContainsTupleNames();
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		}
	}

	internal static ImmutableArray<Symbol> GetExplicitInterfaceImplementations(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).ExplicitInterfaceImplementations.Cast<MethodSymbol, Symbol>(), 
			SymbolKind.Property => ((PropertySymbol)member).ExplicitInterfaceImplementations.Cast<PropertySymbol, Symbol>(), 
			SymbolKind.Event => ((EventSymbol)member).ExplicitInterfaceImplementations.Cast<EventSymbol, Symbol>(), 
			_ => ImmutableArray<Symbol>.Empty, 
		};
	}

	internal static Symbol GetOverriddenMember(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => (Symbol)((MethodSymbol)member).OverriddenMethod, 
			SymbolKind.Property => ((PropertySymbol)member).OverriddenProperty, 
			SymbolKind.Event => ((EventSymbol)member).OverriddenEvent, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static Symbol GetLeastOverriddenMember(this Symbol member, NamedTypeSymbol accessingTypeOpt)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).GetConstructedLeastOverriddenMethod(accessingTypeOpt), 
			SymbolKind.Property => ((PropertySymbol)member).GetLeastOverriddenProperty(accessingTypeOpt), 
			SymbolKind.Event => ((EventSymbol)member).GetLeastOverriddenEvent(accessingTypeOpt), 
			_ => member, 
		};
	}

	internal static bool IsFieldOrFieldLikeEvent(this Symbol member, out FieldSymbol field)
	{
		switch (member.Kind)
		{
		case SymbolKind.Field:
			field = (FieldSymbol)member;
			return true;
		case SymbolKind.Event:
			field = ((EventSymbol)member).AssociatedField;
			return (object)field != null;
		default:
			field = null;
			return false;
		}
	}

	internal static string GetMemberCallerName(this Symbol member)
	{
		if (member.Kind == SymbolKind.Method)
		{
			member = ((MethodSymbol)member).AssociatedSymbol ?? member;
		}
		if (!member.IsIndexer())
		{
			if (!member.IsExplicitInterfaceImplementation())
			{
				return member.Name;
			}
			return ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(member.Name);
		}
		return member.MetadataName;
	}

	public static bool IsCompilationOutputWinMdObj(this Symbol symbol)
	{
		CSharpCompilation declaringCompilation = symbol.DeclaringCompilation;
		if (declaringCompilation != null)
		{
			return declaringCompilation.Options.OutputKind == OutputKind.WindowsRuntimeMetadata;
		}
		return false;
	}

	public static NamedTypeSymbol ConstructIfGeneric(this NamedTypeSymbol type, ImmutableArray<TypeWithModifiers> typeArguments)
	{
		if (!type.TypeParameters.IsEmpty)
		{
			return type.Construct(typeArguments, unbound: false);
		}
		return type;
	}

	public static bool IsNestedType(this Symbol symbol)
	{
		if (symbol is NamedTypeSymbol)
		{
			return (object)symbol.ContainingType != null;
		}
		return false;
	}

	public static bool Any<T>(this ImmutableArray<T> array, SymbolKind kind) where T : Symbol
	{
		int i = 0;
		for (int length = array.Length; i < length; i++)
		{
			T val = array[i];
			if ((object)val != null && val.Kind == kind)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsAccessibleViaInheritance(this TypeSymbol superType, TypeSymbol subType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol originalDefinition = superType.OriginalDefinition;
		TypeSymbol typeSymbol = subType;
		while ((object)typeSymbol != null)
		{
			if ((object)typeSymbol.OriginalDefinition == originalDefinition)
			{
				return true;
			}
			typeSymbol = ((typeSymbol.Kind == SymbolKind.TypeParameter) ? ((TypeParameterSymbol)typeSymbol).EffectiveBaseClass(ref useSiteDiagnostics) : typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics));
		}
		if (superType.TypeKind == TypeKind.Submission)
		{
			return subType.TypeKind == TypeKind.Submission;
		}
		return false;
	}

	public static bool IsNoMoreVisibleThan(this Symbol symbol, TypeSymbol type, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return type.IsAtLeastAsVisibleAs(symbol, ref useSiteDiagnostics);
	}

	public static LocalizableErrorArgument GetKindText(this Symbol symbol)
	{
		return symbol.Kind.Localize();
	}

	internal static NamespaceOrTypeSymbol ContainingNamespaceOrType(this Symbol symbol)
	{
		Symbol containingSymbol = symbol.ContainingSymbol;
		if ((object)containingSymbol != null)
		{
			SymbolKind kind = containingSymbol.Kind;
			if (kind == SymbolKind.ErrorType || (uint)(kind - 11) <= 1u)
			{
				return (NamespaceOrTypeSymbol)containingSymbol;
			}
		}
		return null;
	}

	internal static Symbol ContainingNonLambdaMember(this Symbol containingMember)
	{
		while ((object)containingMember != null && containingMember.Kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = (MethodSymbol)containingMember;
			if (methodSymbol.MethodKind != MethodKind.AnonymousFunction && methodSymbol.MethodKind != MethodKind.LocalFunction)
			{
				break;
			}
			containingMember = containingMember.ContainingSymbol;
		}
		return containingMember;
	}

	internal static ParameterSymbol EnclosingThisSymbol(this Symbol containingMember)
	{
		Symbol symbol = containingMember;
		NamedTypeSymbol namedTypeSymbol;
		while (true)
		{
			MethodSymbol methodSymbol;
			switch (symbol.Kind)
			{
			case SymbolKind.Method:
				methodSymbol = (MethodSymbol)symbol;
				if (methodSymbol.MethodKind == MethodKind.AnonymousFunction || methodSymbol.MethodKind == MethodKind.LocalFunction)
				{
					goto IL_0032;
				}
				return methodSymbol.ThisParameter;
			case SymbolKind.Field:
				namedTypeSymbol = symbol.ContainingType;
				break;
			case SymbolKind.NamedType:
				namedTypeSymbol = (NamedTypeSymbol)symbol;
				break;
			default:
				return null;
			}
			break;
			IL_0032:
			symbol = methodSymbol.ContainingSymbol;
		}
		if (!namedTypeSymbol.IsScriptClass)
		{
			return null;
		}
		return namedTypeSymbol.InstanceConstructors.Single().ThisParameter;
	}

	public static Symbol ConstructedFrom(this Symbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.NamedType => (Symbol)((NamedTypeSymbol)symbol).ConstructedFrom, 
			SymbolKind.Method => ((MethodSymbol)symbol).ConstructedFrom, 
			_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
		};
	}

	public static bool IsContainingSymbolOfAllTypeParameters(this Symbol containingSymbol, TypeSymbol type)
	{
		return (object)type.VisitType(s_hasInvalidTypeParameterFunc, containingSymbol) == null;
	}

	public static bool IsContainingSymbolOfAllTypeParameters(this Symbol containingSymbol, ImmutableArray<TypeSymbol> types)
	{
		return types.All(containingSymbol.IsContainingSymbolOfAllTypeParameters);
	}

	private static bool HasInvalidTypeParameter(TypeSymbol type, Symbol containingSymbol)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
		{
			Symbol containingSymbol2 = type.ContainingSymbol;
			while ((object)containingSymbol != null && containingSymbol.Kind != SymbolKind.Namespace)
			{
				if (containingSymbol == containingSymbol2)
				{
					return false;
				}
				containingSymbol = containingSymbol.ContainingSymbol;
			}
			return true;
		}
		return false;
	}

	public static bool IsTypeOrTypeAlias(this Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
		case SymbolKind.DynamicType:
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
		case SymbolKind.PointerType:
		case SymbolKind.TypeParameter:
			return true;
		case SymbolKind.Alias:
			return ((AliasSymbol)symbol).Target.IsTypeOrTypeAlias();
		default:
			return false;
		}
	}

	internal static bool CompilationAllowsUnsafe(this Symbol symbol)
	{
		return symbol.DeclaringCompilation.Options.AllowUnsafe;
	}

	internal static void CheckUnsafeModifier(this Symbol symbol, DeclarationModifiers modifiers, DiagnosticBag diagnostics)
	{
		symbol.CheckUnsafeModifier(modifiers, symbol.Locations[0], diagnostics);
	}

	internal static void CheckUnsafeModifier(this Symbol symbol, DeclarationModifiers modifiers, Location errorLocation, DiagnosticBag diagnostics)
	{
		if ((modifiers & DeclarationModifiers.Unsafe) == DeclarationModifiers.Unsafe && !symbol.CompilationAllowsUnsafe())
		{
			diagnostics.Add(ErrorCode.ERR_IllegalUnsafe, errorLocation);
		}
	}

	public static bool IsHiddenByCodeAnalysisEmbeddedAttribute(this Symbol symbol)
	{
		NamedTypeSymbol namedTypeSymbol = ((symbol.Kind == SymbolKind.NamedType) ? ((NamedTypeSymbol)symbol) : symbol.ContainingType);
		if ((object)namedTypeSymbol == null)
		{
			return false;
		}
		while ((object)namedTypeSymbol.ContainingType != null)
		{
			namedTypeSymbol = namedTypeSymbol.ContainingType;
		}
		return namedTypeSymbol.HasCodeAnalysisEmbeddedAttribute;
	}

	public static bool MustCallMethodsDirectly(this Symbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.Property => ((PropertySymbol)symbol).MustCallMethodsDirectly, 
			SymbolKind.Event => ((EventSymbol)symbol).MustCallMethodsDirectly, 
			_ => false, 
		};
	}

	public static int GetArity(this Symbol symbol)
	{
		if ((object)symbol != null)
		{
			switch (symbol.Kind)
			{
			case SymbolKind.NamedType:
				return ((NamedTypeSymbol)symbol).Arity;
			case SymbolKind.Method:
				return ((MethodSymbol)symbol).Arity;
			}
		}
		return 0;
	}

	internal static CSharpSyntaxNode GetNonNullSyntaxNode(this Symbol symbol)
	{
		if ((object)symbol != null)
		{
			SyntaxReference syntaxReference = symbol.DeclaringSyntaxReferences.FirstOrDefault();
			if (syntaxReference == null && symbol.IsImplicitlyDeclared)
			{
				Symbol containingSymbol = symbol.ContainingSymbol;
				if ((object)containingSymbol != null)
				{
					syntaxReference = containingSymbol.DeclaringSyntaxReferences.FirstOrDefault();
				}
			}
			if (syntaxReference != null)
			{
				return (CSharpSyntaxNode)syntaxReference.GetSyntax();
			}
		}
		return (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
	}

	internal static TDestination EnsureCSharpSymbolOrNull<TSource, TDestination>(this TSource symbol, string paramName) where TSource : ISymbol where TDestination : Symbol, TSource
	{
		TDestination val = symbol as TDestination;
		if ((object)val == null && symbol != null)
		{
			throw new ArgumentException(CSharpResources.NotACSharpSymbol, paramName);
		}
		return val;
	}

	internal static ImmutableArray<TypeSymbol> ToTypes(this ImmutableArray<TypeWithModifiers> typesWithModifiers, out bool hasModifiers)
	{
		hasModifiers = typesWithModifiers.Any((TypeWithModifiers a) => !a.CustomModifiers.IsDefaultOrEmpty);
		return typesWithModifiers.SelectAsArray((TypeWithModifiers a) => a.Type);
	}

	internal static TypeSymbol GetTypeOrReturnType(this Symbol symbol)
	{
		symbol.GetTypeOrReturnType(out var _, out var returnType, out ImmutableArray<CustomModifier> returnTypeCustomModifiers, out returnTypeCustomModifiers);
		return returnType;
	}

	internal static void GetTypeOrReturnType(this Symbol symbol, out RefKind refKind, out TypeSymbol returnType, out ImmutableArray<CustomModifier> returnTypeCustomModifiers, out ImmutableArray<CustomModifier> refCustomModifiers)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)symbol;
			refKind = RefKind.None;
			returnType = fieldSymbol.Type;
			returnTypeCustomModifiers = fieldSymbol.CustomModifiers;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		}
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)symbol;
			refKind = methodSymbol.RefKind;
			returnType = methodSymbol.ReturnType;
			returnTypeCustomModifiers = methodSymbol.ReturnTypeCustomModifiers;
			refCustomModifiers = methodSymbol.RefCustomModifiers;
			break;
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)symbol;
			refKind = propertySymbol.RefKind;
			returnType = propertySymbol.Type;
			returnTypeCustomModifiers = propertySymbol.TypeCustomModifiers;
			refCustomModifiers = propertySymbol.RefCustomModifiers;
			break;
		}
		case SymbolKind.Event:
		{
			EventSymbol eventSymbol = (EventSymbol)symbol;
			refKind = RefKind.None;
			returnType = eventSymbol.Type;
			returnTypeCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		}
		case SymbolKind.Local:
		{
			LocalSymbol localSymbol = (LocalSymbol)symbol;
			refKind = localSymbol.RefKind;
			returnType = localSymbol.Type;
			returnTypeCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}
}
