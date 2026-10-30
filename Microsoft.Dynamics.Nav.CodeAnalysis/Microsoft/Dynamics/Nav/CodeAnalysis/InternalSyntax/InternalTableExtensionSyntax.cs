using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableExtensionSyntax : InternalApplicationObjectExtensionSyntax
{
	internal readonly InternalSyntaxToken _tableExtensionKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _extendsKeyword;

	internal readonly InternalObjectNameOrIdSyntax _baseObject;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalFieldExtensionListSyntax _fields;

	internal readonly InternalKeyListSyntax _keys;

	internal readonly InternalFieldGroupExtensionListSyntax _fieldGroups;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => TableExtensionSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken TableExtensionKeyword => _tableExtensionKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken ExtendsKeyword => _extendsKeyword;

	public override InternalObjectNameOrIdSyntax BaseObject => _baseObject;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalFieldExtensionListSyntax Fields => _fields;

	public InternalKeyListSyntax Keys => _keys;

	public InternalFieldGroupExtensionListSyntax FieldGroups => _fieldGroups;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalTableExtensionSyntax(SyntaxKind kind, InternalSyntaxToken tableExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldExtensionListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupExtensionListSyntax fieldGroups, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(tableExtensionKeyword);
		_tableExtensionKeyword = tableExtensionKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(extendsKeyword);
		_extendsKeyword = extendsKeyword;
		AdjustFlagsAndWidth(baseObject);
		_baseObject = baseObject;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		if (keys != null)
		{
			AdjustFlagsAndWidth(keys);
			_keys = keys;
		}
		if (fieldGroups != null)
		{
			AdjustFlagsAndWidth(fieldGroups);
			_fieldGroups = fieldGroups;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalTableExtensionSyntax(SyntaxKind kind, InternalSyntaxToken tableExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldExtensionListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupExtensionListSyntax fieldGroups, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(tableExtensionKeyword);
		_tableExtensionKeyword = tableExtensionKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(extendsKeyword);
		_extendsKeyword = extendsKeyword;
		AdjustFlagsAndWidth(baseObject);
		_baseObject = baseObject;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		if (keys != null)
		{
			AdjustFlagsAndWidth(keys);
			_keys = keys;
		}
		if (fieldGroups != null)
		{
			AdjustFlagsAndWidth(fieldGroups);
			_fieldGroups = fieldGroups;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalTableExtensionSyntax(SyntaxKind kind, InternalSyntaxToken tableExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldExtensionListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupExtensionListSyntax fieldGroups, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 12;
		AdjustFlagsAndWidth(tableExtensionKeyword);
		_tableExtensionKeyword = tableExtensionKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(extendsKeyword);
		_extendsKeyword = extendsKeyword;
		AdjustFlagsAndWidth(baseObject);
		_baseObject = baseObject;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (fields != null)
		{
			AdjustFlagsAndWidth(fields);
			_fields = fields;
		}
		if (keys != null)
		{
			AdjustFlagsAndWidth(keys);
			_keys = keys;
		}
		if (fieldGroups != null)
		{
			AdjustFlagsAndWidth(fieldGroups);
			_fieldGroups = fieldGroups;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _tableExtensionKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _extendsKeyword, 
			4 => _baseObject, 
			5 => _openBraceToken, 
			6 => _propertyList, 
			7 => _fields, 
			8 => _keys, 
			9 => _fieldGroups, 
			10 => _members, 
			11 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableExtensionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableExtension(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTableExtension(this);
	}

	public InternalTableExtensionSyntax Update(InternalSyntaxToken tableExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldExtensionListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupExtensionListSyntax fieldGroups, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (tableExtensionKeyword != TableExtensionKeyword || objectId != ObjectId || name != Name || extendsKeyword != ExtendsKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || fields != Fields || keys != Keys || fieldGroups != FieldGroups || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.TableExtension(tableExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableExtensionSyntax(Kind, _tableExtensionKeyword, _objectId, _name, _extendsKeyword, _baseObject, _openBraceToken, _propertyList, _fields, _keys, _fieldGroups, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableExtensionSyntax(Kind, _tableExtensionKeyword, _objectId, _name, _extendsKeyword, _baseObject, _openBraceToken, _propertyList, _fields, _keys, _fieldGroups, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
