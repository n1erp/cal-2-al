using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TupleElementSyntax : CSharpSyntaxNode
{
	private TypeSyntax type;

	public TypeSyntax Type => GetRedAtZero(ref type);

	public SyntaxToken Identifier
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken identifier = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TupleElementSyntax)Green).identifier;
			if (identifier != null)
			{
				return new SyntaxToken(this, identifier, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	internal TupleElementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref type);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return type;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTupleElement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTupleElement(this);
	}

	public TupleElementSyntax Update(TypeSyntax type, SyntaxToken identifier)
	{
		if (type != Type || identifier != Identifier)
		{
			TupleElementSyntax tupleElementSyntax = SyntaxFactory.TupleElement(type, identifier);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tupleElementSyntax.WithAnnotations(annotations);
			}
			return tupleElementSyntax;
		}
		return this;
	}

	public TupleElementSyntax WithType(TypeSyntax type)
	{
		return Update(type, Identifier);
	}

	public TupleElementSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(Type, identifier);
	}
}
