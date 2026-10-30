using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class NamespaceDeclarationSyntax : MemberDeclarationSyntax
{
	private NameSyntax name;

	private SyntaxNode externs;

	private SyntaxNode usings;

	private SyntaxNode members;

	public SyntaxToken NamespaceKeyword => new SyntaxToken(this, Green.namespaceKeyword, Position, 0);

	public NameSyntax Name => GetRed(ref name, 1);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, Green.openBraceToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxList<ExternAliasDirectiveSyntax> Externs => new SyntaxList<ExternAliasDirectiveSyntax>(GetRed(ref externs, 3));

	public SyntaxList<UsingDirectiveSyntax> Usings => new SyntaxList<UsingDirectiveSyntax>(GetRed(ref usings, 4));

	public SyntaxList<MemberDeclarationSyntax> Members => new SyntaxList<MemberDeclarationSyntax>(GetRed(ref members, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, Green.closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	public SyntaxToken SemicolonToken
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken semicolonToken = Green.semicolonToken;
			if (semicolonToken != null)
			{
				return new SyntaxToken(this, semicolonToken, GetChildPosition(7), GetChildIndex(7));
			}
			return default;
		}
	}

	internal new Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NamespaceDeclarationSyntax Green => (Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NamespaceDeclarationSyntax)base.Green;

	internal NamespaceDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref name, 1), 
			3 => GetRed(ref externs, 3), 
			4 => GetRed(ref usings, 4), 
			5 => GetRed(ref members, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => name, 
			3 => externs, 
			4 => usings, 
			5 => members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNamespaceDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNamespaceDeclaration(this);
	}

	public NamespaceDeclarationSyntax Update(SyntaxToken namespaceKeyword, NameSyntax name, SyntaxToken openBraceToken, SyntaxList<ExternAliasDirectiveSyntax> externs, SyntaxList<UsingDirectiveSyntax> usings, SyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
	{
		if (namespaceKeyword != NamespaceKeyword || name != Name || openBraceToken != OpenBraceToken || externs != Externs || usings != Usings || members != Members || closeBraceToken != CloseBraceToken || semicolonToken != SemicolonToken)
		{
			NamespaceDeclarationSyntax namespaceDeclarationSyntax = SyntaxFactory.NamespaceDeclaration(namespaceKeyword, name, openBraceToken, externs, usings, members, closeBraceToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return namespaceDeclarationSyntax.WithAnnotations(annotations);
			}
			return namespaceDeclarationSyntax;
		}
		return this;
	}

	public NamespaceDeclarationSyntax WithNamespaceKeyword(SyntaxToken namespaceKeyword)
	{
		return Update(namespaceKeyword, Name, OpenBraceToken, Externs, Usings, Members, CloseBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithName(NameSyntax name)
	{
		return Update(NamespaceKeyword, name, OpenBraceToken, Externs, Usings, Members, CloseBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(NamespaceKeyword, Name, openBraceToken, Externs, Usings, Members, CloseBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithExterns(SyntaxList<ExternAliasDirectiveSyntax> externs)
	{
		return Update(NamespaceKeyword, Name, OpenBraceToken, externs, Usings, Members, CloseBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithUsings(SyntaxList<UsingDirectiveSyntax> usings)
	{
		return Update(NamespaceKeyword, Name, OpenBraceToken, Externs, usings, Members, CloseBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithMembers(SyntaxList<MemberDeclarationSyntax> members)
	{
		return Update(NamespaceKeyword, Name, OpenBraceToken, Externs, Usings, members, CloseBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(NamespaceKeyword, Name, OpenBraceToken, Externs, Usings, Members, closeBraceToken, SemicolonToken);
	}

	public NamespaceDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(NamespaceKeyword, Name, OpenBraceToken, Externs, Usings, Members, CloseBraceToken, semicolonToken);
	}

	public NamespaceDeclarationSyntax AddExterns(params ExternAliasDirectiveSyntax[] items)
	{
		return WithExterns(Externs.AddRange(items));
	}

	public NamespaceDeclarationSyntax AddUsings(params UsingDirectiveSyntax[] items)
	{
		return WithUsings(Usings.AddRange(items));
	}

	public NamespaceDeclarationSyntax AddMembers(params MemberDeclarationSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
