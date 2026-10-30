using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class SimpleNamedDataTypeSyntax : DataTypeSyntax
{
	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalSimpleNamedDataTypeSyntax)Node)._typeName, Position, 0);

	internal SimpleNamedDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitSimpleNamedDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSimpleNamedDataType(this);
	}

	public SimpleNamedDataTypeSyntax Update(SyntaxToken typeName)
	{
		if (typeName != TypeName)
		{
			SimpleNamedDataTypeSyntax simpleNamedDataTypeSyntax = SyntaxFactory.SimpleNamedDataType(typeName);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return simpleNamedDataTypeSyntax.WithAnnotations(annotations);
			}
			return simpleNamedDataTypeSyntax;
		}
		return this;
	}

	public SimpleNamedDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName);
	}
}
