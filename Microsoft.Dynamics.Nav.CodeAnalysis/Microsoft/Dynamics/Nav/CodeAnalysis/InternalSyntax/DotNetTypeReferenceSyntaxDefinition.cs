using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class DotNetTypeReferenceSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<DotNetTypeReferenceSyntaxDefinition> Instance = new Lazy<DotNetTypeReferenceSyntaxDefinition>(() => new DotNetTypeReferenceSyntaxDefinition());

	public DotNetTypeReferenceSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ArraySyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Array",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = DataTypeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "DataType",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
