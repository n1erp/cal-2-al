using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BlockSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BlockSyntaxDefinition> Instance = new Lazy<BlockSyntaxDefinition>(() => new BlockSyntaxDefinition());

	public BlockSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "BeginKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.BeginKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = StatementSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Statements",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EndKeywordToken",
			Kinds = ImmutableArray.Create(SyntaxKind.EndKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}));
	}
}
