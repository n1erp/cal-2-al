using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CrefParameterSyntax : CSharpSyntaxNode
{
	private TypeSyntax type;

	public SyntaxToken RefKindKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken refKindKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CrefParameterSyntax)Green).refKindKeyword;
			if (refKindKeyword != null)
			{
				return new SyntaxToken(this, refKindKeyword, Position, 0);
			}
			return default;
		}
	}

	public TypeSyntax Type => GetRed(ref type, 1);

	[EditorBrowsable(EditorBrowsableState.Never)]
	public SyntaxToken RefOrOutKeyword => RefKindKeyword;

	internal CrefParameterSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref type, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return type;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCrefParameter(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCrefParameter(this);
	}

	public CrefParameterSyntax Update(SyntaxToken refKindKeyword, TypeSyntax type)
	{
		if (refKindKeyword != RefKindKeyword || type != Type)
		{
			CrefParameterSyntax crefParameterSyntax = SyntaxFactory.CrefParameter(refKindKeyword, type);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return crefParameterSyntax.WithAnnotations(annotations);
			}
			return crefParameterSyntax;
		}
		return this;
	}

	public CrefParameterSyntax WithRefKindKeyword(SyntaxToken refKindKeyword)
	{
		return Update(refKindKeyword, Type);
	}

	public CrefParameterSyntax WithType(TypeSyntax type)
	{
		return Update(RefKindKeyword, type);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public CrefParameterSyntax WithRefOrOutKeyword(SyntaxToken refOrOutKeyword)
	{
		return Update(refOrOutKeyword, Type);
	}
}
