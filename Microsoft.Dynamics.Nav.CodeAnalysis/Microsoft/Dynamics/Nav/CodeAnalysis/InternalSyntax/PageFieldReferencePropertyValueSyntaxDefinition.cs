using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PageFieldReferencePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PageFieldReferencePropertyValueSyntaxDefinition> Instance = new Lazy<PageFieldReferencePropertyValueSyntaxDefinition>(() => new PageFieldReferencePropertyValueSyntaxDefinition());

	public PageFieldReferencePropertyValueSyntaxDefinition()
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
