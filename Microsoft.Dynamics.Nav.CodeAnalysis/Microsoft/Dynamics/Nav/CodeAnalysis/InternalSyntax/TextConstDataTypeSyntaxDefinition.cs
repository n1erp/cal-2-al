using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TextConstDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TextConstDataTypeSyntaxDefinition> Instance = new Lazy<TextConstDataTypeSyntaxDefinition>(() => new TextConstDataTypeSyntaxDefinition());

	public TextConstDataTypeSyntaxDefinition()
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
			Node = CommaSeparatedIdentifierEqualsStringListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Multilanguage",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
