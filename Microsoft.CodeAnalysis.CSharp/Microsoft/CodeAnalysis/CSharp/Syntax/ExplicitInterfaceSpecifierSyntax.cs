using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ExplicitInterfaceSpecifierSyntax : CSharpSyntaxNode
{
	private NameSyntax name;

	public NameSyntax Name => GetRedAtZero(ref name);

	public SyntaxToken DotToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ExplicitInterfaceSpecifierSyntax)Green).dotToken, GetChildPosition(1), GetChildIndex(1));

	internal ExplicitInterfaceSpecifierSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitExplicitInterfaceSpecifier(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExplicitInterfaceSpecifier(this);
	}

	public ExplicitInterfaceSpecifierSyntax Update(NameSyntax name, SyntaxToken dotToken)
	{
		if (name != Name || dotToken != DotToken)
		{
			ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifierSyntax = SyntaxFactory.ExplicitInterfaceSpecifier(name, dotToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return explicitInterfaceSpecifierSyntax.WithAnnotations(annotations);
			}
			return explicitInterfaceSpecifierSyntax;
		}
		return this;
	}

	public ExplicitInterfaceSpecifierSyntax WithName(NameSyntax name)
	{
		return Update(name, DotToken);
	}

	public ExplicitInterfaceSpecifierSyntax WithDotToken(SyntaxToken dotToken)
	{
		return Update(Name, dotToken);
	}
}
