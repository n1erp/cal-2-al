using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class OptionAccessExpressionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<OptionAccessExpressionSyntaxDefinition> Instance = new Lazy<OptionAccessExpressionSyntaxDefinition>(() => new OptionAccessExpressionSyntaxDefinition());

	public OptionAccessExpressionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ColonColonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ColonColonToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
