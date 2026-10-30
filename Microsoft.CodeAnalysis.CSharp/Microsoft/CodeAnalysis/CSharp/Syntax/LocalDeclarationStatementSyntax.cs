using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class LocalDeclarationStatementSyntax : StatementSyntax
{
	private VariableDeclarationSyntax declaration;

	public SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = Green.GetSlot(0);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, Position, 0);
			}
			return default;
		}
	}

	public VariableDeclarationSyntax Declaration => GetRed(ref declaration, 1);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.LocalDeclarationStatementSyntax)Green).semicolonToken, GetChildPosition(2), GetChildIndex(2));

	public bool IsConst => Modifiers.Any(SyntaxKind.ConstKeyword);

	internal LocalDeclarationStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref declaration, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return declaration;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLocalDeclarationStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitLocalDeclarationStatement(this);
	}

	public LocalDeclarationStatementSyntax Update(SyntaxTokenList modifiers, VariableDeclarationSyntax declaration, SyntaxToken semicolonToken)
	{
		if (modifiers != Modifiers || declaration != Declaration || semicolonToken != SemicolonToken)
		{
			LocalDeclarationStatementSyntax localDeclarationStatementSyntax = SyntaxFactory.LocalDeclarationStatement(modifiers, declaration, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return localDeclarationStatementSyntax.WithAnnotations(annotations);
			}
			return localDeclarationStatementSyntax;
		}
		return this;
	}

	public LocalDeclarationStatementSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(modifiers, Declaration, SemicolonToken);
	}

	public LocalDeclarationStatementSyntax WithDeclaration(VariableDeclarationSyntax declaration)
	{
		return Update(Modifiers, declaration, SemicolonToken);
	}

	public LocalDeclarationStatementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Modifiers, Declaration, semicolonToken);
	}

	public LocalDeclarationStatementSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}

	public LocalDeclarationStatementSyntax AddDeclarationVariables(params VariableDeclaratorSyntax[] items)
	{
		return WithDeclaration(Declaration.WithVariables(Declaration.Variables.AddRange(items)));
	}
}
