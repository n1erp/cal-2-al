using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class CaseLineSyntax : SyntaxNode
{
	private SyntaxNode _expressions;

	private StatementSyntax _statement;

	public SeparatedSyntaxList<CodeExpressionSyntax> Expressions
	{
		get
		{
			SyntaxNode red = GetRed(ref _expressions, 0);
			if (red != null)
			{
				return new SeparatedSyntaxList<CodeExpressionSyntax>(red, 0);
			}
			return default;
		}
	}

	public SyntaxToken ColonToken => new SyntaxToken(this, ((InternalCaseLineSyntax)Node)._colonToken, GetChildPosition(1), GetChildIndex(1));

	public StatementSyntax Statement => GetRed(ref _statement, 2);

	internal CaseLineSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _expressions), 
			2 => GetRed(ref _statement, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _expressions, 
			2 => _statement, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCaseLine(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitCaseLine(this);
	}

	public CaseLineSyntax Update(SeparatedSyntaxList<CodeExpressionSyntax> expressions, SyntaxToken colonToken, StatementSyntax statement)
	{
		if (expressions != Expressions || colonToken != ColonToken || statement != Statement)
		{
			CaseLineSyntax caseLineSyntax = SyntaxFactory.CaseLine(expressions, colonToken, statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return caseLineSyntax.WithAnnotations(annotations);
			}
			return caseLineSyntax;
		}
		return this;
	}

	public CaseLineSyntax WithExpressions(SeparatedSyntaxList<CodeExpressionSyntax> expressions)
	{
		return Update(expressions, ColonToken, Statement);
	}

	public CaseLineSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Expressions, colonToken, Statement);
	}

	public CaseLineSyntax WithStatement(StatementSyntax statement)
	{
		return Update(Expressions, ColonToken, statement);
	}

	public CaseLineSyntax AddExpressions(params CodeExpressionSyntax[] items)
	{
		return WithExpressions(Expressions.AddRange(items));
	}
}
