using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ObjectCreationExpressionSyntax : ExpressionSyntax
{
	private TypeSyntax type;

	private ArgumentListSyntax argumentList;

	private InitializerExpressionSyntax initializer;

	public SyntaxToken NewKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ObjectCreationExpressionSyntax)Green).newKeyword, Position, 0);

	public TypeSyntax Type => GetRed(ref type, 1);

	public ArgumentListSyntax ArgumentList => GetRed(ref argumentList, 2);

	public InitializerExpressionSyntax Initializer => GetRed(ref initializer, 3);

	internal ObjectCreationExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref type, 1), 
			2 => GetRed(ref argumentList, 2), 
			3 => GetRed(ref initializer, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)type, 
			2 => argumentList, 
			3 => initializer, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitObjectCreationExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitObjectCreationExpression(this);
	}

	public ObjectCreationExpressionSyntax Update(SyntaxToken newKeyword, TypeSyntax type, ArgumentListSyntax argumentList, InitializerExpressionSyntax initializer)
	{
		if (newKeyword != NewKeyword || type != Type || argumentList != ArgumentList || initializer != Initializer)
		{
			ObjectCreationExpressionSyntax objectCreationExpressionSyntax = SyntaxFactory.ObjectCreationExpression(newKeyword, type, argumentList, initializer);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return objectCreationExpressionSyntax.WithAnnotations(annotations);
			}
			return objectCreationExpressionSyntax;
		}
		return this;
	}

	public ObjectCreationExpressionSyntax WithNewKeyword(SyntaxToken newKeyword)
	{
		return Update(newKeyword, Type, ArgumentList, Initializer);
	}

	public ObjectCreationExpressionSyntax WithType(TypeSyntax type)
	{
		return Update(NewKeyword, type, ArgumentList, Initializer);
	}

	public ObjectCreationExpressionSyntax WithArgumentList(ArgumentListSyntax argumentList)
	{
		return Update(NewKeyword, Type, argumentList, Initializer);
	}

	public ObjectCreationExpressionSyntax WithInitializer(InitializerExpressionSyntax initializer)
	{
		return Update(NewKeyword, Type, ArgumentList, initializer);
	}

	public ObjectCreationExpressionSyntax AddArgumentListArguments(params ArgumentSyntax[] items)
	{
		ArgumentListSyntax argumentListSyntax = ArgumentList ?? SyntaxFactory.ArgumentList();
		return WithArgumentList(argumentListSyntax.WithArguments(argumentListSyntax.Arguments.AddRange(items)));
	}
}
