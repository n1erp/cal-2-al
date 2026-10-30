using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ParenthesizedVariableDesignationSyntax : VariableDesignationSyntax
{
	private SyntaxNode variables;

	public SyntaxToken OpenParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedVariableDesignationSyntax)Green).openParenToken, Position, 0);

	public SeparatedSyntaxList<VariableDesignationSyntax> Variables
	{
		get
		{
			SyntaxNode red = GetRed(ref variables, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<VariableDesignationSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedVariableDesignationSyntax)Green).closeParenToken, GetChildPosition(2), GetChildIndex(2));

	internal ParenthesizedVariableDesignationSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref variables, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return variables;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedVariableDesignation(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedVariableDesignation(this);
	}

	public ParenthesizedVariableDesignationSyntax Update(SyntaxToken openParenToken, SeparatedSyntaxList<VariableDesignationSyntax> variables, SyntaxToken closeParenToken)
	{
		if (openParenToken != OpenParenToken || variables != Variables || closeParenToken != CloseParenToken)
		{
			ParenthesizedVariableDesignationSyntax parenthesizedVariableDesignationSyntax = SyntaxFactory.ParenthesizedVariableDesignation(openParenToken, variables, closeParenToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parenthesizedVariableDesignationSyntax.WithAnnotations(annotations);
			}
			return parenthesizedVariableDesignationSyntax;
		}
		return this;
	}

	public ParenthesizedVariableDesignationSyntax WithOpenParenToken(SyntaxToken openParenToken)
	{
		return Update(openParenToken, Variables, CloseParenToken);
	}

	public ParenthesizedVariableDesignationSyntax WithVariables(SeparatedSyntaxList<VariableDesignationSyntax> variables)
	{
		return Update(OpenParenToken, variables, CloseParenToken);
	}

	public ParenthesizedVariableDesignationSyntax WithCloseParenToken(SyntaxToken closeParenToken)
	{
		return Update(OpenParenToken, Variables, closeParenToken);
	}

	public ParenthesizedVariableDesignationSyntax AddVariables(params VariableDesignationSyntax[] items)
	{
		return WithVariables(Variables.AddRange(items));
	}
}
