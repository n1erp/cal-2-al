using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DeclarationExpressionSyntax : ExpressionSyntax
{
	private TypeSyntax type;

	private VariableDesignationSyntax designation;

	public TypeSyntax Type => GetRedAtZero(ref type);

	public VariableDesignationSyntax Designation => GetRed(ref designation, 1);

	internal DeclarationExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref type), 
			1 => GetRed(ref designation, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)type, 
			1 => designation, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDeclarationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDeclarationExpression(this);
	}

	public DeclarationExpressionSyntax Update(TypeSyntax type, VariableDesignationSyntax designation)
	{
		if (type != Type || designation != Designation)
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = SyntaxFactory.DeclarationExpression(type, designation);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return declarationExpressionSyntax.WithAnnotations(annotations);
			}
			return declarationExpressionSyntax;
		}
		return this;
	}

	public DeclarationExpressionSyntax WithType(TypeSyntax type)
	{
		return Update(type, Designation);
	}

	public DeclarationExpressionSyntax WithDesignation(VariableDesignationSyntax designation)
	{
		return Update(Type, designation);
	}
}
