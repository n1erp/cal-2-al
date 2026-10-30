using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TextExpressionPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TextExpressionPropertyValueSyntaxDefinition> Instance = new Lazy<TextExpressionPropertyValueSyntaxDefinition>(() => new TextExpressionPropertyValueSyntaxDefinition());

	public TextExpressionPropertyValueSyntaxDefinition()
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
