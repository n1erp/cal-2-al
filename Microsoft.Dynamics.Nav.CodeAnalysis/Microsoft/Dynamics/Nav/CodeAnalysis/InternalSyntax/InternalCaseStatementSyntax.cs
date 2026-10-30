using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalCaseStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _caseKeywordToken;

	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _ofKeywordToken;

	internal readonly InternalSyntaxNode _caseLines;

	internal readonly InternalCaseElseSyntax _caseElse;

	internal readonly InternalSyntaxToken _endKeywordToken;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => CaseStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken CaseKeywordToken => _caseKeywordToken;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken OfKeywordToken => _ofKeywordToken;

	public InternalSyntaxList<InternalCaseLineSyntax> CaseLines => new InternalSyntaxList<InternalCaseLineSyntax>(_caseLines);

	public InternalCaseElseSyntax CaseElse => _caseElse;

	public InternalSyntaxToken EndKeywordToken => _endKeywordToken;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalCaseStatementSyntax(SyntaxKind kind, InternalSyntaxToken caseKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken ofKeywordToken, InternalSyntaxNode caseLines, InternalCaseElseSyntax caseElse, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(caseKeywordToken);
		_caseKeywordToken = caseKeywordToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(ofKeywordToken);
		_ofKeywordToken = ofKeywordToken;
		if (caseLines != null)
		{
			AdjustFlagsAndWidth(caseLines);
			_caseLines = caseLines;
		}
		if (caseElse != null)
		{
			AdjustFlagsAndWidth(caseElse);
			_caseElse = caseElse;
		}
		AdjustFlagsAndWidth(endKeywordToken);
		_endKeywordToken = endKeywordToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalCaseStatementSyntax(SyntaxKind kind, InternalSyntaxToken caseKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken ofKeywordToken, InternalSyntaxNode caseLines, InternalCaseElseSyntax caseElse, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(caseKeywordToken);
		_caseKeywordToken = caseKeywordToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(ofKeywordToken);
		_ofKeywordToken = ofKeywordToken;
		if (caseLines != null)
		{
			AdjustFlagsAndWidth(caseLines);
			_caseLines = caseLines;
		}
		if (caseElse != null)
		{
			AdjustFlagsAndWidth(caseElse);
			_caseElse = caseElse;
		}
		AdjustFlagsAndWidth(endKeywordToken);
		_endKeywordToken = endKeywordToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalCaseStatementSyntax(SyntaxKind kind, InternalSyntaxToken caseKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken ofKeywordToken, InternalSyntaxNode caseLines, InternalCaseElseSyntax caseElse, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(caseKeywordToken);
		_caseKeywordToken = caseKeywordToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(ofKeywordToken);
		_ofKeywordToken = ofKeywordToken;
		if (caseLines != null)
		{
			AdjustFlagsAndWidth(caseLines);
			_caseLines = caseLines;
		}
		if (caseElse != null)
		{
			AdjustFlagsAndWidth(caseElse);
			_caseElse = caseElse;
		}
		AdjustFlagsAndWidth(endKeywordToken);
		_endKeywordToken = endKeywordToken;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _caseKeywordToken, 
			1 => _expression, 
			2 => _ofKeywordToken, 
			3 => _caseLines, 
			4 => _caseElse, 
			5 => _endKeywordToken, 
			6 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new CaseStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitCaseStatement(this);
	}

	public InternalCaseStatementSyntax Update(InternalSyntaxToken caseKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken ofKeywordToken, InternalSyntaxList<InternalCaseLineSyntax> caseLines, InternalCaseElseSyntax caseElse, InternalSyntaxToken endKeywordToken, InternalSyntaxToken semicolonToken)
	{
		if (caseKeywordToken != CaseKeywordToken || expression != Expression || ofKeywordToken != OfKeywordToken || caseLines != CaseLines || caseElse != CaseElse || endKeywordToken != EndKeywordToken || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.CaseStatement(caseKeywordToken, expression, ofKeywordToken, caseLines, caseElse, endKeywordToken, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalCaseStatementSyntax(Kind, _caseKeywordToken, _expression, _ofKeywordToken, _caseLines, _caseElse, _endKeywordToken, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalCaseStatementSyntax(Kind, _caseKeywordToken, _expression, _ofKeywordToken, _caseLines, _caseElse, _endKeywordToken, _semicolonToken, GetDiagnostics(), annotations);
	}
}
