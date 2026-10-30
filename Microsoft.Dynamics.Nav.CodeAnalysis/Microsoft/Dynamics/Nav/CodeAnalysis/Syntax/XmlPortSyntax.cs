using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortSyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private XmlPortSchemaSyntax _xmlPortSchema;

	private RequestPageSyntax _xmlPortRequestPage;

	private SyntaxNode _members;

	public SyntaxToken XmlPortKeyword => new SyntaxToken(this, ((InternalXmlPortSyntax)Node)._xmlPortKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortSyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public XmlPortSchemaSyntax XmlPortSchema => GetRed(ref _xmlPortSchema, 5);

	public RequestPageSyntax XmlPortRequestPage => GetRed(ref _xmlPortRequestPage, 6);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 7));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal XmlPortSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
			5 => GetRed(ref _xmlPortSchema, 5), 
			6 => GetRed(ref _xmlPortRequestPage, 6), 
			7 => GetRed(ref _members, 7), 
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
			5 => _xmlPortSchema, 
			6 => _xmlPortRequestPage, 
			7 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPort(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPort(this);
	}

	public XmlPortSyntax Update(SyntaxToken xmlPortKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, XmlPortSchemaSyntax xmlPortSchema, RequestPageSyntax xmlPortRequestPage, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (xmlPortKeyword != XmlPortKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || xmlPortSchema != XmlPortSchema || xmlPortRequestPage != XmlPortRequestPage || members != Members || closeBraceToken != CloseBraceToken)
		{
			XmlPortSyntax xmlPortSyntax = SyntaxFactory.XmlPort(xmlPortKeyword, objectId, name, openBraceToken, propertyList, xmlPortSchema, xmlPortRequestPage, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortSyntax.WithAnnotations(annotations);
			}
			return xmlPortSyntax;
		}
		return this;
	}

	public XmlPortSyntax WithXmlPortKeyword(SyntaxToken xmlPortKeyword)
	{
		return Update(xmlPortKeyword, ObjectId, Name, OpenBraceToken, PropertyList, XmlPortSchema, XmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(XmlPortKeyword, objectId, Name, OpenBraceToken, PropertyList, XmlPortSchema, XmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(XmlPortKeyword, ObjectId, name, OpenBraceToken, PropertyList, XmlPortSchema, XmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortKeyword, ObjectId, Name, openBraceToken, PropertyList, XmlPortSchema, XmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(XmlPortKeyword, ObjectId, Name, OpenBraceToken, propertyList, XmlPortSchema, XmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithXmlPortSchema(XmlPortSchemaSyntax xmlPortSchema)
	{
		return Update(XmlPortKeyword, ObjectId, Name, OpenBraceToken, PropertyList, xmlPortSchema, XmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithXmlPortRequestPage(RequestPageSyntax xmlPortRequestPage)
	{
		return Update(XmlPortKeyword, ObjectId, Name, OpenBraceToken, PropertyList, XmlPortSchema, xmlPortRequestPage, Members, CloseBraceToken);
	}

	public XmlPortSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(XmlPortKeyword, ObjectId, Name, OpenBraceToken, PropertyList, XmlPortSchema, XmlPortRequestPage, members, CloseBraceToken);
	}

	public XmlPortSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortKeyword, ObjectId, Name, OpenBraceToken, PropertyList, XmlPortSchema, XmlPortRequestPage, Members, closeBraceToken);
	}

	public XmlPortSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public XmlPortSyntax AddXmlPortSchemaXmlPortSchema(params XmlPortNodeSyntax[] items)
	{
		XmlPortSchemaSyntax xmlPortSchemaSyntax = XmlPortSchema ?? SyntaxFactory.XmlPortSchema();
		return WithXmlPortSchema(xmlPortSchemaSyntax.WithXmlPortSchema(xmlPortSchemaSyntax.XmlPortSchema.AddRange(items)));
	}

	public XmlPortSyntax AddXmlPortRequestPageMembers(params MemberSyntax[] items)
	{
		RequestPageSyntax requestPageSyntax = XmlPortRequestPage ?? SyntaxFactory.RequestPage();
		return WithXmlPortRequestPage(requestPageSyntax.WithMembers(requestPageSyntax.Members.AddRange(items)));
	}

	public XmlPortSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
