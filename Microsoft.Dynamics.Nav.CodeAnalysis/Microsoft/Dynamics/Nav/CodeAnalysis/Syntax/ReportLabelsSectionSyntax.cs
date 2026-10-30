using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportLabelsSectionSyntax : SyntaxNode
{
	private SyntaxNode _labels;

	public SyntaxToken LabelsKeyword => new SyntaxToken(this, ((InternalReportLabelsSectionSyntax)Node)._labelsKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalReportLabelsSectionSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<ReportLabelBaseSyntax> Labels => new SyntaxList<ReportLabelBaseSyntax>(GetRed(ref _labels, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalReportLabelsSectionSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal ReportLabelsSectionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _labels, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _labels;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabelsSection(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportLabelsSection(this);
	}

	public ReportLabelsSectionSyntax Update(SyntaxToken labelsKeyword, SyntaxToken openBraceToken, SyntaxList<ReportLabelBaseSyntax> labels, SyntaxToken closeBraceToken)
	{
		if (labelsKeyword != LabelsKeyword || openBraceToken != OpenBraceToken || labels != Labels || closeBraceToken != CloseBraceToken)
		{
			ReportLabelsSectionSyntax reportLabelsSectionSyntax = SyntaxFactory.ReportLabelsSection(labelsKeyword, openBraceToken, labels, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportLabelsSectionSyntax.WithAnnotations(annotations);
			}
			return reportLabelsSectionSyntax;
		}
		return this;
	}

	public ReportLabelsSectionSyntax WithLabelsKeyword(SyntaxToken labelsKeyword)
	{
		return Update(labelsKeyword, OpenBraceToken, Labels, CloseBraceToken);
	}

	public ReportLabelsSectionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(LabelsKeyword, openBraceToken, Labels, CloseBraceToken);
	}

	public ReportLabelsSectionSyntax WithLabels(SyntaxList<ReportLabelBaseSyntax> labels)
	{
		return Update(LabelsKeyword, OpenBraceToken, labels, CloseBraceToken);
	}

	public ReportLabelsSectionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(LabelsKeyword, OpenBraceToken, Labels, closeBraceToken);
	}

	public ReportLabelsSectionSyntax AddLabels(params ReportLabelBaseSyntax[] items)
	{
		return WithLabels(Labels.AddRange(items));
	}
}
