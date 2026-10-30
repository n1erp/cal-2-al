using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class XmlPortTextNodeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<XmlPortTextNodeSyntaxDefinition> Instance = new Lazy<XmlPortTextNodeSyntaxDefinition>(() => new XmlPortTextNodeSyntaxDefinition());

	public XmlPortTextNodeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = XmlPortTextElementSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = XmlPortTextAttributeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
