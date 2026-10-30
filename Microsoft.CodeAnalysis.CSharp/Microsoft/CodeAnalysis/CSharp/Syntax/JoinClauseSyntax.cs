using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class JoinClauseSyntax : QueryClauseSyntax
{
	private TypeSyntax type;

	private ExpressionSyntax inExpression;

	private ExpressionSyntax leftExpression;

	private ExpressionSyntax rightExpression;

	private JoinIntoClauseSyntax into;

	public SyntaxToken JoinKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinClauseSyntax)Green).joinKeyword, Position, 0);

	public TypeSyntax Type => GetRed(ref type, 1);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinClauseSyntax)Green).identifier, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken InKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinClauseSyntax)Green).inKeyword, GetChildPosition(3), GetChildIndex(3));

	public ExpressionSyntax InExpression => GetRed(ref inExpression, 4);

	public SyntaxToken OnKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinClauseSyntax)Green).onKeyword, GetChildPosition(5), GetChildIndex(5));

	public ExpressionSyntax LeftExpression => GetRed(ref leftExpression, 6);

	public SyntaxToken EqualsKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.JoinClauseSyntax)Green).equalsKeyword, GetChildPosition(7), GetChildIndex(7));

	public ExpressionSyntax RightExpression => GetRed(ref rightExpression, 8);

	public JoinIntoClauseSyntax Into => GetRed(ref into, 9);

	internal JoinClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref type, 1), 
			4 => GetRed(ref inExpression, 4), 
			6 => GetRed(ref leftExpression, 6), 
			8 => GetRed(ref rightExpression, 8), 
			9 => GetRed(ref into, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)type, 
			4 => inExpression, 
			6 => leftExpression, 
			8 => rightExpression, 
			9 => into, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitJoinClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitJoinClause(this);
	}

	public JoinClauseSyntax Update(SyntaxToken joinKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax inExpression, SyntaxToken onKeyword, ExpressionSyntax leftExpression, SyntaxToken equalsKeyword, ExpressionSyntax rightExpression, JoinIntoClauseSyntax into)
	{
		if (joinKeyword != JoinKeyword || type != Type || identifier != Identifier || inKeyword != InKeyword || inExpression != InExpression || onKeyword != OnKeyword || leftExpression != LeftExpression || equalsKeyword != EqualsKeyword || rightExpression != RightExpression || into != Into)
		{
			JoinClauseSyntax joinClauseSyntax = SyntaxFactory.JoinClause(joinKeyword, type, identifier, inKeyword, inExpression, onKeyword, leftExpression, equalsKeyword, rightExpression, into);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return joinClauseSyntax.WithAnnotations(annotations);
			}
			return joinClauseSyntax;
		}
		return this;
	}

	public JoinClauseSyntax WithJoinKeyword(SyntaxToken joinKeyword)
	{
		return Update(joinKeyword, Type, Identifier, InKeyword, InExpression, OnKeyword, LeftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithType(TypeSyntax type)
	{
		return Update(JoinKeyword, type, Identifier, InKeyword, InExpression, OnKeyword, LeftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(JoinKeyword, Type, identifier, InKeyword, InExpression, OnKeyword, LeftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithInKeyword(SyntaxToken inKeyword)
	{
		return Update(JoinKeyword, Type, Identifier, inKeyword, InExpression, OnKeyword, LeftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithInExpression(ExpressionSyntax inExpression)
	{
		return Update(JoinKeyword, Type, Identifier, InKeyword, inExpression, OnKeyword, LeftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithOnKeyword(SyntaxToken onKeyword)
	{
		return Update(JoinKeyword, Type, Identifier, InKeyword, InExpression, onKeyword, LeftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithLeftExpression(ExpressionSyntax leftExpression)
	{
		return Update(JoinKeyword, Type, Identifier, InKeyword, InExpression, OnKeyword, leftExpression, EqualsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithEqualsKeyword(SyntaxToken equalsKeyword)
	{
		return Update(JoinKeyword, Type, Identifier, InKeyword, InExpression, OnKeyword, LeftExpression, equalsKeyword, RightExpression, Into);
	}

	public JoinClauseSyntax WithRightExpression(ExpressionSyntax rightExpression)
	{
		return Update(JoinKeyword, Type, Identifier, InKeyword, InExpression, OnKeyword, LeftExpression, EqualsKeyword, rightExpression, Into);
	}

	public JoinClauseSyntax WithInto(JoinIntoClauseSyntax into)
	{
		return Update(JoinKeyword, Type, Identifier, InKeyword, InExpression, OnKeyword, LeftExpression, EqualsKeyword, RightExpression, into);
	}
}
