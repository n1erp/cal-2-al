using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class XmlPortFieldNodeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<XmlPortFieldNodeSyntaxDefinition> Instance = new Lazy<XmlPortFieldNodeSyntaxDefinition>(() => new XmlPortFieldNodeSyntaxDefinition());

	public XmlPortFieldNodeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = XmlPortFieldElementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = XmlPortFieldAttributeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
