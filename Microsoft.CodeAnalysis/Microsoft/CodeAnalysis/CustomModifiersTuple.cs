using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal sealed class CustomModifiersTuple
{
	private readonly ImmutableArray<CustomModifier> _typeCustomModifiers;

	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	public static readonly CustomModifiersTuple Empty = new CustomModifiersTuple(ImmutableArray<CustomModifier>.Empty, ImmutableArray<CustomModifier>.Empty);

	public ImmutableArray<CustomModifier> TypeCustomModifiers => _typeCustomModifiers;

	public ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	private CustomModifiersTuple(ImmutableArray<CustomModifier> typeCustomModifiers, ImmutableArray<CustomModifier> refCustomModifiers)
	{
		_typeCustomModifiers = typeCustomModifiers.NullToEmpty();
		_refCustomModifiers = refCustomModifiers.NullToEmpty();
	}

	public static CustomModifiersTuple Create(ImmutableArray<CustomModifier> typeCustomModifiers, ImmutableArray<CustomModifier> refCustomModifiers)
	{
		if (typeCustomModifiers.IsDefaultOrEmpty && refCustomModifiers.IsDefaultOrEmpty)
		{
			return Empty;
		}
		return new CustomModifiersTuple(typeCustomModifiers, refCustomModifiers);
	}
}
