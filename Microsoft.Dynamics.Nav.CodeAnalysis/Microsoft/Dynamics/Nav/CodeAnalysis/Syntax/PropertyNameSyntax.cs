using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PropertyNameSyntax : SyntaxNode
{
	public SyntaxToken Identifier => new SyntaxToken(this, ((InternalPropertyNameSyntax)Node)._identifier, Position, 0);

	internal PropertyNameSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitPropertyName(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPropertyName(this);
	}

	public PropertyNameSyntax Update(SyntaxToken identifier)
	{
		if (identifier != Identifier)
		{
			PropertyNameSyntax propertyNameSyntax = SyntaxFactory.PropertyName(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return propertyNameSyntax.WithAnnotations(annotations);
			}
			return propertyNameSyntax;
		}
		return this;
	}

	public PropertyNameSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(identifier);
	}
}
