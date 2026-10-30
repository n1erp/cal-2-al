using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CommaSeparatedStringsPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CommaSeparatedStringsPropertyValueSyntaxDefinition> Instance = new Lazy<CommaSeparatedStringsPropertyValueSyntaxDefinition>(() => new CommaSeparatedStringsPropertyValueSyntaxDefinition());

	public CommaSeparatedStringsPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = StringLiteralValueSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Values",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
