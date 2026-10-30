namespace Microsoft.Cci;

internal interface INamespaceTypeDefinition : INamedTypeDefinition, ITypeDefinition, IDefinition, IReference, ITypeReference, INamedTypeReference, INamedEntity, INamespaceTypeReference
{
	bool IsPublic { get; }
}
