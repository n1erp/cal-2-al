using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IdentifierEqualsStringSyntax : CodeExpressionSyntax
{
	private StringLiteralValueSyntax _stringLiteral;

	public SyntaxToken Identifier => new SyntaxToken(this, ((InternalIdentifierEqualsStringSyntax)Node)._identifier, Position, 0);

	public SyntaxToken EqualsToken => new SyntaxToken(this, ((InternalIdentifierEqualsStringSyntax)Node)._equalsToken, GetChildPosition(1), GetChildIndex(1));

	public StringLiteralValueSyntax StringLiteral => GetRed(ref _stringLiteral, 2);

	internal IdentifierEqualsStringSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _stringLiteral, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _stringLiteral;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierEqualsString(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIdentifierEqualsString(this);
	}

	public IdentifierEqualsStringSyntax Update(SyntaxToken identifier, SyntaxToken equalsToken, StringLiteralValueSyntax stringLiteral)
	{
		if (identifier != Identifier || equalsToken != EqualsToken || stringLiteral != StringLiteral)
		{
			IdentifierEqualsStringSyntax identifierEqualsStringSyntax = SyntaxFactory.IdentifierEqualsString(identifier, equalsToken, stringLiteral);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return identifierEqualsStringSyntax.WithAnnotations(annotations);
			}
			return identifierEqualsStringSyntax;
		}
		return this;
	}

	public IdentifierEqualsStringSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier, EqualsToken, StringLiteral);
	}

	public IdentifierEqualsStringSyntax WithEqualsToken(SyntaxToken equalsToken)
	{
		return Update(Identifier, equalsToken, StringLiteral);
	}

	public IdentifierEqualsStringSyntax WithStringLiteral(StringLiteralValueSyntax stringLiteral)
	{
		return Update(Identifier, EqualsToken, stringLiteral);
	}
}
