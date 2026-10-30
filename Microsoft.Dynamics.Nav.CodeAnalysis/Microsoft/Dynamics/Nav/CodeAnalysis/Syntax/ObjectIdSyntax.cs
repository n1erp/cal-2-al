using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ObjectIdSyntax : CodeExpressionSyntax
{
	public SyntaxToken Value => new SyntaxToken(this, ((InternalObjectIdSyntax)Node)._value, Position, 0);

	internal ObjectIdSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitObjectId(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitObjectId(this);
	}

	public ObjectIdSyntax Update(SyntaxToken value)
	{
		if (value != Value)
		{
			ObjectIdSyntax objectIdSyntax = SyntaxFactory.ObjectId(value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return objectIdSyntax.WithAnnotations(annotations);
			}
			return objectIdSyntax;
		}
		return this;
	}

	public ObjectIdSyntax WithValue(SyntaxToken value)
	{
		return Update(value);
	}
}
