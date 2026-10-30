using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class GenericNamedDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<GenericNamedDataTypeSyntaxDefinition> Instance = new Lazy<GenericNamedDataTypeSyntaxDefinition>(() => new GenericNamedDataTypeSyntaxDefinition());

	public GenericNamedDataTypeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
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
			IsOptional = false,
			Name = "OfKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.OfKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "OpenBracketToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBracketToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = DataTypeSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "TypeArguments",
			Kinds = ImmutableArray.Create<SyntaxKind>()
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
