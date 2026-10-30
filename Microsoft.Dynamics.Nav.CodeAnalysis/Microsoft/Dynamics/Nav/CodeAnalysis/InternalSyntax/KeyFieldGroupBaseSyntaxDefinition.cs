using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class KeyFieldGroupBaseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<KeyFieldGroupBaseSyntaxDefinition> Instance = new Lazy<KeyFieldGroupBaseSyntaxDefinition>(() => new KeyFieldGroupBaseSyntaxDefinition());

	public KeyFieldGroupBaseSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = KeySyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = FieldGroupSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
