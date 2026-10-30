using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class GenericNamedDataTypeSyntax : DataTypeSyntax
{
	private SyntaxNode _typeArguments;

	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalGenericNamedDataTypeSyntax)Node)._typeName, Position, 0);

	public SyntaxToken OfKeyword => new SyntaxToken(this, ((InternalGenericNamedDataTypeSyntax)Node)._ofKeyword, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken OpenBracketToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalGenericNamedDataTypeSyntax)Node)._openBracketToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	public SeparatedSyntaxList<DataTypeSyntax> TypeArguments
	{
		get
		{
			SyntaxNode red = GetRed(ref _typeArguments, 3);
			if (red != null)
			{
				return new SeparatedSyntaxList<DataTypeSyntax>(red, GetChildIndex(3));
			}
			return default;
		}
	}

	public SyntaxToken CloseBracketToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalGenericNamedDataTypeSyntax)Node)._closeBracketToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(4), GetChildIndex(4));
			}
			return default;
		}
	}

	internal GenericNamedDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 3)
		{
			return GetRed(ref _typeArguments, 3);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 3)
		{
			return _typeArguments;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGenericNamedDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitGenericNamedDataType(this);
	}

	public GenericNamedDataTypeSyntax Update(SyntaxToken typeName, SyntaxToken ofKeyword, SyntaxToken openBracketToken, SeparatedSyntaxList<DataTypeSyntax> typeArguments, SyntaxToken closeBracketToken)
	{
		if (typeName != TypeName || ofKeyword != OfKeyword || openBracketToken != OpenBracketToken || typeArguments != TypeArguments || closeBracketToken != CloseBracketToken)
		{
			GenericNamedDataTypeSyntax genericNamedDataTypeSyntax = SyntaxFactory.GenericNamedDataType(typeName, ofKeyword, openBracketToken, typeArguments, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return genericNamedDataTypeSyntax.WithAnnotations(annotations);
			}
			return genericNamedDataTypeSyntax;
		}
		return this;
	}

	public GenericNamedDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, OfKeyword, OpenBracketToken, TypeArguments, CloseBracketToken);
	}

	public GenericNamedDataTypeSyntax WithOfKeyword(SyntaxToken ofKeyword)
	{
		return Update(TypeName, ofKeyword, OpenBracketToken, TypeArguments, CloseBracketToken);
	}

	public GenericNamedDataTypeSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(TypeName, OfKeyword, openBracketToken, TypeArguments, CloseBracketToken);
	}

	public GenericNamedDataTypeSyntax WithTypeArguments(SeparatedSyntaxList<DataTypeSyntax> typeArguments)
	{
		return Update(TypeName, OfKeyword, OpenBracketToken, typeArguments, CloseBracketToken);
	}

	public GenericNamedDataTypeSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(TypeName, OfKeyword, OpenBracketToken, TypeArguments, closeBracketToken);
	}

	public GenericNamedDataTypeSyntax AddTypeArguments(params DataTypeSyntax[] items)
	{
		return WithTypeArguments(TypeArguments.AddRange(items));
	}
}
