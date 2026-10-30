using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class EnumDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<EnumDataTypeSyntaxDefinition> Instance = new Lazy<EnumDataTypeSyntaxDefinition>(() => new EnumDataTypeSyntaxDefinition());

	public EnumDataTypeSyntaxDefinition()
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
			Node = IdentifierNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "EnumTypeName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
