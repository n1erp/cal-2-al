using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class XmlPortSchemaSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<XmlPortSchemaSyntaxDefinition> Instance = new Lazy<XmlPortSchemaSyntaxDefinition>(() => new XmlPortSchemaSyntaxDefinition());

	public XmlPortSchemaSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "XmlPortSchemaKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.XmlPortSchemaKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = XmlPortNodeSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "XmlPortSchema",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBraceToken)
		}));
	}
}
