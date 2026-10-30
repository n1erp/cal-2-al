using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ReportDataSetSectionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ReportDataSetSectionSyntaxDefinition> Instance = new Lazy<ReportDataSetSectionSyntaxDefinition>(() => new ReportDataSetSectionSyntaxDefinition());

	public ReportDataSetSectionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "DataSetKeyword",
			Kinds = ImmutableArray.Create(SyntaxKind.DataSetKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "OpenBraceToken",
			Kinds = ImmutableArray.Create(SyntaxKind.OpenBraceToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ReportDataItemSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "DataItems",
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
