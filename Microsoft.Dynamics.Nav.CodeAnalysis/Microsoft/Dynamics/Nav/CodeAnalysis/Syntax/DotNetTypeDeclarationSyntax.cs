using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DotNetTypeDeclarationSyntax : SyntaxNode
{
	private NameSyntax _typeName;

	private IdentifierNameSyntax _typeAlias;

	private PropertyListSyntax _propertyList;

	public SyntaxToken Type => new SyntaxToken(this, ((InternalDotNetTypeDeclarationSyntax)Node)._type, Position, 0);

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalDotNetTypeDeclarationSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public NameSyntax TypeName => GetRed(ref _typeName, 2);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalDotNetTypeDeclarationSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	public IdentifierNameSyntax TypeAlias => GetRed(ref _typeAlias, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalDotNetTypeDeclarationSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalDotNetTypeDeclarationSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalDotNetTypeDeclarationSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal DotNetTypeDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _typeName, 2), 
			4 => GetRed(ref _typeAlias, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_typeName, 
			4 => _typeAlias, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetTypeDeclaration(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDotNetTypeDeclaration(this);
	}

	public DotNetTypeDeclarationSyntax Update(SyntaxToken type, SyntaxToken openParenthesisToken, NameSyntax typeName, SyntaxToken semicolonToken, IdentifierNameSyntax typeAlias, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (type != Type || openParenthesisToken != OpenParenthesisToken || typeName != TypeName || semicolonToken != SemicolonToken || typeAlias != TypeAlias || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			DotNetTypeDeclarationSyntax dotNetTypeDeclarationSyntax = SyntaxFactory.DotNetTypeDeclaration(type, openParenthesisToken, typeName, semicolonToken, typeAlias, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dotNetTypeDeclarationSyntax.WithAnnotations(annotations);
			}
			return dotNetTypeDeclarationSyntax;
		}
		return this;
	}

	public DotNetTypeDeclarationSyntax WithType(SyntaxToken type)
	{
		return Update(type, OpenParenthesisToken, TypeName, SemicolonToken, TypeAlias, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(Type, openParenthesisToken, TypeName, SemicolonToken, TypeAlias, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithTypeName(NameSyntax typeName)
	{
		return Update(Type, OpenParenthesisToken, typeName, SemicolonToken, TypeAlias, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Type, OpenParenthesisToken, TypeName, semicolonToken, TypeAlias, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithTypeAlias(IdentifierNameSyntax typeAlias)
	{
		return Update(Type, OpenParenthesisToken, TypeName, SemicolonToken, typeAlias, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(Type, OpenParenthesisToken, TypeName, SemicolonToken, TypeAlias, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(Type, OpenParenthesisToken, TypeName, SemicolonToken, TypeAlias, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(Type, OpenParenthesisToken, TypeName, SemicolonToken, TypeAlias, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public DotNetTypeDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(Type, OpenParenthesisToken, TypeName, SemicolonToken, TypeAlias, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public DotNetTypeDeclarationSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
