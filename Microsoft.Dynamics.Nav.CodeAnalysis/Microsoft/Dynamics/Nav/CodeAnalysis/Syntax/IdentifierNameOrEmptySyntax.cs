using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class IdentifierNameOrEmptySyntax : SyntaxNode
{
	private IdentifierNameSyntax _identifierName;

	public IdentifierNameSyntax IdentifierName => GetRedAtZero(ref _identifierName);

	public SyntaxToken Empty
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalIdentifierNameOrEmptySyntax)Node)._empty;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	internal IdentifierNameOrEmptySyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _identifierName);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _identifierName;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIdentifierNameOrEmpty(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitIdentifierNameOrEmpty(this);
	}

	public IdentifierNameOrEmptySyntax Update(IdentifierNameSyntax identifierName, SyntaxToken empty)
	{
		if (identifierName != IdentifierName || empty != Empty)
		{
			IdentifierNameOrEmptySyntax identifierNameOrEmptySyntax = SyntaxFactory.IdentifierNameOrEmpty(identifierName, empty);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return identifierNameOrEmptySyntax.WithAnnotations(annotations);
			}
			return identifierNameOrEmptySyntax;
		}
		return this;
	}

	public IdentifierNameOrEmptySyntax WithIdentifierName(IdentifierNameSyntax identifierName)
	{
		return Update(identifierName, Empty);
	}

	public IdentifierNameOrEmptySyntax WithEmpty(SyntaxToken empty)
	{
		return Update(IdentifierName, empty);
	}
}
