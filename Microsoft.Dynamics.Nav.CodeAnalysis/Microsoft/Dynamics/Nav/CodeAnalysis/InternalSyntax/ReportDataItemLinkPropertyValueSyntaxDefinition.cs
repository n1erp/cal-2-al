using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ReportDataItemLinkPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ReportDataItemLinkPropertyValueSyntaxDefinition> Instance = new Lazy<ReportDataItemLinkPropertyValueSyntaxDefinition>(() => new ReportDataItemLinkPropertyValueSyntaxDefinition());

	public ReportDataItemLinkPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ReportDataItemLinkExpressionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Joins",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
