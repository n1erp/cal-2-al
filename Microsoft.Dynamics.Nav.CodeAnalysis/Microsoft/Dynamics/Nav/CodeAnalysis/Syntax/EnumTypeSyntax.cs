using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EnumTypeSyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _values;

	private SyntaxNode _members;

	public SyntaxToken EnumToken => new SyntaxToken(this, ((InternalEnumTypeSyntax)Node)._enumToken, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalEnumTypeSyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public SyntaxList<EnumValueSyntax> Values => new SyntaxList<EnumValueSyntax>(GetRed(ref _values, 5));

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 6));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalEnumTypeSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal EnumTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _objectId, 1), 
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _propertyList, 4), 
			5 => GetRed(ref _values, 5), 
			6 => GetRed(ref _members, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _objectId, 
			2 => _name, 
			4 => _propertyList, 
			5 => _values, 
			6 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEnumType(this);
	}

	public EnumTypeSyntax Update(SyntaxToken enumToken, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<EnumValueSyntax> values, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (enumToken != EnumToken || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || values != Values || members != Members || closeBraceToken != CloseBraceToken)
		{
			EnumTypeSyntax enumTypeSyntax = SyntaxFactory.EnumType(enumToken, objectId, name, openBraceToken, propertyList, values, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return enumTypeSyntax.WithAnnotations(annotations);
			}
			return enumTypeSyntax;
		}
		return this;
	}

	public EnumTypeSyntax WithEnumToken(SyntaxToken enumToken)
	{
		return Update(enumToken, ObjectId, Name, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumTypeSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(EnumToken, objectId, Name, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumTypeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(EnumToken, ObjectId, name, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumTypeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(EnumToken, ObjectId, Name, openBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumTypeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(EnumToken, ObjectId, Name, OpenBraceToken, propertyList, Values, Members, CloseBraceToken);
	}

	public EnumTypeSyntax WithValues(SyntaxList<EnumValueSyntax> values)
	{
		return Update(EnumToken, ObjectId, Name, OpenBraceToken, PropertyList, values, Members, CloseBraceToken);
	}

	public EnumTypeSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(EnumToken, ObjectId, Name, OpenBraceToken, PropertyList, Values, members, CloseBraceToken);
	}

	public EnumTypeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(EnumToken, ObjectId, Name, OpenBraceToken, PropertyList, Values, Members, closeBraceToken);
	}

	public EnumTypeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		PropertyListSyntax propertyListSyntax = PropertyList ?? SyntaxFactory.PropertyList();
		return WithPropertyList(propertyListSyntax.WithProperties(propertyListSyntax.Properties.AddRange(items)));
	}

	public EnumTypeSyntax AddValues(params EnumValueSyntax[] items)
	{
		return WithValues(Values.AddRange(items));
	}

	public EnumTypeSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
