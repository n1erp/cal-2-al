using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ForEachStatementSyntax : StatementSyntax
{
	private IdentifierNameSyntax _iterationVariable;

	private CodeExpressionSyntax _expression;

	private StatementSyntax _statement;

	public SyntaxToken ForEachKeywordToken => new SyntaxToken(this, ((InternalForEachStatementSyntax)Node)._forEachKeywordToken, Position, 0);

	public IdentifierNameSyntax IterationVariable => GetRed(ref _iterationVariable, 1);

	public SyntaxToken InKeywordToken => new SyntaxToken(this, ((InternalForEachStatementSyntax)Node)._inKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public CodeExpressionSyntax Expression => GetRed(ref _expression, 3);

	public SyntaxToken DoKeywordToken => new SyntaxToken(this, ((InternalForEachStatementSyntax)Node)._doKeywordToken, GetChildPosition(4), GetChildIndex(4));

	public StatementSyntax Statement => GetRed(ref _statement, 5);

	internal ForEachStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _iterationVariable, 1), 
			3 => GetRed(ref _expression, 3), 
			5 => GetRed(ref _statement, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_iterationVariable, 
			3 => _expression, 
			5 => _statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitForEachStatement(this);
	}

	public ForEachStatementSyntax Update(SyntaxToken forEachKeywordToken, IdentifierNameSyntax iterationVariable, SyntaxToken inKeywordToken, CodeExpressionSyntax expression, SyntaxToken doKeywordToken, StatementSyntax statement)
	{
		if (forEachKeywordToken != ForEachKeywordToken || iterationVariable != IterationVariable || inKeywordToken != InKeywordToken || expression != Expression || doKeywordToken != DoKeywordToken || statement != Statement)
		{
			ForEachStatementSyntax forEachStatementSyntax = SyntaxFactory.ForEachStatement(forEachKeywordToken, iterationVariable, inKeywordToken, expression, doKeywordToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return forEachStatementSyntax.WithAnnotations(annotations);
			}
			return forEachStatementSyntax;
		}
		return this;
	}

	public ForEachStatementSyntax WithForEachKeywordToken(SyntaxToken forEachKeywordToken)
	{
		return Update(forEachKeywordToken, IterationVariable, InKeywordToken, Expression, DoKeywordToken, Statement);
	}

	public ForEachStatementSyntax WithIterationVariable(IdentifierNameSyntax iterationVariable)
	{
		return Update(ForEachKeywordToken, iterationVariable, InKeywordToken, Expression, DoKeywordToken, Statement);
	}

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WithIn")]
	public ForEachStatementSyntax WithInKeywordToken(SyntaxToken inKeywordToken)
	{
		return Update(ForEachKeywordToken, IterationVariable, inKeywordToken, Expression, DoKeywordToken, Statement);
	}

	public ForEachStatementSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(ForEachKeywordToken, IterationVariable, InKeywordToken, expression, DoKeywordToken, Statement);
	}

	public ForEachStatementSyntax WithDoKeywordToken(SyntaxToken doKeywordToken)
	{
		return Update(ForEachKeywordToken, IterationVariable, InKeywordToken, Expression, doKeywordToken, Statement);
	}

	public ForEachStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(ForEachKeywordToken, IterationVariable, InKeywordToken, Expression, DoKeywordToken, statement);
	}
}
