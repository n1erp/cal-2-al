using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class Int32PropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<Int32PropertyValueSyntaxDefinition> Instance = new Lazy<Int32PropertyValueSyntaxDefinition>(() => new Int32PropertyValueSyntaxDefinition());

	public Int32PropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = Int32SignedLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
