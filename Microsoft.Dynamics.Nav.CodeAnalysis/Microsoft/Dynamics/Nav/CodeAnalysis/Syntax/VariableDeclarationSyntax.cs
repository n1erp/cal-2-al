using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class VariableDeclarationSyntax : SyntaxNode
{
	private SyntaxNode _attributes;

	private IdentifierNameSyntax _name;

	private TypeReferenceBaseSyntax _type;

	public SyntaxList<MemberAttributeSyntax> Attributes => new SyntaxList<MemberAttributeSyntax>(GetRed(ref _attributes, 0));

	public IdentifierNameSyntax Name => GetRed(ref _name, 1);

	public SyntaxToken ColonToken => new SyntaxToken(this, ((InternalVariableDeclarationSyntax)Node)._colonToken, GetChildPosition(2), GetChildIndex(2));

	[SuppressMessage("Microsoft.Naming", "CA1721: PropertyNamesShouldNotMatchGetMethods")]
	public TypeReferenceBaseSyntax Type => GetRed(ref _type, 3);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalVariableDeclarationSyntax)Node)._semicolonToken, GetChildPosition(4), GetChildIndex(4));

	internal VariableDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _attributes), 
			1 => GetRed(ref _name, 1), 
			3 => GetRed(ref _type, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			1 => _name, 
			3 => _type, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitVariableDeclaration(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitVariableDeclaration(this);
	}

	public VariableDeclarationSyntax Update(SyntaxList<MemberAttributeSyntax> attributes, IdentifierNameSyntax name, SyntaxToken colonToken, TypeReferenceBaseSyntax type, SyntaxToken semicolonToken)
	{
		if (attributes != Attributes || name != Name || colonToken != ColonToken || type != Type || semicolonToken != SemicolonToken)
		{
			VariableDeclarationSyntax variableDeclarationSyntax = SyntaxFactory.VariableDeclaration(attributes, name, colonToken, type, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return variableDeclarationSyntax.WithAnnotations(annotations);
			}
			return variableDeclarationSyntax;
		}
		return this;
	}

	public VariableDeclarationSyntax WithAttributes(SyntaxList<MemberAttributeSyntax> attributes)
	{
		return Update(attributes, Name, ColonToken, Type, SemicolonToken);
	}

	public VariableDeclarationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Attributes, name, ColonToken, Type, SemicolonToken);
	}

	public VariableDeclarationSyntax WithColonToken(SyntaxToken colonToken)
	{
		return Update(Attributes, Name, colonToken, Type, SemicolonToken);
	}

	public VariableDeclarationSyntax WithType(TypeReferenceBaseSyntax type)
	{
		return Update(Attributes, Name, ColonToken, type, SemicolonToken);
	}

	public VariableDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Attributes, Name, ColonToken, Type, semicolonToken);
	}

	public VariableDeclarationSyntax AddAttributes(params MemberAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}
}
