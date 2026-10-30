using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class DeclarationPatternSyntax : PatternSyntax
{
	private TypeSyntax type;

	private VariableDesignationSyntax designation;

	public TypeSyntax Type => GetRedAtZero(ref type);

	public VariableDesignationSyntax Designation => GetRed(ref designation, 1);

	internal DeclarationPatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitDeclarationPattern(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitDeclarationPattern(this);
	}

	public DeclarationPatternSyntax Update(TypeSyntax type, VariableDesignationSyntax designation)
	{
		if (type != Type || designation != Designation)
		{
			DeclarationPatternSyntax declarationPatternSyntax = SyntaxFactory.DeclarationPattern(type, designation);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return declarationPatternSyntax.WithAnnotations(annotations);
			}
			return declarationPatternSyntax;
		}
		return this;
	}

	public DeclarationPatternSyntax WithType(TypeSyntax type)
	{
		return Update(type, Designation);
	}

	public DeclarationPatternSyntax WithDesignation(VariableDesignationSyntax designation)
	{
		return Update(Type, designation);
	}
}
