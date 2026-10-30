using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class MethodDeclarationSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<MethodDeclarationSyntaxDefinition> Instance = new Lazy<MethodDeclarationSyntaxDefinition>(() => new MethodDeclarationSyntaxDefinition());

	public MethodDeclarationSyntaxDefinition()
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
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "LocalKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.LocalKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ProcedureKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.ProcedureKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Name",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = ParameterListSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ParameterList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = ReturnValueSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "ReturnValue",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "SemicolonToken",
			Kinds = ImmutableArray.Create(SyntaxKind.SemicolonToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = VarSectionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Variables",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = BlockSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Body",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
