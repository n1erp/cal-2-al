using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortTextAttributeSyntax : XmlPortTextNodeSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _schema;

	private SyntaxNode _triggers;

	public SyntaxToken XmlPortTextAttributeKeyword => new SyntaxToken(this, ((InternalXmlPortTextAttributeSyntax)Node)._xmlPortTextAttributeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalXmlPortTextAttributeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalXmlPortTextAttributeSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortTextAttributeSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<XmlPortNodeSyntax> Schema => new SyntaxList<XmlPortNodeSyntax>(GetRed(ref _schema, 6));

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 7));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortTextAttributeSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal XmlPortTextAttributeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _schema, 6), 
			7 => GetRed(ref _triggers, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			5 => _propertyList, 
			6 => _schema, 
			7 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortTextAttribute(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPortTextAttribute(this);
	}

	public XmlPortTextAttributeSyntax Update(SyntaxToken xmlPortTextAttributeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (xmlPortTextAttributeKeyword != XmlPortTextAttributeKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			XmlPortTextAttributeSyntax xmlPortTextAttributeSyntax = SyntaxFactory.XmlPortTextAttribute(xmlPortTextAttributeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortTextAttributeSyntax.WithAnnotations(annotations);
			}
			return xmlPortTextAttributeSyntax;
		}
		return this;
	}

	public XmlPortTextAttributeSyntax WithXmlPortTextAttributeKeyword(SyntaxToken xmlPortTextAttributeKeyword)
	{
		return Update(xmlPortTextAttributeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(XmlPortTextAttributeKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithSchema(SyntaxList<XmlPortNodeSyntax> schema)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, triggers, CloseBraceToken);
	}

	public XmlPortTextAttributeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortTextAttributeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, closeBraceToken);
	}

	public XmlPortTextAttributeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public XmlPortTextAttributeSyntax AddSchema(params XmlPortNodeSyntax[] items)
	{
		return WithSchema(Schema.AddRange(items));
	}

	public XmlPortTextAttributeSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
