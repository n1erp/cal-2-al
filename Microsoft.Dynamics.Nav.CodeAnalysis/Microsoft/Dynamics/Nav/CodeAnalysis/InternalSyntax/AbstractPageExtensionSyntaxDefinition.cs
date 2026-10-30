using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class AbstractPageExtensionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<AbstractPageExtensionSyntaxDefinition> Instance = new Lazy<AbstractPageExtensionSyntaxDefinition>(() => new AbstractPageExtensionSyntaxDefinition());

	public AbstractPageExtensionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = PageExtensionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = PageCustomizationSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
