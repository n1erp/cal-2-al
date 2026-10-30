using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ObjectNameOrIdSyntax : SyntaxNode
{
	private CodeExpressionSyntax _identifier;

	public CodeExpressionSyntax Identifier => GetRedAtZero(ref _identifier);

	internal ObjectNameOrIdSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitObjectNameOrId(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitObjectNameOrId(this);
	}

	public ObjectNameOrIdSyntax Update(CodeExpressionSyntax identifier)
	{
		if (identifier != Identifier)
		{
			ObjectNameOrIdSyntax objectNameOrIdSyntax = SyntaxFactory.ObjectNameOrId(identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return objectNameOrIdSyntax.WithAnnotations(annotations);
			}
			return objectNameOrIdSyntax;
		}
		return this;
	}

	public ObjectNameOrIdSyntax WithIdentifier(CodeExpressionSyntax identifier)
	{
		return Update(identifier);
	}
}
