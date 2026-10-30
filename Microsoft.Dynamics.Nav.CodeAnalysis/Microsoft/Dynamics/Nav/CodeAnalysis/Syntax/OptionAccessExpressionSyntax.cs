using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class OptionAccessExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _expression;

	private IdentifierNameSyntax _name;

	public CodeExpressionSyntax Expression => GetRedAtZero(ref _expression);

	public SyntaxToken ColonColonToken => new SyntaxToken(this, ((InternalOptionAccessExpressionSyntax)Node)._colonColonToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	internal OptionAccessExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitOptionAccessExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitOptionAccessExpression(this);
	}

	public OptionAccessExpressionSyntax Update(CodeExpressionSyntax expression, SyntaxToken colonColonToken, IdentifierNameSyntax name)
	{
		if (expression != Expression || colonColonToken != ColonColonToken || name != Name)
		{
			OptionAccessExpressionSyntax optionAccessExpressionSyntax = SyntaxFactory.OptionAccessExpression(expression, colonColonToken, name);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return optionAccessExpressionSyntax.WithAnnotations(annotations);
			}
			return optionAccessExpressionSyntax;
		}
		return this;
	}

	public OptionAccessExpressionSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(expression, ColonColonToken, Name);
	}

	public OptionAccessExpressionSyntax WithColonColonToken(SyntaxToken colonColonToken)
	{
		return Update(Expression, colonColonToken, Name);
	}

	public OptionAccessExpressionSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Expression, ColonColonToken, name);
	}
}
