using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class VariableDeclarationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<VariableDeclarationSyntaxDefinition> Instance = new Lazy<VariableDeclarationSyntaxDefinition>(() => new VariableDeclarationSyntaxDefinition());

	public VariableDeclarationSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = MemberAttributeSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Attributes",
			Kinds = ImmutableArray.Create<SyntaxKind>()
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
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}));
	}
}
