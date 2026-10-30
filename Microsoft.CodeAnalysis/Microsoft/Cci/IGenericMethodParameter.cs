namespace Microsoft.Cci;

internal interface IGenericMethodParameter : IGenericParameter, IGenericParameterReference, ITypeReference, IReference, INamedEntity, IParameterListEntry, IGenericMethodParameterReference
{
	new IMethodDefinition DefiningMethod { get; }
}
