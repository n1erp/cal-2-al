using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LabelSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<LabelSyntaxDefinition> Instance = new Lazy<LabelSyntaxDefinition>(() => new LabelSyntaxDefinition());

	public LabelSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = StringLiteralValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "LabelText",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "CommaToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CommaToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = CommaSeparatedIdentifierEqualsLiteralListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Properties",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
