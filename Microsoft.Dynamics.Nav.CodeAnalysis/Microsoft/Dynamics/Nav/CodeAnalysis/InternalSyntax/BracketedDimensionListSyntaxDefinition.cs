using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BracketedDimensionListSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BracketedDimensionListSyntaxDefinition> Instance = new Lazy<BracketedDimensionListSyntaxDefinition>(() => new BracketedDimensionListSyntaxDefinition());

	public BracketedDimensionListSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBracketToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = DimensionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Dimensions",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBracketToken)
		}));
	}
}
