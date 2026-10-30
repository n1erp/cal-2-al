using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ImagePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ImagePropertyValueSyntaxDefinition> Instance = new Lazy<ImagePropertyValueSyntaxDefinition>(() => new ImagePropertyValueSyntaxDefinition());

	public ImagePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
