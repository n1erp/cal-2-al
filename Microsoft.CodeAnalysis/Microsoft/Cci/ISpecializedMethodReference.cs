namespace Microsoft.Cci;

internal interface ISpecializedMethodReference : IMethodReference, ISignature, ITypeMemberReference, IReference, INamedEntity
{
	IMethodReference UnspecializedVersion { get; }
}
