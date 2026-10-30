using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalRepeatStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _repeatKeywordToken;

	internal readonly InternalSyntaxNode _statements;

	internal readonly InternalSyntaxToken _untilKeywordToken;

	internal readonly InternalCodeExpressionSyntax _condition;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal override SyntaxNodeDefinition Definition => RepeatStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken RepeatKeywordToken => _repeatKeywordToken;

	public InternalSyntaxList<InternalStatementSyntax> Statements => new InternalSyntaxList<InternalStatementSyntax>(_statements);

	public InternalSyntaxToken UntilKeywordToken => _untilKeywordToken;

	public InternalCodeExpressionSyntax Condition => _condition;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	internal InternalRepeatStatementSyntax(SyntaxKind kind, InternalSyntaxToken repeatKeywordToken, InternalSyntaxNode statements, InternalSyntaxToken untilKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(repeatKeywordToken);
		_repeatKeywordToken = repeatKeywordToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			_statements = statements;
		}
		AdjustFlagsAndWidth(untilKeywordToken);
		_untilKeywordToken = untilKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalRepeatStatementSyntax(SyntaxKind kind, InternalSyntaxToken repeatKeywordToken, InternalSyntaxNode statements, InternalSyntaxToken untilKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken semicolonToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(repeatKeywordToken);
		_repeatKeywordToken = repeatKeywordToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			_statements = statements;
		}
		AdjustFlagsAndWidth(untilKeywordToken);
		_untilKeywordToken = untilKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
		if (semicolonToken != null)
		{
			AdjustFlagsAndWidth(semicolonToken);
			_semicolonToken = semicolonToken;
		}
	}

	internal InternalRepeatStatementSyntax(SyntaxKind kind, InternalSyntaxToken repeatKeywordToken, InternalSyntaxNode statements, InternalSyntaxToken untilKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 5;
		AdjustFlagsAndWidth(repeatKeywordToken);
		_repeatKeywordToken = repeatKeywordToken;
		if (statements != null)
		{
			AdjustFlagsAndWidth(statements);
			_statements = statements;
		}
		AdjustFlagsAndWidth(untilKeywordToken);
		_untilKeywordToken = untilKeywordToken;
		AdjustFlagsAndWidth(condition);
		_condition = condition;
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
			0 => _repeatKeywordToken, 
			1 => _statements, 
			2 => _untilKeywordToken, 
			3 => _condition, 
			4 => _semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new RepeatStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRepeatStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitRepeatStatement(this);
	}

	public InternalRepeatStatementSyntax Update(InternalSyntaxToken repeatKeywordToken, InternalSyntaxList<InternalStatementSyntax> statements, InternalSyntaxToken untilKeywordToken, InternalCodeExpressionSyntax condition, InternalSyntaxToken semicolonToken)
	{
		if (repeatKeywordToken != RepeatKeywordToken || statements != Statements || untilKeywordToken != UntilKeywordToken || condition != Condition || semicolonToken != SemicolonToken)
		{
			return InternalSyntaxFactory.RepeatStatement(repeatKeywordToken, statements, untilKeywordToken, condition, semicolonToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalRepeatStatementSyntax(Kind, _repeatKeywordToken, _statements, _untilKeywordToken, _condition, _semicolonToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalRepeatStatementSyntax(Kind, _repeatKeywordToken, _statements, _untilKeywordToken, _condition, _semicolonToken, GetDiagnostics(), annotations);
	}
}
