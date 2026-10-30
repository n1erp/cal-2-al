using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LabelDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<LabelDataTypeSyntaxDefinition> Instance = new Lazy<LabelDataTypeSyntaxDefinition>(() => new LabelDataTypeSyntaxDefinition());

	public LabelDataTypeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TypeName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = LabelSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Label",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
