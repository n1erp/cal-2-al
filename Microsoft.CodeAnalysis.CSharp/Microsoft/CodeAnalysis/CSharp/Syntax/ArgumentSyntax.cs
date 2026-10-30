using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ArgumentSyntax : CSharpSyntaxNode
{
	private NameColonSyntax nameColon;

	private ExpressionSyntax expression;

	public NameColonSyntax NameColon => GetRedAtZero(ref nameColon);

	public SyntaxToken RefKindKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken refKindKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArgumentSyntax)Green).refKindKeyword;
			if (refKindKeyword != null)
			{
				return new SyntaxToken(this, refKindKeyword, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SyntaxToken RefOrOutKeyword => RefKindKeyword;

	internal ArgumentSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref nameColon), 
			2 => GetRed(ref expression, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)nameColon, 
			2 => expression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArgument(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArgument(this);
	}

	public ArgumentSyntax Update(NameColonSyntax nameColon, SyntaxToken refKindKeyword, ExpressionSyntax expression)
	{
		if (nameColon != NameColon || refKindKeyword != RefKindKeyword || expression != Expression)
		{
			ArgumentSyntax argumentSyntax = SyntaxFactory.Argument(nameColon, refKindKeyword, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return argumentSyntax.WithAnnotations(annotations);
			}
			return argumentSyntax;
		}
		return this;
	}

	public ArgumentSyntax WithNameColon(NameColonSyntax nameColon)
	{
		return Update(nameColon, RefKindKeyword, Expression);
	}

	public ArgumentSyntax WithRefKindKeyword(SyntaxToken refKindKeyword)
	{
		return Update(NameColon, refKindKeyword, Expression);
	}

	public ArgumentSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(NameColon, RefKindKeyword, expression);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public ArgumentSyntax WithRefOrOutKeyword(SyntaxToken refOrOutKeyword)
	{
		return Update(NameColon, refOrOutKeyword, Expression);
	}
}
