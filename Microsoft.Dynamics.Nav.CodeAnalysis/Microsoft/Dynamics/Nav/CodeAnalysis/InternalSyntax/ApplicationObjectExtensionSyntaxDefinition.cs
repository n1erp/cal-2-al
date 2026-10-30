using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ApplicationObjectExtensionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ApplicationObjectExtensionSyntaxDefinition> Instance = new Lazy<ApplicationObjectExtensionSyntaxDefinition>(() => new ApplicationObjectExtensionSyntaxDefinition());

	public ApplicationObjectExtensionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = EnumExtensionTypeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = AbstractPageExtensionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = TableExtensionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
