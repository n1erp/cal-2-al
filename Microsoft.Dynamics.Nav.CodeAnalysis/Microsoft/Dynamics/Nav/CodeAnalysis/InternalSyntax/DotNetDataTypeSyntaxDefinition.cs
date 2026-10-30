using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DotNetDataTypeSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DotNetDataTypeSyntaxDefinition> Instance = new Lazy<DotNetDataTypeSyntaxDefinition>(() => new DotNetDataTypeSyntaxDefinition());

	public DotNetDataTypeSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "TypeName",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
