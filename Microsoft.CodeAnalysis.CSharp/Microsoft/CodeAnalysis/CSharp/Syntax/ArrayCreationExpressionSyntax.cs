using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ArrayCreationExpressionSyntax : ExpressionSyntax
{
	private ArrayTypeSyntax type;

	private InitializerExpressionSyntax initializer;

	public SyntaxToken NewKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ArrayCreationExpressionSyntax)Green).newKeyword, Position, 0);

	public ArrayTypeSyntax Type => GetRed(ref type, 1);

	public InitializerExpressionSyntax Initializer => GetRed(ref initializer, 2);

	internal ArrayCreationExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitArrayCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitArrayCreationExpression(this);
	}

	public ArrayCreationExpressionSyntax Update(SyntaxToken newKeyword, ArrayTypeSyntax type, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || type != Type || initializer != Initializer)
		{
			ArrayCreationExpressionSyntax arrayCreationExpressionSyntax = SyntaxFactory.ArrayCreationExpression(newKeyword, type, initializer);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return arrayCreationExpressionSyntax.WithAnnotations(annotations);
			}
			return arrayCreationExpressionSyntax;
		}
		return this;
	}

	public ArrayCreationExpressionSyntax WithNewKeyword(SyntaxToken newKeyword)
	{
		return Update(newKeyword, Type, Initializer);
	}

	public ArrayCreationExpressionSyntax WithType(ArrayTypeSyntax type)
	{
		return Update(NewKeyword, type, Initializer);
	}

	public ArrayCreationExpressionSyntax WithInitializer(InitializerExpressionSyntax initializer)
	{
		return Update(NewKeyword, Type, initializer);
	}

	public ArrayCreationExpressionSyntax AddTypeRankSpecifiers(params ArrayRankSpecifierSyntax[] items)
	{
		return WithType(Type.WithRankSpecifiers(Type.RankSpecifiers.AddRange(items)));
	}
}
