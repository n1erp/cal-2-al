using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class BooleanExpressionPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<BooleanExpressionPropertyValueSyntaxDefinition> Instance = new Lazy<BooleanExpressionPropertyValueSyntaxDefinition>(() => new BooleanExpressionPropertyValueSyntaxDefinition());

	public BooleanExpressionPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
