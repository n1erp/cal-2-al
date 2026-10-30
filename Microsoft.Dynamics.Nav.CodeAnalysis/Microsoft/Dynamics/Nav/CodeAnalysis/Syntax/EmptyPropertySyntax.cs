using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EmptyPropertySyntax : PropertySyntaxOrEmpty
{
	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalEmptyPropertySyntax)Node)._semicolonToken, Position, 0);

	internal EmptyPropertySyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEmptyProperty(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEmptyProperty(this);
	}

	public EmptyPropertySyntax Update(SyntaxToken semicolonToken)
	{
		if (semicolonToken != SemicolonToken)
		{
			EmptyPropertySyntax emptyPropertySyntax = SyntaxFactory.EmptyProperty(semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return emptyPropertySyntax.WithAnnotations(annotations);
			}
			return emptyPropertySyntax;
		}
		return this;
	}

	public EmptyPropertySyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(semicolonToken);
	}
}
