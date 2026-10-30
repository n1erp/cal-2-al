using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ArraySyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ArraySyntaxDefinition> Instance = new Lazy<ArraySyntaxDefinition>(() => new ArraySyntaxDefinition());

	public ArraySyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ArrayKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.ArrayKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = BracketedDimensionListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "DimensionList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OfKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.OfKeyword)
		}));
	}
}
