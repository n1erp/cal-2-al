using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class EmptyPropertySyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<EmptyPropertySyntaxDefinition> Instance = new Lazy<EmptyPropertySyntaxDefinition>(() => new EmptyPropertySyntaxDefinition());

	public EmptyPropertySyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}));
	}
}
