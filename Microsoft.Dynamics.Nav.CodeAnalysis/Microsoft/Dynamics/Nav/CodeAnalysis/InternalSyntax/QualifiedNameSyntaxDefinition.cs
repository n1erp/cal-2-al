using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class QualifiedNameSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<QualifiedNameSyntaxDefinition> Instance = new Lazy<QualifiedNameSyntaxDefinition>(() => new QualifiedNameSyntaxDefinition());

	public QualifiedNameSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = NameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Left",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "DotToken",
			Kinds = ImmutableArray.Create(SyntaxKind.DotToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = SimpleNameSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Right",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
