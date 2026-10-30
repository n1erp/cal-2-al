using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EmptyStatementSyntax : StatementSyntax
{
	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalEmptyStatementSyntax)Node)._semicolonToken, Position, 0);

	internal EmptyStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitEmptyStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEmptyStatement(this);
	}

	public EmptyStatementSyntax Update(SyntaxToken semicolonToken)
	{
		if (semicolonToken != SemicolonToken)
		{
			EmptyStatementSyntax emptyStatementSyntax = SyntaxFactory.EmptyStatement(semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return emptyStatementSyntax.WithAnnotations(annotations);
			}
			return emptyStatementSyntax;
		}
		return this;
	}

	public EmptyStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(semicolonToken);
	}
}
