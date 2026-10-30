using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TypeParameterListSyntax : CSharpSyntaxNode
{
	private SyntaxNode parameters;

	public SyntaxToken LessThanToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)Green).lessThanToken, Position, 0);

	public SeparatedSyntaxList<TypeParameterSyntax> Parameters
	{
		get
		{
			SyntaxNode red = GetRed(ref parameters, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<TypeParameterSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken GreaterThanToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TypeParameterListSyntax)Green).greaterThanToken, GetChildPosition(2), GetChildIndex(2));

	internal TypeParameterListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref parameters, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return parameters;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTypeParameterList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTypeParameterList(this);
	}

	public TypeParameterListSyntax Update(SyntaxToken lessThanToken, SeparatedSyntaxList<TypeParameterSyntax> parameters, SyntaxToken greaterThanToken)
	{
		if (lessThanToken != LessThanToken || parameters != Parameters || greaterThanToken != GreaterThanToken)
		{
			TypeParameterListSyntax typeParameterListSyntax = SyntaxFactory.TypeParameterList(lessThanToken, parameters, greaterThanToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return typeParameterListSyntax.WithAnnotations(annotations);
			}
			return typeParameterListSyntax;
		}
		return this;
	}

	public TypeParameterListSyntax WithLessThanToken(SyntaxToken lessThanToken)
	{
		return Update(lessThanToken, Parameters, GreaterThanToken);
	}

	public TypeParameterListSyntax WithParameters(SeparatedSyntaxList<TypeParameterSyntax> parameters)
	{
		return Update(LessThanToken, parameters, GreaterThanToken);
	}

	public TypeParameterListSyntax WithGreaterThanToken(SyntaxToken greaterThanToken)
	{
		return Update(LessThanToken, Parameters, greaterThanToken);
	}

	public TypeParameterListSyntax AddParameters(params TypeParameterSyntax[] items)
	{
		return WithParameters(Parameters.AddRange(items));
	}
}
