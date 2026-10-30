using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldGroupSyntax : KeyFieldGroupBaseSyntax
{
	private IdentifierNameSyntax _name;

	private SyntaxNode _fields;

	private PropertyListSyntax _propertyList;

	public SyntaxToken FieldGroupKeyword => new SyntaxToken(this, ((InternalFieldGroupSyntax)Node)._fieldGroupKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldGroupSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override SyntaxToken Id
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalFieldGroupSyntax)Node)._id;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	public override SyntaxToken SemicolonToken1
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalFieldGroupSyntax)Node)._semicolonToken1;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	public override IdentifierNameSyntax Name => GetRed(ref _name, 4);

	public override SyntaxToken SemicolonToken2 => new SyntaxToken(this, ((InternalFieldGroupSyntax)Node)._semicolonToken2, GetChildPosition(5), GetChildIndex(5));

	public override SeparatedSyntaxList<IdentifierNameSyntax> Fields
	{
		get
		{
			SyntaxNode red = GetRed(ref _fields, 6);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, GetChildIndex(6));
			}
			return default;
		}
	}

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldGroupSyntax)Node)._closeParenthesisToken, GetChildPosition(7), GetChildIndex(7));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldGroupSyntax)Node)._openBraceToken, GetChildPosition(8), GetChildIndex(8));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 9);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldGroupSyntax)Node)._closeBraceToken, GetChildPosition(10), GetChildIndex(10));

	internal FieldGroupSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			4 => GetRed(ref _name, 4), 
			6 => GetRed(ref _fields, 6), 
			9 => GetRed(ref _propertyList, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			4 => _name, 
			6 => _fields, 
			9 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldGroup(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldGroup(this);
	}

	public FieldGroupSyntax Update(SyntaxToken fieldGroupKeyword, SyntaxToken openParenthesisToken, SyntaxToken id, SyntaxToken semicolonToken1, IdentifierNameSyntax name, SyntaxToken semicolonToken2, SeparatedSyntaxList<IdentifierNameSyntax> fields, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (fieldGroupKeyword != FieldGroupKeyword || openParenthesisToken != OpenParenthesisToken || id != Id || semicolonToken1 != SemicolonToken1 || name != Name || semicolonToken2 != SemicolonToken2 || fields != Fields || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			FieldGroupSyntax fieldGroupSyntax = SyntaxFactory.FieldGroup(fieldGroupKeyword, openParenthesisToken, id, semicolonToken1, name, semicolonToken2, fields, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldGroupSyntax.WithAnnotations(annotations);
			}
			return fieldGroupSyntax;
		}
		return this;
	}

	public FieldGroupSyntax WithFieldGroupKeyword(SyntaxToken fieldGroupKeyword)
	{
		return Update(fieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(FieldGroupKeyword, openParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithId(SyntaxToken id)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, id, SemicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithSemicolonToken1(SyntaxToken semicolonToken1)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, semicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithSemicolonToken2(SyntaxToken semicolonToken2)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, semicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithFields(SeparatedSyntaxList<IdentifierNameSyntax> fields)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, fields, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, Fields, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public FieldGroupSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldGroupKeyword, OpenParenthesisToken, Id, SemicolonToken1, Name, SemicolonToken2, Fields, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public FieldGroupSyntax AddFields(params IdentifierNameSyntax[] items)
	{
		return WithFields(Fields.AddRange(items));
	}

	public FieldGroupSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
