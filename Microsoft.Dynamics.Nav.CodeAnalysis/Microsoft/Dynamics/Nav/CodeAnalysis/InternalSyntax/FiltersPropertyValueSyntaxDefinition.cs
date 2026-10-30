using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class FiltersPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<FiltersPropertyValueSyntaxDefinition> Instance = new Lazy<FiltersPropertyValueSyntaxDefinition>(() => new FiltersPropertyValueSyntaxDefinition());

	public FiltersPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = WhereExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "TableFilter",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
