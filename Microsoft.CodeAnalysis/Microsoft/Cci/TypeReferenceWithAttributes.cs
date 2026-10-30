using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Microsoft.Cci;

internal struct TypeReferenceWithAttributes
{
	public ITypeReference TypeRef { get; }

	public ImmutableArray<ICustomAttribute> Attributes { get; }

	public TypeReferenceWithAttributes(ITypeReference typeRef, ImmutableArray<ICustomAttribute> attributes = default(ImmutableArray<ICustomAttribute>))
	{
		TypeRef = typeRef;
		Attributes = attributes.NullToEmpty();
	}
}
