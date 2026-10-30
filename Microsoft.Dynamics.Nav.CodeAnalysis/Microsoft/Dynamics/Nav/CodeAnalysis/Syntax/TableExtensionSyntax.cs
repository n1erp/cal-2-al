using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableExtensionSyntax : ApplicationObjectExtensionSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _baseObject;

	private PropertyListSyntax _propertyList;

	private FieldExtensionListSyntax _fields;

	private KeyListSyntax _keys;

	private FieldGroupExtensionListSyntax _fieldGroups;

	private SyntaxNode _members;

	public SyntaxToken TableExtensionKeyword => new SyntaxToken(this, ((InternalTableExtensionSyntax)Node)._tableExtensionKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken ExtendsKeyword => new SyntaxToken(this, ((InternalTableExtensionSyntax)Node)._extendsKeyword, GetChildPosition(3), GetChildIndex(3));

	public override ObjectNameOrIdSyntax BaseObject => GetRed(ref _baseObject, 4);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalTableExtensionSyntax)Node)._openBraceToken, GetChildPosition(5), GetChildIndex(5));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 6);

	public FieldExtensionListSyntax Fields => GetRed(ref _fields, 7);

	public KeyListSyntax Keys => GetRed(ref _keys, 8);

	public FieldGroupExtensionListSyntax FieldGroups => GetRed(ref _fieldGroups, 9);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 10));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalTableExtensionSyntax)Node)._closeBraceToken, GetChildPosition(11), GetChildIndex(11));

	internal TableExtensionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
			7 => GetRed(ref _fields, 7), 
			8 => GetRed(ref _keys, 8), 
			9 => GetRed(ref _fieldGroups, 9), 
			10 => GetRed(ref _members, 10), 
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
			7 => _fields, 
			8 => _keys, 
			9 => _fieldGroups, 
			10 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableExtension(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTableExtension(this);
	}

	public TableExtensionSyntax Update(SyntaxToken tableExtensionKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken extendsKeyword, ObjectNameOrIdSyntax baseObject, SyntaxToken openBraceToken, PropertyListSyntax propertyList, FieldExtensionListSyntax fields, KeyListSyntax keys, FieldGroupExtensionListSyntax fieldGroups, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (tableExtensionKeyword != TableExtensionKeyword || objectId != ObjectId || name != Name || extendsKeyword != ExtendsKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || fields != Fields || keys != Keys || fieldGroups != FieldGroups || members != Members || closeBraceToken != CloseBraceToken)
		{
			TableExtensionSyntax tableExtensionSyntax = SyntaxFactory.TableExtension(tableExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableExtensionSyntax.WithAnnotations(annotations);
			}
			return tableExtensionSyntax;
		}
		return this;
	}

	public TableExtensionSyntax WithTableExtensionKeyword(SyntaxToken tableExtensionKeyword)
	{
		return Update(tableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(TableExtensionKeyword, objectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(TableExtensionKeyword, ObjectId, name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithExtendsKeyword(SyntaxToken extendsKeyword)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, extendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithBaseObject(ObjectNameOrIdSyntax baseObject)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, baseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, openBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, propertyList, Fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithFields(FieldExtensionListSyntax fields)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, fields, Keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithKeys(KeyListSyntax keys)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, keys, FieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithFieldGroups(FieldGroupExtensionListSyntax fieldGroups)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, fieldGroups, Members, CloseBraceToken);
	}

	public TableExtensionSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, members, CloseBraceToken);
	}

	public TableExtensionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(TableExtensionKeyword, ObjectId, Name, ExtendsKeyword, BaseObject, OpenBraceToken, PropertyList, Fields, Keys, FieldGroups, Members, closeBraceToken);
	}

	public TableExtensionSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public TableExtensionSyntax AddFieldsFields(params FieldBaseSyntax[] items)
	{
		FieldExtensionListSyntax fieldExtensionListSyntax = Fields ?? SyntaxFactory.FieldExtensionList();
		return WithFields(fieldExtensionListSyntax.WithFields(fieldExtensionListSyntax.Fields.AddRange(items)));
	}

	public TableExtensionSyntax AddKeysKeys(params KeySyntax[] items)
	{
		KeyListSyntax keyListSyntax = Keys ?? SyntaxFactory.KeyList();
		return WithKeys(keyListSyntax.WithKeys(keyListSyntax.Keys.AddRange(items)));
	}

	public TableExtensionSyntax AddFieldGroupsChanges(params FieldGroupChangeBaseSyntax[] items)
	{
		FieldGroupExtensionListSyntax fieldGroupExtensionListSyntax = FieldGroups ?? SyntaxFactory.FieldGroupExtensionList();
		return WithFieldGroups(fieldGroupExtensionListSyntax.WithChanges(fieldGroupExtensionListSyntax.Changes.AddRange(items)));
	}

	public TableExtensionSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
