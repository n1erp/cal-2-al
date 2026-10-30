using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalExitStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _exitKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalCodeExpressionSyntax _exitValue;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => ExitStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ExitKeywordToken => _exitKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalCodeExpressionSyntax ExitValue => _exitValue;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalExitStatementSyntax(SyntaxKind kind, InternalSyntaxToken exitKeywordToken, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax exitValue, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(exitKeywordToken);
		_exitKeywordToken = exitKeywordToken;
		if (openParenthesisToken != null)
		{
			AdjustFlagsAndWidth(openParenthesisToken);
			_openParenthesisToken = openParenthesisToken;
		}
		if (exitValue != null)
		{
			AdjustFlagsAndWidth(exitValue);
			_exitValue = exitValue;
		}
		if (closeParenthesisToken != null)
		{
			AdjustFlagsAndWidth(closeParenthesisToken);
			_closeParenthesisToken = closeParenthesisToken;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalExitStatementSyntax(SyntaxKind kind, InternalSyntaxToken exitKeywordToken, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax exitValue, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(exitKeywordToken);
		_exitKeywordToken = exitKeywordToken;
		if (openParenthesisToken != null)
		{
			AdjustFlagsAndWidth(openParenthesisToken);
			_openParenthesisToken = openParenthesisToken;
		}
		if (exitValue != null)
		{
			AdjustFlagsAndWidth(exitValue);
			_exitValue = exitValue;
		}
		if (closeParenthesisToken != null)
		{
			AdjustFlagsAndWidth(closeParenthesisToken);
			_closeParenthesisToken = closeParenthesisToken;
		}
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalExitStatementSyntax(SyntaxKind kind, InternalSyntaxToken exitKeywordToken, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax exitValue, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(exitKeywordToken);
		_exitKeywordToken = exitKeywordToken;
		if (openParenthesisToken != null)
		{
			AdjustFlagsAndWidth(openParenthesisToken);
			_openParenthesisToken = openParenthesisToken;
		}
		if (exitValue != null)
		{
			AdjustFlagsAndWidth(exitValue);
			_exitValue = exitValue;
		}
		if (closeParenthesisToken != null)
		{
			AdjustFlagsAndWidth(closeParenthesisToken);
			_closeParenthesisToken = closeParenthesisToken;
		}
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
			0 => (InternalSyntaxNode)_exitKeywordToken, 
			1 => _openParenthesisToken, 
			2 => _exitValue, 
			3 => _closeParenthesisToken, 
			4 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ExitStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExitStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitExitStatement(this);
	}

	public InternalExitStatementSyntax Update(InternalSyntaxToken exitKeywordToken, InternalSyntaxToken openParenthesisToken, InternalCodeExpressionSyntax exitValue, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken semicolonToken)
	{
		if (exitKeywordToken != ExitKeywordToken || openParenthesisToken != OpenParenthesisToken || exitValue != ExitValue || closeParenthesisToken != CloseParenthesisToken || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.ExitStatement(exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalExitStatementSyntax(Kind, _exitKeywordToken, _openParenthesisToken, _exitValue, _closeParenthesisToken, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalExitStatementSyntax(Kind, _exitKeywordToken, _openParenthesisToken, _exitValue, _closeParenthesisToken, _semicolonToken, GetDiagnostics(), annotations);
	}
}
