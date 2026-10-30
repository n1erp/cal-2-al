using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldSyntax : FieldBaseSyntax
{
	private IdentifierNameSyntax _name;

	private DataTypeSyntax _type;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _triggers;

	public override SyntaxToken FieldKeyword => new SyntaxToken(this, ((InternalFieldSyntax)Node)._fieldKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken No => new SyntaxToken(this, ((InternalFieldSyntax)Node)._no, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken SemicolonToken1 => new SyntaxToken(this, ((InternalFieldSyntax)Node)._semicolonToken1, GetChildPosition(3), GetChildIndex(3));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 4);

	public SyntaxToken SemicolonToken2 => new SyntaxToken(this, ((InternalFieldSyntax)Node)._semicolonToken2, GetChildPosition(5), GetChildIndex(5));

	[SuppressMessage("Microsoft.Naming", "CA1721: PropertyNamesShouldNotMatchGetMethods")]
	public DataTypeSyntax Type => GetRed(ref _type, 6);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldSyntax)Node)._closeParenthesisToken, GetChildPosition(7), GetChildIndex(7));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldSyntax)Node)._openBraceToken, GetChildPosition(8), GetChildIndex(8));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 9);

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 10));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldSyntax)Node)._closeBraceToken, GetChildPosition(11), GetChildIndex(11));

	internal FieldSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			4 => GetRed(ref _name, 4), 
			6 => GetRed(ref _type, 6), 
			9 => GetRed(ref _propertyList, 9), 
			10 => GetRed(ref _triggers, 10), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			4 => _name, 
			6 => _type, 
			9 => _propertyList, 
			10 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitField(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitField(this);
	}

	public FieldSyntax Update(SyntaxToken fieldKeyword, SyntaxToken openParenthesisToken, SyntaxToken no, SyntaxToken semicolonToken1, IdentifierNameSyntax name, SyntaxToken semicolonToken2, DataTypeSyntax type, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (fieldKeyword != FieldKeyword || openParenthesisToken != OpenParenthesisToken || no != No || semicolonToken1 != SemicolonToken1 || name != Name || semicolonToken2 != SemicolonToken2 || type != Type || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			FieldSyntax fieldSyntax = SyntaxFactory.Field(fieldKeyword, openParenthesisToken, no, semicolonToken1, name, semicolonToken2, type, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldSyntax.WithAnnotations(annotations);
			}
			return fieldSyntax;
		}
		return this;
	}

	public FieldSyntax WithFieldKeyword(SyntaxToken fieldKeyword)
	{
		return Update(fieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(FieldKeyword, openParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithNo(SyntaxToken no)
	{
		return Update(FieldKeyword, OpenParenthesisToken, no, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithSemicolonToken1(SyntaxToken semicolonToken1)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, semicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithSemicolonToken2(SyntaxToken semicolonToken2)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, semicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithType(DataTypeSyntax type)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, closeParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, openBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, propertyList, Triggers, CloseBraceToken);
	}

	public FieldSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, triggers, CloseBraceToken);
	}

	public FieldSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldKeyword, OpenParenthesisToken, No, SemicolonToken1, Name, SemicolonToken2, Type, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, closeBraceToken);
	}

	public FieldSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public FieldSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
