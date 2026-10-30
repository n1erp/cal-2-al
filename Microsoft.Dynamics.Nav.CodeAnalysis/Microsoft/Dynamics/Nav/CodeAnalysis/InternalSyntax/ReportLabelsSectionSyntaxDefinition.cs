using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ReportLabelsSectionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ReportLabelsSectionSyntaxDefinition> Instance = new Lazy<ReportLabelsSectionSyntaxDefinition>(() => new ReportLabelsSectionSyntaxDefinition());

	public ReportLabelsSectionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "LabelsKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.LabelsKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ReportLabelBaseSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Labels",
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
