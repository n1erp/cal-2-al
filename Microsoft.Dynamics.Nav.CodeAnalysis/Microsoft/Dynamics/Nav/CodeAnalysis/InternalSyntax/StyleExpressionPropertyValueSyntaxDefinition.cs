using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class StyleExpressionPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<StyleExpressionPropertyValueSyntaxDefinition> Instance = new Lazy<StyleExpressionPropertyValueSyntaxDefinition>(() => new StyleExpressionPropertyValueSyntaxDefinition());

	public StyleExpressionPropertyValueSyntaxDefinition()
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
