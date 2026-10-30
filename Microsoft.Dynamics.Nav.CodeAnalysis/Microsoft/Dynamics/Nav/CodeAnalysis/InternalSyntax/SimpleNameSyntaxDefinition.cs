using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SimpleNameSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<SimpleNameSyntaxDefinition> Instance = new Lazy<SimpleNameSyntaxDefinition>(() => new SimpleNameSyntaxDefinition());

	public SimpleNameSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
