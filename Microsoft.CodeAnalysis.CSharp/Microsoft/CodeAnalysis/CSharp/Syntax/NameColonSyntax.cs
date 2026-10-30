using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class NameColonSyntax : CSharpSyntaxNode
{
	private IdentifierNameSyntax name;

	public IdentifierNameSyntax Name => GetRedAtZero(ref name);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.NameColonSyntax)Green).colonToken, GetChildPosition(1), GetChildIndex(1));

	internal NameColonSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref name);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return name;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNameColon(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNameColon(this);
	}

	public NameColonSyntax Update(IdentifierNameSyntax name, SyntaxToken colonToken)
	{
		if (name != Name || colonToken != ColonToken)
		{
			NameColonSyntax nameColonSyntax = SyntaxFactory.NameColon(name, colonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return nameColonSyntax.WithAnnotations(annotations);
			}
			return nameColonSyntax;
		}
		return this;
	}

	public NameColonSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(name, ColonToken);
	}

	public NameColonSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Name, colonToken);
	}
}
