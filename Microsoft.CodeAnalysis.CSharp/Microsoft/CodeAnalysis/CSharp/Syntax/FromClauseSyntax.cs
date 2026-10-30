using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class FromClauseSyntax : QueryClauseSyntax
{
	private TypeSyntax type;

	private ExpressionSyntax expression;

	public SyntaxToken FromKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FromClauseSyntax)Green).fromKeyword, Position, 0);

	public TypeSyntax Type => GetRed(ref type, 1);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FromClauseSyntax)Green).identifier, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken InKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FromClauseSyntax)Green).inKeyword, GetChildPosition(3), GetChildIndex(3));

	public ExpressionSyntax Expression => GetRed(ref expression, 4);

	internal FromClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref type, 1), 
			4 => GetRed(ref expression, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)type, 
			4 => expression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFromClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFromClause(this);
	}

	public FromClauseSyntax Update(SyntaxToken fromKeyword, TypeSyntax type, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax expression)
	{
		if (fromKeyword != FromKeyword || type != Type || identifier != Identifier || inKeyword != InKeyword || expression != Expression)
		{
			FromClauseSyntax fromClauseSyntax = SyntaxFactory.FromClause(fromKeyword, type, identifier, inKeyword, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fromClauseSyntax.WithAnnotations(annotations);
			}
			return fromClauseSyntax;
		}
		return this;
	}

	public FromClauseSyntax WithFromKeyword(SyntaxToken fromKeyword)
	{
		return Update(fromKeyword, Type, Identifier, InKeyword, Expression);
	}

	public FromClauseSyntax WithType(TypeSyntax type)
	{
		return Update(FromKeyword, type, Identifier, InKeyword, Expression);
	}

	public FromClauseSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(FromKeyword, Type, identifier, InKeyword, Expression);
	}

	public FromClauseSyntax WithInKeyword(SyntaxToken inKeyword)
	{
		return Update(FromKeyword, Type, Identifier, inKeyword, Expression);
	}

	public FromClauseSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(FromKeyword, Type, Identifier, InKeyword, expression);
	}
}
