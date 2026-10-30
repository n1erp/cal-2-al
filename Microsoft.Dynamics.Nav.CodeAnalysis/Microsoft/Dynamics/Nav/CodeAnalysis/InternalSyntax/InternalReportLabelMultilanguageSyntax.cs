using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportLabelMultilanguageSyntax : InternalReportLabelBaseSyntax
{
	internal readonly InternalSyntaxToken _labelKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalCommaSeparatedIdentifierEqualsStringListSyntax _caption;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ReportLabelMultilanguageSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken LabelKeyword => _labelKeyword;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalCommaSeparatedIdentifierEqualsStringListSyntax Caption => _caption;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalReportLabelMultilanguageSyntax(SyntaxKind kind, InternalSyntaxToken labelKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCommaSeparatedIdentifierEqualsStringListSyntax caption, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(labelKeyword);
		_labelKeyword = labelKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(caption);
		_caption = caption;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalReportLabelMultilanguageSyntax(SyntaxKind kind, InternalSyntaxToken labelKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCommaSeparatedIdentifierEqualsStringListSyntax caption, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(labelKeyword);
		_labelKeyword = labelKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(caption);
		_caption = caption;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalReportLabelMultilanguageSyntax(SyntaxKind kind, InternalSyntaxToken labelKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCommaSeparatedIdentifierEqualsStringListSyntax caption, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(labelKeyword);
		_labelKeyword = labelKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		AdjustFlagsAndWidth(caption);
		_caption = caption;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_labelKeyword, 
			1 => _openParenthesisToken, 
			2 => _name, 
			3 => _semicolonToken, 
			4 => _caption, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportLabelMultilanguageSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReportLabelMultilanguage(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReportLabelMultilanguage(this);
	}

	public InternalReportLabelMultilanguageSyntax Update(InternalSyntaxToken labelKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax name, InternalSyntaxToken semicolonToken, InternalCommaSeparatedIdentifierEqualsStringListSyntax caption, InternalSyntaxToken closeParenthesisToken)
	{
		if (labelKeyword != LabelKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || caption != Caption || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ReportLabelMultilanguage(labelKeyword, openParenthesisToken, name, semicolonToken, caption, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportLabelMultilanguageSyntax(Kind, _labelKeyword, _openParenthesisToken, _name, _semicolonToken, _caption, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportLabelMultilanguageSyntax(Kind, _labelKeyword, _openParenthesisToken, _name, _semicolonToken, _caption, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
