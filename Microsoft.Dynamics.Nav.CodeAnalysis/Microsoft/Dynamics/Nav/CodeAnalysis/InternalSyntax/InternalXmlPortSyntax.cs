using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalXmlPortSyntax : InternalApplicationObjectSyntax
{
	internal readonly InternalSyntaxToken _xmlPortKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalXmlPortSchemaSyntax _xmlPortSchema;

	internal readonly InternalRequestPageSyntax _xmlPortRequestPage;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => XmlPortSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken XmlPortKeyword => _xmlPortKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalXmlPortSchemaSyntax XmlPortSchema => _xmlPortSchema;

	public InternalRequestPageSyntax XmlPortRequestPage => _xmlPortRequestPage;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalXmlPortSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalXmlPortSchemaSyntax xmlPortSchema, InternalRequestPageSyntax xmlPortRequestPage, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(xmlPortKeyword);
		_xmlPortKeyword = xmlPortKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (xmlPortSchema != null)
		{
			AdjustFlagsAndWidth(xmlPortSchema);
			_xmlPortSchema = xmlPortSchema;
		}
		if (xmlPortRequestPage != null)
		{
			AdjustFlagsAndWidth(xmlPortRequestPage);
			_xmlPortRequestPage = xmlPortRequestPage;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalXmlPortSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalXmlPortSchemaSyntax xmlPortSchema, InternalRequestPageSyntax xmlPortRequestPage, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(xmlPortKeyword);
		_xmlPortKeyword = xmlPortKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (xmlPortSchema != null)
		{
			AdjustFlagsAndWidth(xmlPortSchema);
			_xmlPortSchema = xmlPortSchema;
		}
		if (xmlPortRequestPage != null)
		{
			AdjustFlagsAndWidth(xmlPortRequestPage);
			_xmlPortRequestPage = xmlPortRequestPage;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalXmlPortSyntax(SyntaxKind kind, InternalSyntaxToken xmlPortKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalXmlPortSchemaSyntax xmlPortSchema, InternalRequestPageSyntax xmlPortRequestPage, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 9;
		AdjustFlagsAndWidth(xmlPortKeyword);
		_xmlPortKeyword = xmlPortKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (xmlPortSchema != null)
		{
			AdjustFlagsAndWidth(xmlPortSchema);
			_xmlPortSchema = xmlPortSchema;
		}
		if (xmlPortRequestPage != null)
		{
			AdjustFlagsAndWidth(xmlPortRequestPage);
			_xmlPortRequestPage = xmlPortRequestPage;
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
			0 => _xmlPortKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _openBraceToken, 
			4 => _propertyList, 
			5 => _xmlPortSchema, 
			6 => _xmlPortRequestPage, 
			7 => _members, 
			8 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new XmlPortSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPort(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitXmlPort(this);
	}

	public InternalXmlPortSyntax Update(InternalSyntaxToken xmlPortKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalXmlPortSchemaSyntax xmlPortSchema, InternalRequestPageSyntax xmlPortRequestPage, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (xmlPortKeyword != XmlPortKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || xmlPortSchema != XmlPortSchema || xmlPortRequestPage != XmlPortRequestPage || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.XmlPort(xmlPortKeyword, objectId, name, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalXmlPortSyntax(Kind, _xmlPortKeyword, _objectId, _name, _openBraceToken, _propertyList, _xmlPortSchema, _xmlPortRequestPage, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalXmlPortSyntax(Kind, _xmlPortKeyword, _objectId, _name, _openBraceToken, _propertyList, _xmlPortSchema, _xmlPortRequestPage, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
