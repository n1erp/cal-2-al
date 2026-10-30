using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class StringPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<StringPropertyValueSyntaxDefinition> Instance = new Lazy<StringPropertyValueSyntaxDefinition>(() => new StringPropertyValueSyntaxDefinition());

	public StringPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = StringLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
