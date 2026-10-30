using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ReportLabelBaseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ReportLabelBaseSyntaxDefinition> Instance = new Lazy<ReportLabelBaseSyntaxDefinition>(() => new ReportLabelBaseSyntaxDefinition());

	public ReportLabelBaseSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ReportLabelMultilanguageSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = ReportLabelSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
