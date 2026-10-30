using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortFieldElementSyntax : XmlPortFieldNodeSyntax
{
	private IdentifierNameSyntax _name;

	private CodeExpressionSyntax _sourceField;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _schema;

	private SyntaxNode _triggers;

	public SyntaxToken XmlPortFieldElementKeyword => new SyntaxToken(this, ((InternalXmlPortFieldElementSyntax)Node)._xmlPortFieldElementKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalXmlPortFieldElementSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalXmlPortFieldElementSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public override CodeExpressionSyntax SourceField => GetRed(ref _sourceField, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalXmlPortFieldElementSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortFieldElementSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxList<XmlPortNodeSyntax> Schema => new SyntaxList<XmlPortNodeSyntax>(GetRed(ref _schema, 8));

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 9));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortFieldElementSyntax)Node)._closeBraceToken, GetChildPosition(10), GetChildIndex(10));

	internal XmlPortFieldElementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitXmlPortFieldElement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPortFieldElement(this);
	}

	public XmlPortFieldElementSyntax Update(SyntaxToken xmlPortFieldElementKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax sourceField, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (xmlPortFieldElementKeyword != XmlPortFieldElementKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceField != SourceField || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			XmlPortFieldElementSyntax xmlPortFieldElementSyntax = SyntaxFactory.XmlPortFieldElement(xmlPortFieldElementKeyword, openParenthesisToken, name, semicolonToken, sourceField, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortFieldElementSyntax.WithAnnotations(annotations);
			}
			return xmlPortFieldElementSyntax;
		}
		return this;
	}

	public XmlPortFieldElementSyntax WithXmlPortFieldElementKeyword(SyntaxToken xmlPortFieldElementKeyword)
	{
		return Update(xmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(XmlPortFieldElementKeyword, openParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, semicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithSourceField(CodeExpressionSyntax sourceField)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, sourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, closeParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, openBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, propertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithSchema(SyntaxList<XmlPortNodeSyntax> schema)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, schema, Triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, triggers, CloseBraceToken);
	}

	public XmlPortFieldElementSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortFieldElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceField, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, closeBraceToken);
	}

	public XmlPortFieldElementSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public XmlPortFieldElementSyntax AddSchema(params XmlPortNodeSyntax[] items)
	{
		return WithSchema(Schema.AddRange(items));
	}

	public XmlPortFieldElementSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
