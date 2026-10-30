namespace Microsoft.Cci;

internal interface ISpecializedFieldReference : IFieldReference, ITypeMemberReference, IReference, INamedEntity
{
	IFieldReference UnspecializedVersion { get; }
}
