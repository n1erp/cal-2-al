namespace Microsoft.Cci;

internal interface IGenericTypeParameterReference : IGenericParameterReference, ITypeReference, IReference, INamedEntity, IParameterListEntry
{
	ITypeReference DefiningType { get; }
}
