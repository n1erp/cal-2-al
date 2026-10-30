namespace Microsoft.Cci;

internal interface ITypeDefinitionMember : ITypeMemberReference, IReference, INamedEntity, IDefinition
{
	ITypeDefinition ContainingTypeDefinition { get; }

	TypeMemberVisibility Visibility { get; }
}
