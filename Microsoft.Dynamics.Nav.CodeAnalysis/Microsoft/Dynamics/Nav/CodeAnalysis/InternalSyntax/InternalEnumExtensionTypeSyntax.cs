using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalEnumExtensionTypeSyntax : InternalApplicationObjectExtensionSyntax
{
	internal readonly InternalSyntaxToken _enumExtensionKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _extendsKeyword;

	internal readonly InternalObjectNameOrIdSyntax _baseObject;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalSyntaxNode _values;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => EnumExtensionTypeSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken EnumExtensionKeyword => _enumExtensionKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken ExtendsKeyword => _extendsKeyword;

	public override InternalObjectNameOrIdSyntax BaseObject => _baseObject;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalSyntaxList<InternalEnumValueSyntax> Values => new InternalSyntaxList<InternalEnumValueSyntax>(_values);

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalEnumExtensionTypeSyntax(SyntaxKind kind, InternalSyntaxToken enumExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode values, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(enumExtensionKeyword);
		_enumExtensionKeyword = enumExtensionKeyword;
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

	internal InternalEnumExtensionTypeSyntax(SyntaxKind kind, InternalSyntaxToken enumExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode values, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(enumExtensionKeyword);
		_enumExtensionKeyword = enumExtensionKeyword;
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

	internal InternalEnumExtensionTypeSyntax(SyntaxKind kind, InternalSyntaxToken enumExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxNode values, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(enumExtensionKeyword);
		_enumExtensionKeyword = enumExtensionKeyword;
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
			0 => _enumExtensionKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _extendsKeyword, 
			4 => _baseObject, 
			5 => _openBraceToken, 
			6 => _propertyList, 
			7 => _values, 
			8 => _members, 
			9 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new EnumExtensionTypeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEnumExtensionType(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitEnumExtensionType(this);
	}

	public InternalEnumExtensionTypeSyntax Update(InternalSyntaxToken enumExtensionKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken extendsKeyword, InternalObjectNameOrIdSyntax baseObject, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalSyntaxList<InternalEnumValueSyntax> values, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (enumExtensionKeyword != EnumExtensionKeyword || objectId != ObjectId || name != Name || extendsKeyword != ExtendsKeyword || baseObject != BaseObject || openBraceToken != OpenBraceToken || propertyList != PropertyList || values != Values || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.EnumExtensionType(enumExtensionKeyword, objectId, name, extendsKeyword, baseObject, openBraceToken, propertyList, values, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalEnumExtensionTypeSyntax(Kind, _enumExtensionKeyword, _objectId, _name, _extendsKeyword, _baseObject, _openBraceToken, _propertyList, _values, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalEnumExtensionTypeSyntax(Kind, _enumExtensionKeyword, _objectId, _name, _extendsKeyword, _baseObject, _openBraceToken, _propertyList, _values, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
