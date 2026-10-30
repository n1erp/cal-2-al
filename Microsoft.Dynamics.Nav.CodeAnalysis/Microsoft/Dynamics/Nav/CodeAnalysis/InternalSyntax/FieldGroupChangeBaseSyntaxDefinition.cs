using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class FieldGroupChangeBaseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<FieldGroupChangeBaseSyntaxDefinition> Instance = new Lazy<FieldGroupChangeBaseSyntaxDefinition>(() => new FieldGroupChangeBaseSyntaxDefinition());

	public FieldGroupChangeBaseSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = FieldGroupAddChangeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
