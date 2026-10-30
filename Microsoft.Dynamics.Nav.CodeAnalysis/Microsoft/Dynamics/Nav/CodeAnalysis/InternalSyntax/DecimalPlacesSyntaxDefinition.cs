using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DecimalPlacesSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DecimalPlacesSyntaxDefinition> Instance = new Lazy<DecimalPlacesSyntaxDefinition>(() => new DecimalPlacesSyntaxDefinition());

	public DecimalPlacesSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "MinimumToken",
			Kinds = ImmutableArray.Create(SyntaxKind.Int32LiteralToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "ColonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ColonToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "MaximumToken",
			Kinds = ImmutableArray.Create(SyntaxKind.Int32LiteralToken)
		}));
	}
}
