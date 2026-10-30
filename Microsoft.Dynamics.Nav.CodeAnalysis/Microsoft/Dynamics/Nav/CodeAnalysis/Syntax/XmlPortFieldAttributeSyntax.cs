using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortFieldAttributeSyntax : XmlPortFieldNodeSyntax
{
	private IdentifierNameSyntax _name;

	private CodeExpressionSyntax _sourceField;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _schema;

	private SyntaxNode _triggers;

	public SyntaxToken XmlPortFieldAttributeKeyword => new SyntaxToken(this, ((InternalXmlPortFieldAttributeSyntax)Node)._xmlPortFieldAttributeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalXmlPortFieldAttributeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalXmlPortFieldAttributeSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public override CodeExpressionSyntax SourceField => GetRed(ref _sourceField, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalXmlPortFieldAttributeSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortFieldAttributeSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxList<XmlPortNodeSyntax> Schema => new SyntaxList<XmlPortNodeSyntax>(GetRed(ref _schema, 8));

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 9));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortFieldAttributeSyntax)Node)._closeBraceToken, GetChildPosition(10), GetChildIndex(10));

	internal XmlPortFieldAttributeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _sourceField, 4), 
			7 => GetRed(ref _propertyList, 7), 
			8 => GetRed(ref _schema, 8), 
			9 => GetRed(ref _triggers, 9), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			4 => _sourceField, 
			7 => _propertyList, 
			8 => _schema, 
			9 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortFieldAttribute(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPortFieldAttribute(this);
	}

	public XmlPortFieldAttributeSyntax Update(SyntaxToken xmlPortFieldAttributeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax sourceField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (xmlPortFieldAttributeKeyword != XmlPortFieldAttributeKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceField != SourceField || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			XmlPortFieldAttributeSyntax xmlPortFieldAttributeSyntax = SyntaxFactory.XmlPortFieldAttribute(xmlPortFieldAttributeKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortFieldAttributeSyntax.WithAnnotations(annotations);
			}
			return xmlPortFieldAttributeSyntax;
		}
		return this;
	}

	public XmlPortFieldAttributeSyntax WithXmlPortFieldAttributeKeyword(SyntaxToken xmlPortFieldAttributeKeyword)
	{
		return Update(xmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(XmlPortFieldAttributeKeyword, openParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, semicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithSourceField(CodeExpressionSyntax sourceField)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, sourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, closeParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, openBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, propertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithSchema(SyntaxList<XmlPortNodeSyntax> schema)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, triggers, CloseBraceToken);
	}

	public XmlPortFieldAttributeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortFieldAttributeKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, closeBraceToken);
	}

	public XmlPortFieldAttributeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public XmlPortFieldAttributeSyntax AddSchema(params XmlPortNodeSyntax[] items)
	{
		return WithSchema(Schema.AddRange(items));
	}

	public XmlPortFieldAttributeSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
