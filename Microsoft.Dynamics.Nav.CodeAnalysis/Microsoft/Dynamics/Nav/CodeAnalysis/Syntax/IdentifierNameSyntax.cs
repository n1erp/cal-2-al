using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IdentifierNameSyntax : SimpleNameSyntax
{
	public override SyntaxToken Identifier => new SyntaxToken(this, ((InternalIdentifierNameSyntax)Node)._identifier, Position, 0);

	internal IdentifierNameSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitIdentifierName(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIdentifierName(this);
	}

	public IdentifierNameSyntax Update(SyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			IdentifierNameSyntax identifierNameSyntax = SyntaxFactory.IdentifierName(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return identifierNameSyntax.WithAnnotations(annotations);
			}
			return identifierNameSyntax;
		}
		return this;
	}

	public IdentifierNameSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier);
	}
}
