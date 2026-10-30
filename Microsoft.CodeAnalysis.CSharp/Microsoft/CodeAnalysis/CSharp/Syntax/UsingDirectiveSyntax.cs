using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class UsingDirectiveSyntax : CSharpSyntaxNode
{
	private NameEqualsSyntax alias;

	private NameSyntax name;

	public SyntaxToken UsingKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)Green).usingKeyword, Position, 0);

	public SyntaxToken StaticKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken staticKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)Green).staticKeyword;
			if (staticKeyword != null)
			{
				return new SyntaxToken(this, staticKeyword, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public NameEqualsSyntax Alias => GetRed(ref alias, 2);

	public NameSyntax Name => GetRed(ref name, 3);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UsingDirectiveSyntax)Green).semicolonToken, GetChildPosition(4), GetChildIndex(4));

	internal UsingDirectiveSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref alias, 2), 
			3 => GetRed(ref name, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)alias, 
			3 => name, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitUsingDirective(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUsingDirective(this);
	}

	public UsingDirectiveSyntax Update(SyntaxToken usingKeyword, SyntaxToken staticKeyword, NameEqualsSyntax alias, NameSyntax name, SyntaxToken semicolonToken)
	{
		if (usingKeyword != UsingKeyword || staticKeyword != StaticKeyword || alias != Alias || name != Name || semicolonToken != SemicolonToken)
		{
			UsingDirectiveSyntax usingDirectiveSyntax = SyntaxFactory.UsingDirective(usingKeyword, staticKeyword, alias, name, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return usingDirectiveSyntax.WithAnnotations(annotations);
			}
			return usingDirectiveSyntax;
		}
		return this;
	}

	public UsingDirectiveSyntax WithUsingKeyword(SyntaxToken usingKeyword)
	{
		return Update(usingKeyword, StaticKeyword, Alias, Name, SemicolonToken);
	}

	public UsingDirectiveSyntax WithStaticKeyword(SyntaxToken staticKeyword)
	{
		return Update(UsingKeyword, staticKeyword, Alias, Name, SemicolonToken);
	}

	public UsingDirectiveSyntax WithAlias(NameEqualsSyntax alias)
	{
		return Update(UsingKeyword, StaticKeyword, alias, Name, SemicolonToken);
	}

	public UsingDirectiveSyntax WithName(NameSyntax name)
	{
		return Update(UsingKeyword, StaticKeyword, Alias, name, SemicolonToken);
	}

	public UsingDirectiveSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(UsingKeyword, StaticKeyword, Alias, Name, semicolonToken);
	}
}
