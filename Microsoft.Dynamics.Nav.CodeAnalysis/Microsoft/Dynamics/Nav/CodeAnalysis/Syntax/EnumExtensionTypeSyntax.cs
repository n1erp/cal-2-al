using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EnumExtensionTypeSyntax : ApplicationObjectExtensionSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _baseObject;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _values;

	private SyntaxNode _members;

	public SyntaxToken EnumExtensionKeyword => new SyntaxToken(this, ((InternalEnumExtensionTypeSyntax)Node)._enumExtensionKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken ExtendsKeyword => new SyntaxToken(this, ((InternalEnumExtensionTypeSyntax)Node)._extendsKeyword, GetChildPosition(3), GetChildIndex(3));

	public override ObjectNameOrIdSyntax BaseObject => GetRed(ref _baseObject, 4);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalEnumExtensionTypeSyntax)Node)._openBraceToken, GetChildPosition(5), GetChildIndex(5));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 6);

	public SyntaxList<EnumValueSyntax> Values => new SyntaxList<EnumValueSyntax>(GetRed(ref _values, 7));

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 8));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalEnumExtensionTypeSyntax)Node)._closeBraceToken, GetChildPosition(9), GetChildIndex(9));

	internal EnumExtensionTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _objectId, 1), 
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _baseObject, 4), 
			6 => GetRed(ref _propertyList, 6), 
			7 => GetRed(ref _values, 7), 
			8 => GetRed(ref _members, 8), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _objectId, 
			2 => _name, 
			4 => _baseObject, 
			6 => _propertyList, 
			7 => _values, 
			8 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumExtensionType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEnumExtensionType(this);
	}

	public EnumExtensionTypeSyntax Update(SyntaxToken enumExtensionKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken extendsKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<EnumValueSyntax> values, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (enumExtensionKeyword != EnumExtensionKeyword || objectId != ObjectId || name != Name || extendsKeyword != ExtendsKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || values != Values || members != Members || closeBraceToken != CloseBraceToken)
		{
			EnumExtensionTypeSyntax enumExtensionTypeSyntax = SyntaxFactory.EnumExtensionType(enumExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, values, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return enumExtensionTypeSyntax.WithAnnotations(annotations);
			}
			return enumExtensionTypeSyntax;
		}
		return this;
	}

	public EnumExtensionTypeSyntax WithEnumExtensionKeyword(SyntaxToken enumExtensionKeyword)
	{
		return Update(enumExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(EnumExtensionKeyword, objectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(EnumExtensionKeyword, ObjectId, name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithExtendsKeyword(SyntaxToken extendsKeyword)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, extendsKeyword, BaseObject, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithBaseObject(ObjectNameOrIdSyntax baseObject)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, ExtendsKeyword, baseObject, OpenBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, openBraceToken, PropertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, propertyList, Values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithValues(SyntaxList<EnumValueSyntax> values)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, values, Members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Values, members, CloseBraceToken);
	}

	public EnumExtensionTypeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(EnumExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Values, Members, closeBraceToken);
	}

	public EnumExtensionTypeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		PropertyListSyntax propertyListSyntax = PropertyList ?? SyntaxFactory.PropertyList();
		return WithPropertyList(propertyListSyntax.WithProperties(propertyListSyntax.Properties.AddRange(items)));
	}

	public EnumExtensionTypeSyntax AddValues(params EnumValueSyntax[] items)
	{
		return WithValues(Values.AddRange(items));
	}

	public EnumExtensionTypeSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
