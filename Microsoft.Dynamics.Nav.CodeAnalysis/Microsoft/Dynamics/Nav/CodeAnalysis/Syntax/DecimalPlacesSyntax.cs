using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DecimalPlacesSyntax : SyntaxNode
{
	public SyntaxToken MinimumToken => new SyntaxToken(this, ((InternalDecimalPlacesSyntax)Node)._minimumToken, Position, 0);

	public SyntaxToken ColonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalDecimalPlacesSyntax)Node)._colonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken MaximumToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalDecimalPlacesSyntax)Node)._maximumToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	internal DecimalPlacesSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDecimalPlaces(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDecimalPlaces(this);
	}

	public DecimalPlacesSyntax Update(SyntaxToken minimumToken, SyntaxToken colonToken, SyntaxToken maximumToken)
	{
		if (minimumToken != MinimumToken || colonToken != ColonToken || maximumToken != MaximumToken)
		{
			DecimalPlacesSyntax decimalPlacesSyntax = SyntaxFactory.DecimalPlaces(minimumToken, colonToken, maximumToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return decimalPlacesSyntax.WithAnnotations(annotations);
			}
			return decimalPlacesSyntax;
		}
		return this;
	}

	public DecimalPlacesSyntax WithMinimumToken(SyntaxToken minimumToken)
	{
		return Update(minimumToken, ColonToken, MaximumToken);
	}

	public DecimalPlacesSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(MinimumToken, colonToken, MaximumToken);
	}

	public DecimalPlacesSyntax WithMaximumToken(SyntaxToken maximumToken)
	{
		return Update(MinimumToken, ColonToken, maximumToken);
	}
}
