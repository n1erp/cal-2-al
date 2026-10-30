using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableSyntax : InternalApplicationObjectSyntax
{
	internal readonly InternalSyntaxToken _tableKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalFieldListSyntax _fields;

	internal readonly InternalKeyListSyntax _keys;

	internal readonly InternalFieldGroupListSyntax _fieldGroups;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => TableSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken TableKeyword => _tableKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalFieldListSyntax Fields => _fields;

	public InternalKeyListSyntax Keys => _keys;

	public InternalFieldGroupListSyntax FieldGroups => _fieldGroups;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalTableSyntax(SyntaxKind kind, InternalSyntaxToken tableKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupListSyntax fieldGroups, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(tableKeyword);
		_tableKeyword = tableKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(fields);
		_fields = fields;
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

	internal InternalTableSyntax(SyntaxKind kind, InternalSyntaxToken tableKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupListSyntax fieldGroups, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(tableKeyword);
		_tableKeyword = tableKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(fields);
		_fields = fields;
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

	internal InternalTableSyntax(SyntaxKind kind, InternalSyntaxToken tableKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupListSyntax fieldGroups, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(tableKeyword);
		_tableKeyword = tableKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		AdjustFlagsAndWidth(fields);
		_fields = fields;
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
			0 => _tableKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _openBraceToken, 
			4 => _propertyList, 
			5 => _fields, 
			6 => _keys, 
			7 => _fieldGroups, 
			8 => _members, 
			9 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTable(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTable(this);
	}

	public InternalTableSyntax Update(InternalSyntaxToken tableKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalFieldListSyntax fields, InternalKeyListSyntax keys, InternalFieldGroupListSyntax fieldGroups, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (tableKeyword != TableKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || fields != Fields || keys != Keys || fieldGroups != FieldGroups || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.Table(tableKeyword, objectId, name, openBraceToken, propertyList, fields, keys, fieldGroups, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableSyntax(Kind, _tableKeyword, _objectId, _name, _openBraceToken, _propertyList, _fields, _keys, _fieldGroups, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableSyntax(Kind, _tableKeyword, _objectId, _name, _openBraceToken, _propertyList, _fields, _keys, _fieldGroups, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
