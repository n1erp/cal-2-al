using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class MethodSymbolExtensions
{
	public static bool IsParams(this MethodSymbol method)
	{
		if (method.ParameterCount != 0)
		{
			return method.Parameters[method.ParameterCount - 1].IsParams;
		}
		return false;
	}

	public static MethodSymbol InferExtensionMethodTypeArguments(this MethodSymbol method, TypeSymbol thisType, Compilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!method.IsGenericMethod || method != method.ConstructedFrom)
		{
			return method;
		}
		if (thisType.IsDynamic())
		{
			return null;
		}
		AssemblySymbol containingAssembly = method.ContainingAssembly;
		NamespaceSymbol globalNamespace = containingAssembly.GlobalNamespace;
		TypeConversions conversions = new TypeConversions(containingAssembly.CorLibrary);
		CSharpSyntaxNode syntax = (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
		BoundLiteral boundLiteral = new BoundLiteral(syntax, ConstantValue.Bad, thisType)
		{
			WasCompilerGenerated = true
		};
		BoundLiteral boundLiteral2 = new BoundLiteral(type: new ExtendedErrorTypeSymbol(globalNamespace, string.Empty, 0, null), syntax: syntax, constantValueOpt: ConstantValue.Bad)
		{
			WasCompilerGenerated = true
		};
		int parameterCount = method.ParameterCount;
		BoundExpression[] array = new BoundExpression[parameterCount];
		for (int i = 0; i < parameterCount; i++)
		{
			BoundLiteral boundLiteral3 = ((i == 0) ? boundLiteral : boundLiteral2);
			array[i] = boundLiteral3;
		}
		ImmutableArray<TypeSymbol> immutableArray = MethodTypeInferrer.InferTypeArgumentsFromFirstArgument(conversions, method, array.AsImmutable(), ref useSiteDiagnostics);
		if (immutableArray.IsDefault)
		{
			return null;
		}
		int num = -1;
		PooledHashSet<TypeParameterSymbol> instance = PooledHashSet<TypeParameterSymbol>.GetInstance();
		ImmutableArray<TypeParameterSymbol> typeParameters = method.TypeParameters;
		ImmutableArray<TypeSymbol> immutableArray2 = immutableArray;
		for (int j = 0; j < immutableArray2.Length; j++)
		{
			if ((object)immutableArray2[j] != null)
			{
				continue;
			}
			num = j;
			ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
			instance2.AddRange(immutableArray, num);
			for (; j < immutableArray2.Length; j++)
			{
				TypeSymbol typeSymbol = immutableArray2[j];
				if ((object)typeSymbol == null)
				{
					instance.Add(typeParameters[j]);
					instance2.Add(ErrorTypeSymbol.UnknownResultType);
				}
				else
				{
					instance2.Add(typeSymbol);
				}
			}
			immutableArray2 = instance2.ToImmutableAndFree();
			break;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance3 = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		TypeMap substitution = new TypeMap(typeParameters, immutableArray2.SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers));
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		bool flag = method.CheckConstraints(conversions, substitution, typeParameters, immutableArray2, compilation, instance3, ref useSiteDiagnosticsBuilder, default, (instance.Count > 0) ? instance : null);
		instance3.Free();
		instance.Free();
		if (useSiteDiagnosticsBuilder != null && useSiteDiagnosticsBuilder.Count > 0)
		{
			if (useSiteDiagnostics == null)
			{
				useSiteDiagnostics = new HashSet<DiagnosticInfo>();
			}
			foreach (TypeParameterDiagnosticInfo item in useSiteDiagnosticsBuilder)
			{
				useSiteDiagnostics.Add(item.DiagnosticInfo);
			}
		}
		if (!flag)
		{
			return null;
		}
		ImmutableArray<TypeSymbol> typeArguments = immutableArray;
		if (num != -1)
		{
			ArrayBuilder<TypeSymbol> instance4 = ArrayBuilder<TypeSymbol>.GetInstance();
			instance4.AddRange(immutableArray, num);
			for (int k = num; k < typeArguments.Length; k++)
			{
				instance4.Add(typeArguments[k] ?? typeParameters[k]);
			}
			typeArguments = instance4.ToImmutableAndFree();
		}
		return method.Construct(typeArguments);
	}

	internal static bool IsSynthesizedLambda(this MethodSymbol method)
	{
		if (method.IsImplicitlyDeclared)
		{
			return method.MethodKind == MethodKind.AnonymousFunction;
		}
		return false;
	}

	public static bool IsRuntimeFinalizer(this MethodSymbol method, bool skipFirstMethodKindCheck = false)
	{
		if ((object)method == null || method.Name != "Finalize" || method.ParameterCount != 0 || method.Arity != 0 || !method.IsMetadataVirtual(ignoreInterfaceImplementationChanges: true))
		{
			return false;
		}
		while ((object)method != null)
		{
			if (!skipFirstMethodKindCheck && method.MethodKind == MethodKind.Destructor)
			{
				return true;
			}
			if (method.ContainingType.SpecialType == SpecialType.System_Object)
			{
				return true;
			}
			if (method.IsMetadataNewSlot(ignoreInterfaceImplementationChanges: true))
			{
				return false;
			}
			method = method.GetFirstRuntimeOverriddenMethodIgnoringNewSlot(ignoreInterfaceImplementationChanges: true);
			skipFirstMethodKindCheck = false;
		}
		return false;
	}

	public static MethodSymbol ConstructIfGeneric(this MethodSymbol method, ImmutableArray<TypeSymbol> typeArguments)
	{
		if (!method.IsGenericMethod)
		{
			return method;
		}
		return method.Construct(typeArguments);
	}

	public static bool CanBeHiddenByMemberKind(this MethodSymbol hiddenMethod, SymbolKind hidingMemberKind)
	{
		if (hiddenMethod.MethodKind == MethodKind.Destructor)
		{
			return false;
		}
		switch (hidingMemberKind)
		{
		case SymbolKind.ErrorType:
		case SymbolKind.Method:
		case SymbolKind.NamedType:
		case SymbolKind.Property:
			return CanBeHiddenByMethodPropertyOrType(hiddenMethod);
		case SymbolKind.Event:
		case SymbolKind.Field:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(hidingMemberKind);
		}
	}

	private static bool CanBeHiddenByMethodPropertyOrType(MethodSymbol method)
	{
		switch (method.MethodKind)
		{
		case MethodKind.Constructor:
		case MethodKind.Conversion:
		case MethodKind.Destructor:
		case MethodKind.UserDefinedOperator:
		case MethodKind.StaticConstructor:
			return false;
		case MethodKind.EventAdd:
		case MethodKind.EventRemove:
		case MethodKind.PropertyGet:
		case MethodKind.PropertySet:
			return method.IsIndexedPropertyAccessor();
		default:
			return true;
		}
	}

	public static bool IsVoidReturningAsync(this MethodSymbol method)
	{
		if (method.IsAsync)
		{
			return method.ReturnsVoid;
		}
		return false;
	}

	public static bool IsTaskReturningAsync(this MethodSymbol method, CSharpCompilation compilation)
	{
		if (method.IsAsync)
		{
			return method.ReturnType.IsNonGenericTaskType(compilation);
		}
		return false;
	}

	public static bool IsGenericTaskReturningAsync(this MethodSymbol method, CSharpCompilation compilation)
	{
		if (method.IsAsync)
		{
			return method.ReturnType.IsGenericTaskType(compilation);
		}
		return false;
	}

	internal static CSharpSyntaxNode ExtractReturnTypeSyntax(this MethodSymbol method)
	{
		method = method.PartialDefinitionPart ?? method;
		foreach (SyntaxReference declaringSyntaxReference in method.DeclaringSyntaxReferences)
		{
			if (declaringSyntaxReference.GetSyntax() is MethodDeclarationSyntax methodDeclarationSyntax)
			{
				return methodDeclarationSyntax.ReturnType;
			}
		}
		return (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
	}
}
