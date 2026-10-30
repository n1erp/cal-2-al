using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalEnumTypeSyntax : InternalApplicationObjectSyntax
{
	internal readonly InternalSyntaxToken _enumToken;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _values;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => EnumTypeSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken EnumToken => _enumToken;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxList<InternalEnumValueSyntax> Values => new InternalSyntaxList<InternalEnumValueSyntax>(_values);

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalEnumTypeSyntax(SyntaxKind kind, InternalSyntaxToken enumToken, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode values, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(enumToken);
		_enumToken = enumToken;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalEnumTypeSyntax(SyntaxKind kind, InternalSyntaxToken enumToken, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode values, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(enumToken);
		_enumToken = enumToken;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalEnumTypeSyntax(SyntaxKind kind, InternalSyntaxToken enumToken, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode values, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 8;
		AdjustFlagsAndWidth(enumToken);
		_enumToken = enumToken;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (propertyList != null)
		{
			AdjustFlagsAndWidth(propertyList);
			_propertyList = propertyList;
		}
		if (values != null)
		{
			AdjustFlagsAndWidth(values);
			_values = values;
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
			0 => _enumToken, 
			1 => _objectId, 
			2 => _name, 
			3 => _openBraceToken, 
			4 => _propertyList, 
			5 => _values, 
			6 => _members, 
			7 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new EnumTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitEnumType(this);
	}

	public InternalEnumTypeSyntax Update(InternalSyntaxToken enumToken, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalEnumValueSyntax> values, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (enumToken != EnumToken || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || values != Values || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.EnumType(enumToken, objectId, name, openBraceToken, propertyList, values, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalEnumTypeSyntax(Kind, _enumToken, _objectId, _name, _openBraceToken, _propertyList, _values, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalEnumTypeSyntax(Kind, _enumToken, _objectId, _name, _openBraceToken, _propertyList, _values, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
