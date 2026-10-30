using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class DotNetDataTypeSyntax : DataTypeSyntax
{
	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalDotNetDataTypeSyntax)Node)._typeName, Position, 0);

	internal DotNetDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDotNetDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitDotNetDataType(this);
	}

	public DotNetDataTypeSyntax Update(SyntaxToken typeName)
	{
		if (typeName != TypeName)
		{
			DotNetDataTypeSyntax dotNetDataTypeSyntax = SyntaxFactory.DotNetDataType(typeName);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return dotNetDataTypeSyntax.WithAnnotations(annotations);
			}
			return dotNetDataTypeSyntax;
		}
		return this;
	}

	public DotNetDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName);
	}
}
