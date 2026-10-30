using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableSyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private FieldListSyntax _fields;

	private KeyListSyntax _keys;

	private FieldGroupListSyntax _fieldGroups;

	private SyntaxNode _members;

	public SyntaxToken TableKeyword => new SyntaxToken(this, ((InternalTableSyntax)Node)._tableKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalTableSyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public FieldListSyntax Fields => GetRed(ref _fields, 5);

	public KeyListSyntax Keys => GetRed(ref _keys, 6);

	public FieldGroupListSyntax FieldGroups => GetRed(ref _fieldGroups, 7);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 8));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalTableSyntax)Node)._closeBraceToken, GetChildPosition(9), GetChildIndex(9));

	internal TableSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
			5 => GetRed(ref _fields, 5), 
			6 => GetRed(ref _keys, 6), 
			7 => GetRed(ref _fieldGroups, 7), 
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
			4 => _propertyList, 
			5 => _fields, 
			6 => _keys, 
			7 => _fieldGroups, 
			8 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTable(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTable(this);
	}

	public TableSyntax Update(SyntaxToken tableKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, FieldListSyntax fields, KeyListSyntax keys, FieldGroupListSyntax fieldGroups, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (tableKeyword != TableKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || fields != Fields || keys != Keys || fieldGroups != FieldGroups || members != Members || closeBraceToken != CloseBraceToken)
		{
			TableSyntax tableSyntax = SyntaxFactory.Table(tableKeyword, objectId, name, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableSyntax.WithAnnotations(annotations);
			}
			return tableSyntax;
		}
		return this;
	}

	public TableSyntax WithTableKeyword(SyntaxToken tableKeyword)
	{
		return Update(tableKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(TableKeyword, objectId, Name, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(TableKeyword, ObjectId, name, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(TableKeyword, ObjectId, Name, openBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(TableKeyword, ObjectId, Name, OpenBraceToken, propertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithFields(FieldListSyntax fields)
	{
		return Update(TableKeyword, ObjectId, Name, OpenBraceToken, PropertyList, fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithKeys(KeyListSyntax keys)
	{
		return Update(TableKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Fields, keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithFieldGroups(FieldGroupListSyntax fieldGroups)
	{
		return Update(TableKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Fields, Keys, fieldGroups, Members, CloseBraceToken);
	}

	public TableSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(TableKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, members, CloseBraceToken);
	}

	public TableSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(TableKeyword, ObjectId, Name, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, closeBraceToken);
	}

	public TableSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public TableSyntax AddFieldsFields(params FieldSyntax[] items)
	{
		return WithFields(Fields.WithFields(Fields.Fields.AddRange(items)));
	}

	public TableSyntax AddKeysKeys(params KeySyntax[] items)
	{
		KeyListSyntax keyListSyntax = Keys ?? SyntaxFactory.KeyList();
		return WithKeys(keyListSyntax.WithKeys(keyListSyntax.Keys.AddRange(items)));
	}

	public TableSyntax AddFieldGroupsFieldGroups(params FieldGroupSyntax[] items)
	{
		FieldGroupListSyntax fieldGroupListSyntax = FieldGroups ?? SyntaxFactory.FieldGroupList();
		return WithFieldGroups(fieldGroupListSyntax.WithFieldGroups(fieldGroupListSyntax.FieldGroups.AddRange(items)));
	}

	public TableSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
