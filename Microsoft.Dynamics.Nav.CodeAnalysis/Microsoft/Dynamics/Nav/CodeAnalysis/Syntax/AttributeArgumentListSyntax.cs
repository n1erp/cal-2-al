using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class AttributeArgumentListSyntax : SyntaxNode
{
	private SyntaxNode _arguments;

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalAttributeArgumentListSyntax)Node)._openParenthesisToken, Position, 0);

	public SeparatedSyntaxList<AttributeArgumentSyntax> Arguments
	{
		get
		{
			SyntaxNode red = GetRed(ref _arguments, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<AttributeArgumentSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalAttributeArgumentListSyntax)Node)._closeParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	internal AttributeArgumentListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _arguments, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _arguments;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeArgumentList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitAttributeArgumentList(this);
	}

	public AttributeArgumentListSyntax Update(SyntaxToken openParenthesisToken, SeparatedSyntaxList<AttributeArgumentSyntax> arguments, SyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || arguments != Arguments || closeParenthesisToken != CloseParenthesisToken)
		{
			AttributeArgumentListSyntax attributeArgumentListSyntax = SyntaxFactory.AttributeArgumentList(openParenthesisToken, arguments, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return attributeArgumentListSyntax.WithAnnotations(annotations);
			}
			return attributeArgumentListSyntax;
		}
		return this;
	}

	public AttributeArgumentListSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(openParenthesisToken, Arguments, CloseParenthesisToken);
	}

	public AttributeArgumentListSyntax WithArguments(SeparatedSyntaxList<AttributeArgumentSyntax> arguments)
	{
		return Update(OpenParenthesisToken, arguments, CloseParenthesisToken);
	}

	public AttributeArgumentListSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OpenParenthesisToken, Arguments, closeParenthesisToken);
	}

	public AttributeArgumentListSyntax AddArguments(params AttributeArgumentSyntax[] items)
	{
		return WithArguments(Arguments.AddRange(items));
	}
}
