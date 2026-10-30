using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ParameterSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ParameterSyntaxDefinition> Instance = new Lazy<ParameterSyntaxDefinition>(() => new ParameterSyntaxDefinition());

	public ParameterSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "VarKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.VarKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
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
			Node = TypeReferenceBaseSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Type",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
