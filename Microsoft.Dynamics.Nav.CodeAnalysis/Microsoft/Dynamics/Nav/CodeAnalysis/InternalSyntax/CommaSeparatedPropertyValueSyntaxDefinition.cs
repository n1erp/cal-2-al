using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CommaSeparatedPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CommaSeparatedPropertyValueSyntaxDefinition> Instance = new Lazy<CommaSeparatedPropertyValueSyntaxDefinition>(() => new CommaSeparatedPropertyValueSyntaxDefinition());

	public CommaSeparatedPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Values",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
