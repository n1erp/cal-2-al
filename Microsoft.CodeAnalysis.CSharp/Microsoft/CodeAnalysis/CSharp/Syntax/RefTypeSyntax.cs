using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class RefTypeSyntax : TypeSyntax
{
	private TypeSyntax type;

	public SyntaxToken RefKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefTypeSyntax)Green).refKeyword, Position, 0);

	public SyntaxToken ReadOnlyKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken readOnlyKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.RefTypeSyntax)Green).readOnlyKeyword;
			if (readOnlyKeyword != null)
			{
				return new SyntaxToken(this, readOnlyKeyword, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public TypeSyntax Type => GetRed(ref type, 2);

	internal RefTypeSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref type, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return type;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRefType(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitRefType(this);
	}

	public RefTypeSyntax Update(SyntaxToken refKeyword, SyntaxToken readOnlyKeyword, TypeSyntax type)
	{
		if (refKeyword != RefKeyword || readOnlyKeyword != ReadOnlyKeyword || type != Type)
		{
			RefTypeSyntax refTypeSyntax = SyntaxFactory.RefType(refKeyword, readOnlyKeyword, type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return refTypeSyntax.WithAnnotations(annotations);
			}
			return refTypeSyntax;
		}
		return this;
	}

	public RefTypeSyntax WithRefKeyword(SyntaxToken refKeyword)
	{
		return Update(refKeyword, ReadOnlyKeyword, Type);
	}

	public RefTypeSyntax WithReadOnlyKeyword(SyntaxToken readOnlyKeyword)
	{
		return Update(RefKeyword, readOnlyKeyword, Type);
	}

	public RefTypeSyntax WithType(TypeSyntax type)
	{
		return Update(RefKeyword, ReadOnlyKeyword, type);
	}

	public RefTypeSyntax Update(SyntaxToken refKeyword, TypeSyntax type)
	{
		return Update(refKeyword, default, type);
	}
}
