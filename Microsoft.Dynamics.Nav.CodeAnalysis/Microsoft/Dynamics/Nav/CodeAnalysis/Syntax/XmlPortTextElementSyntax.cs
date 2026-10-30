using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortTextElementSyntax : XmlPortTextNodeSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _schema;

	private SyntaxNode _triggers;

	public SyntaxToken XmlPortTextElementKeyword => new SyntaxToken(this, ((InternalXmlPortTextElementSyntax)Node)._xmlPortTextElementKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalXmlPortTextElementSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalXmlPortTextElementSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortTextElementSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<XmlPortNodeSyntax> Schema => new SyntaxList<XmlPortNodeSyntax>(GetRed(ref _schema, 6));

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 7));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortTextElementSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal XmlPortTextElementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitXmlPortTextElement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPortTextElement(this);
	}

	public XmlPortTextElementSyntax Update(SyntaxToken xmlPortTextElementKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (xmlPortTextElementKeyword != XmlPortTextElementKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			XmlPortTextElementSyntax xmlPortTextElementSyntax = SyntaxFactory.XmlPortTextElement(xmlPortTextElementKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortTextElementSyntax.WithAnnotations(annotations);
			}
			return xmlPortTextElementSyntax;
		}
		return this;
	}

	public XmlPortTextElementSyntax WithXmlPortTextElementKeyword(SyntaxToken xmlPortTextElementKeyword)
	{
		return Update(xmlPortTextElementKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(XmlPortTextElementKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithSchema(SyntaxList<XmlPortNodeSyntax> schema)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, schema, Triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, triggers, CloseBraceToken);
	}

	public XmlPortTextElementSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortTextElementKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, closeBraceToken);
	}

	public XmlPortTextElementSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public XmlPortTextElementSyntax AddSchema(params XmlPortNodeSyntax[] items)
	{
		return WithSchema(Schema.AddRange(items));
	}

	public XmlPortTextElementSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
