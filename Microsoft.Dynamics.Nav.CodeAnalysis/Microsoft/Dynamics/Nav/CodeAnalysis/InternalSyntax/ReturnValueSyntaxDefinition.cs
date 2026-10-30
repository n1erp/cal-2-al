using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ReturnValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ReturnValueSyntaxDefinition> Instance = new Lazy<ReturnValueSyntaxDefinition>(() => new ReturnValueSyntaxDefinition());

	public ReturnValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ColonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.ColonToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = DataTypeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "DataType",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
