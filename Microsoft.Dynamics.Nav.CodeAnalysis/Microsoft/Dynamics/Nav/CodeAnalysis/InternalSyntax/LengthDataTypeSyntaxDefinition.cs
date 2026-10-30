using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LengthDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<LengthDataTypeSyntaxDefinition> Instance = new Lazy<LengthDataTypeSyntaxDefinition>(() => new LengthDataTypeSyntaxDefinition());

	public LengthDataTypeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TypeName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "OpenBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBracketToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "Length",
			Kinds = ImmutableArray.Create(SyntaxKind.Int32LiteralToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "CloseBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBracketToken)
		}));
	}
}
