using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportLabelsSectionSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _labelsKeyword;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _labels;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ReportLabelsSectionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken LabelsKeyword => _labelsKeyword;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalReportLabelBaseSyntax> Labels => new InternalSyntaxList<InternalReportLabelBaseSyntax>(_labels);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalReportLabelsSectionSyntax(SyntaxKind kind, InternalSyntaxToken labelsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode labels, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(labelsKeyword);
		_labelsKeyword = labelsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (labels != null)
		{
			AdjustFlagsAndWidth(labels);
			_labels = labels;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportLabelsSectionSyntax(SyntaxKind kind, InternalSyntaxToken labelsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode labels, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(labelsKeyword);
		_labelsKeyword = labelsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (labels != null)
		{
			AdjustFlagsAndWidth(labels);
			_labels = labels;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportLabelsSectionSyntax(SyntaxKind kind, InternalSyntaxToken labelsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxNode labels, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(labelsKeyword);
		_labelsKeyword = labelsKeyword;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (labels != null)
		{
			AdjustFlagsAndWidth(labels);
			_labels = labels;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _labelsKeyword, 
			1 => _openBraceToken, 
			2 => _labels, 
			3 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportLabelsSectionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabelsSection(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReportLabelsSection(this);
	}

	public InternalReportLabelsSectionSyntax Update(InternalSyntaxToken labelsKeyword, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalReportLabelBaseSyntax> labels, InternalSyntaxToken closeBraceToken)
	{
		if (labelsKeyword != LabelsKeyword || openBraceToken != OpenBraceToken || labels != Labels || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.ReportLabelsSection(labelsKeyword, openBraceToken, labels, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportLabelsSectionSyntax(Kind, _labelsKeyword, _openBraceToken, _labels, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportLabelsSectionSyntax(Kind, _labelsKeyword, _openBraceToken, _labels, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
