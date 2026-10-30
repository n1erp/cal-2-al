namespace Microsoft.Cci;

internal interface IGenericMethodParameterReference : IGenericParameterReference, ITypeReference, IReference, INamedEntity, IParameterListEntry
{
	IMethodReference DefiningMethod { get; }
}
