using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class VariableDeclarationSyntax : CSharpSyntaxNode
{
	private TypeSyntax type;

	private SyntaxNode variables;

	public TypeSyntax Type => GetRedAtZero(ref type);

	public SeparatedSyntaxList<VariableDeclaratorSyntax> Variables
	{
		get
		{
			SyntaxNode red = GetRed(ref variables, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<VariableDeclaratorSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	internal VariableDeclarationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref type), 
			1 => GetRed(ref variables, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => type, 
			1 => variables, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVariableDeclaration(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclaration(this);
	}

	public VariableDeclarationSyntax Update(TypeSyntax type, SeparatedSyntaxList<VariableDeclaratorSyntax> variables)
	{
		if (type != Type || variables != Variables)
		{
			VariableDeclarationSyntax variableDeclarationSyntax = SyntaxFactory.VariableDeclaration(type, variables);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return variableDeclarationSyntax.WithAnnotations(annotations);
			}
			return variableDeclarationSyntax;
		}
		return this;
	}

	public VariableDeclarationSyntax WithType(TypeSyntax type)
	{
		return Update(type, Variables);
	}

	public VariableDeclarationSyntax WithVariables(SeparatedSyntaxList<VariableDeclaratorSyntax> variables)
	{
		return Update(Type, variables);
	}

	public VariableDeclarationSyntax AddVariables(params VariableDeclaratorSyntax[] items)
	{
		return WithVariables(Variables.AddRange(items));
	}
}
