using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SimpleNamedDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<SimpleNamedDataTypeSyntaxDefinition> Instance = new Lazy<SimpleNamedDataTypeSyntaxDefinition>(() => new SimpleNamedDataTypeSyntaxDefinition());

	public SimpleNamedDataTypeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TypeName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
