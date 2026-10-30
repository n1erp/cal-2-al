using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ReportDataItemElementSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ReportDataItemElementSyntaxDefinition> Instance = new Lazy<ReportDataItemElementSyntaxDefinition>(() => new ReportDataItemElementSyntaxDefinition());

	public ReportDataItemElementSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ReportDataItemSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = ReportColumnSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
