using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DimensionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DimensionSyntaxDefinition> Instance = new Lazy<DimensionSyntaxDefinition>(() => new DimensionSyntaxDefinition());

	public DimensionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create(SyntaxKind.Int32LiteralToken)
		}));
	}
}
