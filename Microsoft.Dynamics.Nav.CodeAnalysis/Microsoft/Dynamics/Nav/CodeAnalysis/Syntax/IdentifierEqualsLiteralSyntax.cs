using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IdentifierEqualsLiteralSyntax : CodeExpressionSyntax
{
	private LiteralValueSyntax _literal;

	public SyntaxToken Identifier => new SyntaxToken(this, ((InternalIdentifierEqualsLiteralSyntax)Node)._identifier, Position, 0);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalIdentifierEqualsLiteralSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public LiteralValueSyntax Literal => GetRed(ref _literal, 2);

	internal IdentifierEqualsLiteralSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _literal, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _literal;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierEqualsLiteral(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIdentifierEqualsLiteral(this);
	}

	public IdentifierEqualsLiteralSyntax Update(SyntaxToken identifier, SyntaxToken equalsToken, LiteralValueSyntax literal)
	{
		if (identifier != Identifier || equalsToken != EqualsToken || literal != Literal)
		{
			IdentifierEqualsLiteralSyntax identifierEqualsLiteralSyntax = SyntaxFactory.IdentifierEqualsLiteral(identifier, equalsToken, literal);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return identifierEqualsLiteralSyntax.WithAnnotations(annotations);
			}
			return identifierEqualsLiteralSyntax;
		}
		return this;
	}

	public IdentifierEqualsLiteralSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier, EqualsToken, Literal);
	}

	public IdentifierEqualsLiteralSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(Identifier, equalsToken, Literal);
	}

	public IdentifierEqualsLiteralSyntax WithLiteral(LiteralValueSyntax literal)
	{
		return Update(Identifier, EqualsToken, literal);
	}
}
