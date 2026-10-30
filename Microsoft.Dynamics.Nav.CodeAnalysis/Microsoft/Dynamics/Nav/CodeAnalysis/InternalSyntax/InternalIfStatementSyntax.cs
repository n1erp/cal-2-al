using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalIfStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _ifKeywordToken;

	internal readonly InternalCodeExpressionSyntax _condition;

	internal readonly InternalSyntaxToken _thenKeywordToken;

	internal readonly InternalStatementSyntax _statement;

	internal readonly InternalSyntaxToken _elseKeywordToken;

	internal readonly InternalStatementSyntax _elseStatement;

	internal override SyntaxNodeDefinition Definition => IfStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken IfKeywordToken => _ifKeywordToken;

	public InternalCodeExpressionSyntax Condition => _condition;

	public InternalSyntaxToken ThenKeywordToken => _thenKeywordToken;

	public InternalStatementSyntax Statement => _statement;

	public InternalSyntaxToken ElseKeywordToken => _elseKeywordToken;

	public InternalStatementSyntax ElseStatement => _elseStatement;

	internal InternalIfStatementSyntax(SyntaxKind kind, InternalSyntaxToken ifKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken thenKeywordToken, InternalStatementSyntax statement, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(ifKeywordToken);
		_ifKeywordToken = ifKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		AdjustFlagsAndWidth(thenKeywordToken);
		_thenKeywordToken = thenKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
		if (elseKeywordToken != null)
		{
			AdjustFlagsAndWidth(elseKeywordToken);
			_elseKeywordToken = elseKeywordToken;
		}
		if (elseStatement != null)
		{
			AdjustFlagsAndWidth(elseStatement);
			_elseStatement = elseStatement;
		}
	}

	internal InternalIfStatementSyntax(SyntaxKind kind, InternalSyntaxToken ifKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken thenKeywordToken, InternalStatementSyntax statement, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(ifKeywordToken);
		_ifKeywordToken = ifKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		AdjustFlagsAndWidth(thenKeywordToken);
		_thenKeywordToken = thenKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
		if (elseKeywordToken != null)
		{
			AdjustFlagsAndWidth(elseKeywordToken);
			_elseKeywordToken = elseKeywordToken;
		}
		if (elseStatement != null)
		{
			AdjustFlagsAndWidth(elseStatement);
			_elseStatement = elseStatement;
		}
	}

	internal InternalIfStatementSyntax(SyntaxKind kind, InternalSyntaxToken ifKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken thenKeywordToken, InternalStatementSyntax statement, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(ifKeywordToken);
		_ifKeywordToken = ifKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		AdjustFlagsAndWidth(thenKeywordToken);
		_thenKeywordToken = thenKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
		if (elseKeywordToken != null)
		{
			AdjustFlagsAndWidth(elseKeywordToken);
			_elseKeywordToken = elseKeywordToken;
		}
		if (elseStatement != null)
		{
			AdjustFlagsAndWidth(elseStatement);
			_elseStatement = elseStatement;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_ifKeywordToken, 
			1 => _condition, 
			2 => _thenKeywordToken, 
			3 => _statement, 
			4 => _elseKeywordToken, 
			5 => _elseStatement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new IfStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitIfStatement(this);
	}

	public InternalIfStatementSyntax Update(InternalSyntaxToken ifKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken thenKeywordToken, InternalStatementSyntax statement, InternalSyntaxToken elseKeywordToken, InternalStatementSyntax elseStatement)
	{
		if (ifKeywordToken != IfKeywordToken || condition != Condition || thenKeywordToken != ThenKeywordToken || statement != Statement || elseKeywordToken != ElseKeywordToken || elseStatement != ElseStatement)
		{
			return InternalSyntaxFactory.IfStatement(ifKeywordToken, condition, thenKeywordToken, statement, elseKeywordToken, elseStatement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalIfStatementSyntax(Kind, _ifKeywordToken, _condition, _thenKeywordToken, _statement, _elseKeywordToken, _elseStatement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalIfStatementSyntax(Kind, _ifKeywordToken, _condition, _thenKeywordToken, _statement, _elseKeywordToken, _elseStatement, GetDiagnostics(), annotations);
	}
}
