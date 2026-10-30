using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ViewChangeBaseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ViewChangeBaseSyntaxDefinition> Instance = new Lazy<ViewChangeBaseSyntaxDefinition>(() => new ViewChangeBaseSyntaxDefinition());

	public ViewChangeBaseSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ViewAddChangeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = ViewMoveChangeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = ViewModifyChangeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
