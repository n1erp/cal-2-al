using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AttributeArgumentSyntax : CSharpSyntaxNode
{
	private NameEqualsSyntax nameEquals;

	private NameColonSyntax nameColon;

	private ExpressionSyntax expression;

	public NameEqualsSyntax NameEquals => GetRedAtZero(ref nameEquals);

	public NameColonSyntax NameColon => GetRed(ref nameColon, 1);

	public ExpressionSyntax Expression => GetRed(ref expression, 2);

	internal AttributeArgumentSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref nameEquals), 
			1 => GetRed(ref nameColon, 1), 
			2 => GetRed(ref expression, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)nameEquals, 
			1 => nameColon, 
			2 => expression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAttributeArgument(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAttributeArgument(this);
	}

	public AttributeArgumentSyntax Update(NameEqualsSyntax nameEquals, NameColonSyntax nameColon, ExpressionSyntax expression)
	{
		if (nameEquals != NameEquals || nameColon != NameColon || expression != Expression)
		{
			AttributeArgumentSyntax attributeArgumentSyntax = SyntaxFactory.AttributeArgument(nameEquals, nameColon, expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return attributeArgumentSyntax.WithAnnotations(annotations);
			}
			return attributeArgumentSyntax;
		}
		return this;
	}

	public AttributeArgumentSyntax WithNameEquals(NameEqualsSyntax nameEquals)
	{
		return Update(nameEquals, NameColon, Expression);
	}

	public AttributeArgumentSyntax WithNameColon(NameColonSyntax nameColon)
	{
		return Update(NameEquals, nameColon, Expression);
	}

	public AttributeArgumentSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(NameEquals, NameColon, expression);
	}
}
