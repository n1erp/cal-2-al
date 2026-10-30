using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PropertySyntaxOrEmptyDefinition : SyntaxNodeDefinition
{
	public static Lazy<PropertySyntaxOrEmptyDefinition> Instance = new Lazy<PropertySyntaxOrEmptyDefinition>(() => new PropertySyntaxOrEmptyDefinition());

	public PropertySyntaxOrEmptyDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = EmptyPropertySyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = PropertySyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
