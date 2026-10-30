using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportDataItemLinkPropertyValueSyntax : PropertyValueSyntax
{
	private SyntaxNode _joins;

	public SeparatedSyntaxList<ReportDataItemLinkExpressionSyntax> Joins
	{
		get
		{
			SyntaxNode red = GetRed(ref _joins, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<ReportDataItemLinkExpressionSyntax>(red, 0);
			}
			return default;
		}
	}

	internal ReportDataItemLinkPropertyValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _joins);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _joins;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataItemLinkPropertyValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportDataItemLinkPropertyValue(this);
	}

	public ReportDataItemLinkPropertyValueSyntax Update(SeparatedSyntaxList<ReportDataItemLinkExpressionSyntax> joins)
	{
		if (joins != Joins)
		{
			ReportDataItemLinkPropertyValueSyntax reportDataItemLinkPropertyValueSyntax = SyntaxFactory.ReportDataItemLinkPropertyValue(joins);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportDataItemLinkPropertyValueSyntax.WithAnnotations(annotations);
			}
			return reportDataItemLinkPropertyValueSyntax;
		}
		return this;
	}

	public ReportDataItemLinkPropertyValueSyntax WithJoins(SeparatedSyntaxList<ReportDataItemLinkExpressionSyntax> joins)
	{
		return Update(joins);
	}

	public ReportDataItemLinkPropertyValueSyntax AddJoins(params ReportDataItemLinkExpressionSyntax[] items)
	{
		return Update(Joins.AddRange(items));
	}
}
