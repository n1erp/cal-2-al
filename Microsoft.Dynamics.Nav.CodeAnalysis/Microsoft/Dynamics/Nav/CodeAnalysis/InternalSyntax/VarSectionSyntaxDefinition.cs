using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class VarSectionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<VarSectionSyntaxDefinition> Instance = new Lazy<VarSectionSyntaxDefinition>(() => new VarSectionSyntaxDefinition());

	public VarSectionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "VarKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.VarKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = VariableDeclarationSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Variables",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
