using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalForEachStatementSyntax : InternalStatementSyntax
{
	internal readonly InternalSyntaxToken _forEachKeywordToken;

	internal readonly InternalIdentifierNameSyntax _iterationVariable;

	internal readonly InternalSyntaxToken _inKeywordToken;

	internal readonly InternalCodeExpressionSyntax _expression;

	internal readonly InternalSyntaxToken _doKeywordToken;

	internal readonly InternalStatementSyntax _statement;

	internal override SyntaxNodeDefinition Definition => ForEachStatementSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ForEachKeywordToken => _forEachKeywordToken;

	public InternalIdentifierNameSyntax IterationVariable => _iterationVariable;

	public InternalSyntaxToken InKeywordToken => _inKeywordToken;

	public InternalCodeExpressionSyntax Expression => _expression;

	public InternalSyntaxToken DoKeywordToken => _doKeywordToken;

	public InternalStatementSyntax Statement => _statement;

	internal InternalForEachStatementSyntax(SyntaxKind kind, InternalSyntaxToken forEachKeywordToken, InternalIdentifierNameSyntax iterationVariable, InternalSyntaxToken inKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(forEachKeywordToken);
		_forEachKeywordToken = forEachKeywordToken;
		AdjustFlagsAndWidth(iterationVariable);
		_iterationVariable = iterationVariable;
		AdjustFlagsAndWidth(inKeywordToken);
		_inKeywordToken = inKeywordToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalForEachStatementSyntax(SyntaxKind kind, InternalSyntaxToken forEachKeywordToken, InternalIdentifierNameSyntax iterationVariable, InternalSyntaxToken inKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(forEachKeywordToken);
		_forEachKeywordToken = forEachKeywordToken;
		AdjustFlagsAndWidth(iterationVariable);
		_iterationVariable = iterationVariable;
		AdjustFlagsAndWidth(inKeywordToken);
		_inKeywordToken = inKeywordToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal InternalForEachStatementSyntax(SyntaxKind kind, InternalSyntaxToken forEachKeywordToken, InternalIdentifierNameSyntax iterationVariable, InternalSyntaxToken inKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(forEachKeywordToken);
		_forEachKeywordToken = forEachKeywordToken;
		AdjustFlagsAndWidth(iterationVariable);
		_iterationVariable = iterationVariable;
		AdjustFlagsAndWidth(inKeywordToken);
		_inKeywordToken = inKeywordToken;
		AdjustFlagsAndWidth(expression);
		_expression = expression;
		AdjustFlagsAndWidth(doKeywordToken);
		_doKeywordToken = doKeywordToken;
		AdjustFlagsAndWidth(statement);
		_statement = statement;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_forEachKeywordToken, 
			1 => _iterationVariable, 
			2 => _inKeywordToken, 
			3 => _expression, 
			4 => _doKeywordToken, 
			5 => _statement, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ForEachStatementSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitForEachStatement(this);
	}

	public InternalForEachStatementSyntax Update(InternalSyntaxToken forEachKeywordToken, InternalIdentifierNameSyntax iterationVariable, InternalSyntaxToken inKeywordToken, InternalCodeExpressionSyntax expression, InternalSyntaxToken doKeywordToken, InternalStatementSyntax statement)
	{
		if (forEachKeywordToken != ForEachKeywordToken || iterationVariable != IterationVariable || inKeywordToken != InKeywordToken || expression != Expression || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			return InternalSyntaxFactory.ForEachStatement(forEachKeywordToken, iterationVariable, inKeywordToken, expression, doKeywordToken, statement).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalForEachStatementSyntax(Kind, _forEachKeywordToken, _iterationVariable, _inKeywordToken, _expression, _doKeywordToken, _statement, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalForEachStatementSyntax(Kind, _forEachKeywordToken, _iterationVariable, _inKeywordToken, _expression, _doKeywordToken, _statement, GetDiagnostics(), annotations);
	}
}
