using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class InvalidAttributeArgumentSyntax : AttributeArgumentSyntax
{
	private SyntaxNode _invalidToken;

	public SyntaxNode InvalidToken => GetRedAtZero(ref _invalidToken);

	internal InvalidAttributeArgumentSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _invalidToken);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _invalidToken;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvalidAttributeArgument(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitInvalidAttributeArgument(this);
	}

	public InvalidAttributeArgumentSyntax Update(SyntaxNode invalidToken)
	{
		if (invalidToken != InvalidToken)
		{
			InvalidAttributeArgumentSyntax invalidAttributeArgumentSyntax = SyntaxFactory.InvalidAttributeArgument(invalidToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return invalidAttributeArgumentSyntax.WithAnnotations(annotations);
			}
			return invalidAttributeArgumentSyntax;
		}
		return this;
	}

	public InvalidAttributeArgumentSyntax WithInvalidToken(SyntaxNode invalidToken)
	{
		return Update(invalidToken);
	}
}
