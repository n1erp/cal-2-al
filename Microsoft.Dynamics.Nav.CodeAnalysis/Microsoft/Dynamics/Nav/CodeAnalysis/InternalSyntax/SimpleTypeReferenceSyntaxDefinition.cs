using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SimpleTypeReferenceSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<SimpleTypeReferenceSyntaxDefinition> Instance = new Lazy<SimpleTypeReferenceSyntaxDefinition>(() => new SimpleTypeReferenceSyntaxDefinition());

	public SimpleTypeReferenceSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ArraySyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Array",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = DataTypeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "DataType",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
