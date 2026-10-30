using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalForStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _forKeywordToken;

	internal readonly InternalCodeExpressionSyntax _loopVariable;

	internal readonly InternalSyntaxToken _assignToken;

	internal readonly InternalCodeExpressionSyntax _initialValue;

	internal readonly InternalSyntaxToken _operatorKeywordToken;

	internal readonly InternalCodeExpressionSyntax _endValue;

	internal readonly InternalSyntaxToken _doKeywordToken;

	internal readonly InternalStatementSyntax _statement;

	internal override SyntaxNodeDefinition Definition => ForStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ForKeywordToken => _forKeywordToken;

	public InternalCodeExpressionSyntax LoopVariable => _loopVariable;

	public InternalSyntaxToken AssignToken => _assignToken;

	public InternalCodeExpressionSyntax InitialValue => _initialValue;

	public InternalSyntaxToken OperatorKeywordToken => _operatorKeywordToken;

	public InternalCodeExpressionSyntax EndValue => _endValue;

	public InternalSyntaxToken DoKeywordToken => _doKeywordToken;

	public InternalStatementSyntax Statement => _statement;

	internal InternalForStatementSyntax(SyntaxKind kind, InternalSyntaxToken forKeywordToken, InternalCodeExpressionSyntax loopVariable, InternalSyntaxToken assignToken, InternalCodeExpressionSyntax initialValue, InternalSyntaxToken operatorKeywordToken, InternalCodeExpressionSyntax endValue, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(forKeywordToken);
		_forKeywordToken = forKeywordToken;
		AdjustFlagsAndWidth(loopVariable);
		_loopVariable = loopVariable;
		AdjustFlagsAndWidth(assignToken);
		_assignToken = assignToken;
		AdjustFlagsAndWidth(initialValue);
		_initialValue = initialValue;
		AdjustFlagsAndWidth(operatorKeywordToken);
		_operatorKeywordToken = operatorKeywordToken;
		AdjustFlagsAndWidth(endValue);
		_endValue = endValue;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalForStatementSyntax(SyntaxKind kind, InternalSyntaxToken forKeywordToken, InternalCodeExpressionSyntax loopVariable, InternalSyntaxToken assignToken, InternalCodeExpressionSyntax initialValue, InternalSyntaxToken operatorKeywordToken, InternalCodeExpressionSyntax endValue, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(forKeywordToken);
		_forKeywordToken = forKeywordToken;
		AdjustFlagsAndWidth(loopVariable);
		_loopVariable = loopVariable;
		AdjustFlagsAndWidth(assignToken);
		_assignToken = assignToken;
		AdjustFlagsAndWidth(initialValue);
		_initialValue = initialValue;
		AdjustFlagsAndWidth(operatorKeywordToken);
		_operatorKeywordToken = operatorKeywordToken;
		AdjustFlagsAndWidth(endValue);
		_endValue = endValue;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalForStatementSyntax(SyntaxKind kind, InternalSyntaxToken forKeywordToken, InternalCodeExpressionSyntax loopVariable, InternalSyntaxToken assignToken, InternalCodeExpressionSyntax initialValue, InternalSyntaxToken operatorKeywordToken, InternalCodeExpressionSyntax endValue, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(forKeywordToken);
		_forKeywordToken = forKeywordToken;
		AdjustFlagsAndWidth(loopVariable);
		_loopVariable = loopVariable;
		AdjustFlagsAndWidth(assignToken);
		_assignToken = assignToken;
		AdjustFlagsAndWidth(initialValue);
		_initialValue = initialValue;
		AdjustFlagsAndWidth(operatorKeywordToken);
		_operatorKeywordToken = operatorKeywordToken;
		AdjustFlagsAndWidth(endValue);
		_endValue = endValue;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_forKeywordToken, 
			1 => _loopVariable, 
			2 => _assignToken, 
			3 => _initialValue, 
			4 => _operatorKeywordToken, 
			5 => _endValue, 
			6 => _doKeywordToken, 
			7 => _statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ForStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitForStatement(this);
	}

	public InternalForStatementSyntax Update(InternalSyntaxToken forKeywordToken, InternalCodeExpressionSyntax loopVariable, InternalSyntaxToken assignToken, InternalCodeExpressionSyntax initialValue, InternalSyntaxToken operatorKeywordToken, InternalCodeExpressionSyntax endValue, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		if (forKeywordToken != ForKeywordToken || loopVariable != LoopVariable || assignToken != AssignToken || initialValue != InitialValue || operatorKeywordToken != OperatorKeywordToken || endValue != EndValue || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			return InternalSyntaxFactory.ForStatement(forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalForStatementSyntax(Kind, _forKeywordToken, _loopVariable, _assignToken, _initialValue, _operatorKeywordToken, _endValue, _doKeywordToken, _statement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalForStatementSyntax(Kind, _forKeywordToken, _loopVariable, _assignToken, _initialValue, _operatorKeywordToken, _endValue, _doKeywordToken, _statement, GetDiagnostics(), annotations);
	}
}
