using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DecimalPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DecimalPropertyValueSyntaxDefinition> Instance = new Lazy<DecimalPropertyValueSyntaxDefinition>(() => new DecimalPropertyValueSyntaxDefinition());

	public DecimalPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = DecimalSignedLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
