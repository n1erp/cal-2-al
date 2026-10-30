using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BreakStatementSyntax : StatementSyntax
{
	public SyntaxToken BreakKeyword => new SyntaxToken(this, ((InternalBreakStatementSyntax)Node)._breakKeyword, Position, 0);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalBreakStatementSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	internal BreakStatementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBreakStatement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBreakStatement(this);
	}

	public BreakStatementSyntax Update(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
	{
		if (breakKeyword != BreakKeyword || semicolonToken != SemicolonToken)
		{
			BreakStatementSyntax breakStatementSyntax = SyntaxFactory.BreakStatement(breakKeyword, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return breakStatementSyntax.WithAnnotations(annotations);
			}
			return breakStatementSyntax;
		}
		return this;
	}

	public BreakStatementSyntax WithBreakKeyword(SyntaxToken breakKeyword)
	{
		return Update(breakKeyword, SemicolonToken);
	}

	public BreakStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(BreakKeyword, semicolonToken);
	}
}
