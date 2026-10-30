using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DimensionSyntax : SyntaxNode
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalDimensionSyntax)Node)._value, Position, 0);

	internal DimensionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDimension(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDimension(this);
	}

	public DimensionSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			DimensionSyntax dimensionSyntax = SyntaxFactory.Dimension(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dimensionSyntax.WithAnnotations(annotations);
			}
			return dimensionSyntax;
		}
		return this;
	}

	public DimensionSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
