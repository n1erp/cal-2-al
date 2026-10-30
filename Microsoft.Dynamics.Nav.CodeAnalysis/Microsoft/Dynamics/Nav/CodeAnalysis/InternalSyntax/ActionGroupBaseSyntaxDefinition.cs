using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ActionGroupBaseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ActionGroupBaseSyntaxDefinition> Instance = new Lazy<ActionGroupBaseSyntaxDefinition>(() => new ActionGroupBaseSyntaxDefinition());

	public ActionGroupBaseSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = PageActionGroupSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageActionAreaSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
