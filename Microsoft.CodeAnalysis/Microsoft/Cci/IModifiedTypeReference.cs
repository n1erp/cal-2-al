using System.Collections.Immutable;

namespace Microsoft.Cci;

internal interface IModifiedTypeReference : ITypeReference, IReference
{
	ImmutableArray<ICustomModifier> CustomModifiers { get; }

	ITypeReference UnmodifiedType { get; }
}
