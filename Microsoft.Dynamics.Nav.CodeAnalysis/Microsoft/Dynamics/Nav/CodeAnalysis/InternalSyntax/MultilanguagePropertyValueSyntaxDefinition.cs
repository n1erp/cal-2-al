using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MultilanguagePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MultilanguagePropertyValueSyntaxDefinition> Instance = new Lazy<MultilanguagePropertyValueSyntaxDefinition>(() => new MultilanguagePropertyValueSyntaxDefinition());

	public MultilanguagePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedIdentifierEqualsStringListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
