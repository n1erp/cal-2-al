using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LabelPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<LabelPropertyValueSyntaxDefinition> Instance = new Lazy<LabelPropertyValueSyntaxDefinition>(() => new LabelPropertyValueSyntaxDefinition());

	public LabelPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = LabelSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
