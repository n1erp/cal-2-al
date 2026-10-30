using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SubtypedDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<SubtypedDataTypeSyntaxDefinition> Instance = new Lazy<SubtypedDataTypeSyntaxDefinition>(() => new SubtypedDataTypeSyntaxDefinition());

	public SubtypedDataTypeSyntaxDefinition()
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
			Node = ObjectNameOrIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Subtype",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
