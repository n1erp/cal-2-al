using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DotNetPackageSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DotNetPackageSyntaxDefinition> Instance = new Lazy<DotNetPackageSyntaxDefinition>(() => new DotNetPackageSyntaxDefinition());

	public DotNetPackageSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create<SyntaxNodeSlotDefinition>(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "DotNetKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.DotNetKeyword)
		}, new SyntaxNodeSlotDefinition
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
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = PropertyListSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "PropertyList",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = DotNetAssemblySyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Assemblies",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = MemberSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = true,
			Name = "Members",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "CloseBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.CloseBraceToken)
		}));
	}
}
