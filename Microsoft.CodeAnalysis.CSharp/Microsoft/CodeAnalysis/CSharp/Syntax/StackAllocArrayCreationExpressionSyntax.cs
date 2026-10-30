using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class StackAllocArrayCreationExpressionSyntax : ExpressionSyntax
{
	private TypeSyntax type;

	private InitializerExpressionSyntax initializer;

	public SyntaxToken StackAllocKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.StackAllocArrayCreationExpressionSyntax)Green).stackAllocKeyword, Position, 0);

	public TypeSyntax Type => GetRed(ref type, 1);

	public InitializerExpressionSyntax Initializer => GetRed(ref initializer, 2);

	internal StackAllocArrayCreationExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref type, 1), 
			2 => GetRed(ref initializer, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)type, 
			2 => initializer, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitStackAllocArrayCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitStackAllocArrayCreationExpression(this);
	}

	public StackAllocArrayCreationExpressionSyntax Update(SyntaxToken stackAllocKeyword, TypeSyntax type, InitializerExpressionSyntax initializer)
	{
		if (stackAllocKeyword != StackAllocKeyword || type != Type || initializer != Initializer)
		{
			StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpressionSyntax = SyntaxFactory.StackAllocArrayCreationExpression(stackAllocKeyword, type, initializer);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return stackAllocArrayCreationExpressionSyntax.WithAnnotations(annotations);
			}
			return stackAllocArrayCreationExpressionSyntax;
		}
		return this;
	}

	public StackAllocArrayCreationExpressionSyntax WithStackAllocKeyword(SyntaxToken stackAllocKeyword)
	{
		return Update(stackAllocKeyword, Type, Initializer);
	}

	public StackAllocArrayCreationExpressionSyntax WithType(TypeSyntax type)
	{
		return Update(StackAllocKeyword, type, Initializer);
	}

	public StackAllocArrayCreationExpressionSyntax WithInitializer(InitializerExpressionSyntax initializer)
	{
		return Update(StackAllocKeyword, Type, initializer);
	}

	public StackAllocArrayCreationExpressionSyntax Update(SyntaxToken stackAllocKeyword, TypeSyntax type)
	{
		return Update(StackAllocKeyword, type, null);
	}
}
