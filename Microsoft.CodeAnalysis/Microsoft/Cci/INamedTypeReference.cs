namespace Microsoft.Cci;

internal interface INamedTypeReference : ITypeReference, IReference, INamedEntity
{
	ushort GenericParameterCount { get; }

	bool MangleName { get; }
}
