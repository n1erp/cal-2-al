using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportDataSetSectionSyntax : SyntaxNode
{
	private SyntaxNode _dataItems;

	public SyntaxToken DataSetKeyword => new SyntaxToken(this, ((InternalReportDataSetSectionSyntax)Node)._dataSetKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalReportDataSetSectionSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<ReportDataItemSyntax> DataItems => new SyntaxList<ReportDataItemSyntax>(GetRed(ref _dataItems, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalReportDataSetSectionSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal ReportDataSetSectionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _dataItems, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _dataItems;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataSetSection(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReportDataSetSection(this);
	}

	public ReportDataSetSectionSyntax Update(SyntaxToken dataSetKeyword, SyntaxToken openBraceToken, SyntaxList<ReportDataItemSyntax> dataItems, SyntaxToken closeBraceToken)
	{
		if (dataSetKeyword != DataSetKeyword || openBraceToken != OpenBraceToken || dataItems != DataItems || closeBraceToken != CloseBraceToken)
		{
			ReportDataSetSectionSyntax reportDataSetSectionSyntax = SyntaxFactory.ReportDataSetSection(dataSetKeyword, openBraceToken, dataItems, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportDataSetSectionSyntax.WithAnnotations(annotations);
			}
			return reportDataSetSectionSyntax;
		}
		return this;
	}

	public ReportDataSetSectionSyntax WithDataSetKeyword(SyntaxToken dataSetKeyword)
	{
		return Update(dataSetKeyword, OpenBraceToken, DataItems, CloseBraceToken);
	}

	public ReportDataSetSectionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(DataSetKeyword, openBraceToken, DataItems, CloseBraceToken);
	}

	public ReportDataSetSectionSyntax WithDataItems(SyntaxList<ReportDataItemSyntax> dataItems)
	{
		return Update(DataSetKeyword, OpenBraceToken, dataItems, CloseBraceToken);
	}

	public ReportDataSetSectionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(DataSetKeyword, OpenBraceToken, DataItems, closeBraceToken);
	}

	public ReportDataSetSectionSyntax AddDataItems(params ReportDataItemSyntax[] items)
	{
		return WithDataItems(DataItems.AddRange(items));
	}
}
