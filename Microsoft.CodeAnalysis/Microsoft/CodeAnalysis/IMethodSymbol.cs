using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

public interface IMethodSymbol : ISymbol, IEquatable<ISymbol>
{
	MethodKind MethodKind { get; }

	int Arity { get; }

	bool IsGenericMethod { get; }

	bool IsExtensionMethod { get; }

	bool IsAsync { get; }

	bool IsVararg { get; }

	bool IsCheckedBuiltin { get; }

	bool HidesBaseMethodsByName { get; }

	bool ReturnsVoid { get; }

	bool ReturnsByRef { get; }

	bool ReturnsByRefReadonly { get; }

	RefKind RefKind { get; }

	ITypeSymbol ReturnType { get; }

	ImmutableArray<ITypeSymbol> TypeArguments { get; }

	ImmutableArray<ITypeParameterSymbol> TypeParameters { get; }

	ImmutableArray<IParameterSymbol> Parameters { get; }

	IMethodSymbol ConstructedFrom { get; }

	new IMethodSymbol OriginalDefinition { get; }

	IMethodSymbol OverriddenMethod { get; }

	ITypeSymbol ReceiverType { get; }

	IMethodSymbol ReducedFrom { get; }

	ImmutableArray<IMethodSymbol> ExplicitInterfaceImplementations { get; }

	ImmutableArray<CustomModifier> ReturnTypeCustomModifiers { get; }

	ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	ISymbol AssociatedSymbol { get; }

	IMethodSymbol PartialDefinitionPart { get; }

	IMethodSymbol PartialImplementationPart { get; }

	INamedTypeSymbol AssociatedAnonymousDelegate { get; }

	ITypeSymbol GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter);

	IMethodSymbol ReduceExtensionMethod(ITypeSymbol receiverType);

	ImmutableArray<AttributeData> GetReturnTypeAttributes();

	IMethodSymbol Construct(params ITypeSymbol[] typeArguments);

	DllImportData GetDllImportData();
}
