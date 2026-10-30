using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ObjectNameReferenceSyntax : SyntaxNode
{
	private CodeExpressionSyntax _identifier;

	public CodeExpressionSyntax Identifier => GetRedAtZero(ref _identifier);

	internal ObjectNameReferenceSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _identifier);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _identifier;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectNameReference(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitObjectNameReference(this);
	}

	public ObjectNameReferenceSyntax Update(CodeExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			ObjectNameReferenceSyntax objectNameReferenceSyntax = SyntaxFactory.ObjectNameReference(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return objectNameReferenceSyntax.WithAnnotations(annotations);
			}
			return objectNameReferenceSyntax;
		}
		return this;
	}

	public ObjectNameReferenceSyntax WithIdentifier(CodeExpressionSyntax identifier)
	{
		return Update(identifier);
	}
}
