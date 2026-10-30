using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MemberAccessExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	private IdentifierNameSyntax _name;

	public CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	public SyntaxToken DotToken => new SyntaxToken(this, ((InternalMemberAccessExpressionSyntax)Node)._dotToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	internal MemberAccessExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _expression), 
			2 => GetRed(ref _name, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_expression, 
			2 => _name, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAccessExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMemberAccessExpression(this);
	}

	public MemberAccessExpressionSyntax Update(CodeExpressionSyntax expression, SyntaxToken dotToken, IdentifierNameSyntax name)
	{
		if (expression != Expression || dotToken != DotToken || name != Name)
		{
			MemberAccessExpressionSyntax memberAccessExpressionSyntax = SyntaxFactory.MemberAccessExpression(expression, dotToken, name);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return memberAccessExpressionSyntax.WithAnnotations(annotations);
			}
			return memberAccessExpressionSyntax;
		}
		return this;
	}

	public MemberAccessExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression, DotToken, Name);
	}

	public MemberAccessExpressionSyntax WithDotToken(SyntaxToken dotToken)
	{
		return Update(Expression, dotToken, Name);
	}

	public MemberAccessExpressionSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Expression, DotToken, name);
	}
}
