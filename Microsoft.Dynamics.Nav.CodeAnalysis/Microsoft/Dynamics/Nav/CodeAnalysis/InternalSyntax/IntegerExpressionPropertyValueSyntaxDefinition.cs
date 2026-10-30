using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class IntegerExpressionPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<IntegerExpressionPropertyValueSyntaxDefinition> Instance = new Lazy<IntegerExpressionPropertyValueSyntaxDefinition>(() => new IntegerExpressionPropertyValueSyntaxDefinition());

	public IntegerExpressionPropertyValueSyntaxDefinition()
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
