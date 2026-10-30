using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OptionAccessAttributeArgumentSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OptionAccessAttributeArgumentSyntaxDefinition> Instance = new Lazy<OptionAccessAttributeArgumentSyntaxDefinition>(() => new OptionAccessAttributeArgumentSyntaxDefinition());

	public OptionAccessAttributeArgumentSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = OptionAccessExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "OptionAccess",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
