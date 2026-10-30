using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportDataSetSectionSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _dataSetKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _dataItems;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ReportDataSetSectionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken DataSetKeyword => _dataSetKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalReportDataItemSyntax> DataItems => new InternalSyntaxList<InternalReportDataItemSyntax>(_dataItems);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalReportDataSetSectionSyntax(SyntaxKind kind, InternalSyntaxToken dataSetKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode dataItems, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(dataSetKeyword);
		_dataSetKeyword = dataSetKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (dataItems != null)
		{
			AdjustFlagsAndWidth(dataItems);
			_dataItems = dataItems;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportDataSetSectionSyntax(SyntaxKind kind, InternalSyntaxToken dataSetKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode dataItems, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(dataSetKeyword);
		_dataSetKeyword = dataSetKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (dataItems != null)
		{
			AdjustFlagsAndWidth(dataItems);
			_dataItems = dataItems;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportDataSetSectionSyntax(SyntaxKind kind, InternalSyntaxToken dataSetKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode dataItems, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(dataSetKeyword);
		_dataSetKeyword = dataSetKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (dataItems != null)
		{
			AdjustFlagsAndWidth(dataItems);
			_dataItems = dataItems;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _dataSetKeyword, 
			1 => _openBraceToken, 
			2 => _dataItems, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportDataSetSectionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportDataSetSection(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReportDataSetSection(this);
	}

	public InternalReportDataSetSectionSyntax Update(InternalSyntaxToken dataSetKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalReportDataItemSyntax> dataItems, InternalSyntaxToken closeBraceToken)
	{
		if (dataSetKeyword != DataSetKeyword || openBraceToken != OpenBraceToken || dataItems != DataItems || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.ReportDataSetSection(dataSetKeyword, openBraceToken, dataItems, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportDataSetSectionSyntax(Kind, _dataSetKeyword, _openBraceToken, _dataItems, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportDataSetSectionSyntax(Kind, _dataSetKeyword, _openBraceToken, _dataItems, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
