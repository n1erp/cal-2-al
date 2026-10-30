using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ParameterSyntax : CSharpSyntaxNode
{
	private SyntaxNode attributeLists;

	private TypeSyntax type;

	private EqualsValueClauseSyntax @default;

	public SyntaxList<AttributeListSyntax> AttributeLists => new SyntaxList<AttributeListSyntax>(GetRed(ref attributeLists, 0));

	public SyntaxTokenList Modifiers
	{
		get
		{
			GreenNode slot = Green.GetSlot(1);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, GetChildPosition(1), GetChildIndex(1));
			}
			return default;
		}
	}

	public TypeSyntax Type => GetRed(ref type, 2);

	public SyntaxToken Identifier => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParameterSyntax)Green).identifier, GetChildPosition(3), GetChildIndex(3));

	public EqualsValueClauseSyntax Default => GetRed(ref @default, 4);

	internal bool IsArgList
	{
		get
		{
			if (Type == null)
			{
				return Identifier.ContextualKind() == SyntaxKind.ArgListKeyword;
			}
			return false;
		}
	}

	internal ParameterSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref attributeLists), 
			2 => GetRed(ref type, 2), 
			4 => GetRed(ref @default, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => attributeLists, 
			2 => type, 
			4 => @default, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public ParameterSyntax Update(SyntaxList<AttributeListSyntax> attributeLists, SyntaxTokenList modifiers, TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default)
	{
		if (attributeLists != AttributeLists || modifiers != Modifiers || type != Type || identifier != Identifier || @default != Default)
		{
			ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(attributeLists, modifiers, type, identifier, @default);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parameterSyntax.WithAnnotations(annotations);
			}
			return parameterSyntax;
		}
		return this;
	}

	public ParameterSyntax WithAttributeLists(SyntaxList<AttributeListSyntax> attributeLists)
	{
		return Update(attributeLists, Modifiers, Type, Identifier, Default);
	}

	public ParameterSyntax WithModifiers(SyntaxTokenList modifiers)
	{
		return Update(AttributeLists, modifiers, Type, Identifier, Default);
	}

	public ParameterSyntax WithType(TypeSyntax type)
	{
		return Update(AttributeLists, Modifiers, type, Identifier, Default);
	}

	public ParameterSyntax WithIdentifier(SyntaxToken identifier)
	{
		return Update(AttributeLists, Modifiers, Type, identifier, Default);
	}

	public ParameterSyntax WithDefault(EqualsValueClauseSyntax @default)
	{
		return Update(AttributeLists, Modifiers, Type, Identifier, @default);
	}

	public ParameterSyntax AddAttributeLists(params AttributeListSyntax[] items)
	{
		return WithAttributeLists(AttributeLists.AddRange(items));
	}

	public ParameterSyntax AddModifiers(params SyntaxToken[] items)
	{
		return WithModifiers(Modifiers.AddRange(items));
	}
}
