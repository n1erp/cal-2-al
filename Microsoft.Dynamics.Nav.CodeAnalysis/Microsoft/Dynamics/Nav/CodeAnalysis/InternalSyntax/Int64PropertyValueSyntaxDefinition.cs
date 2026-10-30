using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class Int64PropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<Int64PropertyValueSyntaxDefinition> Instance = new Lazy<Int64PropertyValueSyntaxDefinition>(() => new Int64PropertyValueSyntaxDefinition());

	public Int64PropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = Int64SignedLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Value",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
