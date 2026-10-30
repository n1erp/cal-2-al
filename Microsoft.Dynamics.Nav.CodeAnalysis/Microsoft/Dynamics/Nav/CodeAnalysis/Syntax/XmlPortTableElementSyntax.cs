using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class XmlPortTableElementSyntax : XmlPortNodeSyntax
{
	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _sourceTable;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _schema;

	private SyntaxNode _triggers;

	public SyntaxToken XmlPortTableElementKeyword => new SyntaxToken(this, ((InternalXmlPortTableElementSyntax)Node)._xmlPortTableElementKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalXmlPortTableElementSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalXmlPortTableElementSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public ObjectNameOrIdSyntax SourceTable => GetRed(ref _sourceTable, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalXmlPortTableElementSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalXmlPortTableElementSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxList<XmlPortNodeSyntax> Schema => new SyntaxList<XmlPortNodeSyntax>(GetRed(ref _schema, 8));

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 9));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalXmlPortTableElementSyntax)Node)._closeBraceToken, GetChildPosition(10), GetChildIndex(10));

	internal XmlPortTableElementSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _sourceTable, 4), 
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
			4 => _sourceTable, 
			7 => _propertyList, 
			8 => _schema, 
			9 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortTableElement(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitXmlPortTableElement(this);
	}

	public XmlPortTableElementSyntax Update(SyntaxToken xmlPortTableElementKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax sourceTable, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<XmlPortNodeSyntax> schema, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (xmlPortTableElementKeyword != XmlPortTableElementKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || sourceTable != SourceTable || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || schema != Schema || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			XmlPortTableElementSyntax xmlPortTableElementSyntax = SyntaxFactory.XmlPortTableElement(xmlPortTableElementKeyword, openParenthesisToken, name, semicolonToken, sourceTable, closeParenthesisToken, openBraceToken, propertyList, schema, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return xmlPortTableElementSyntax.WithAnnotations(annotations);
			}
			return xmlPortTableElementSyntax;
		}
		return this;
	}

	public XmlPortTableElementSyntax WithXmlPortTableElementKeyword(SyntaxToken xmlPortTableElementKeyword)
	{
		return Update(xmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(XmlPortTableElementKeyword, openParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, semicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithSourceTable(ObjectNameOrIdSyntax sourceTable)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, sourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, closeParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, openBraceToken, PropertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, propertyList, Schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithSchema(SyntaxList<XmlPortNodeSyntax> schema)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, schema, Triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, triggers, CloseBraceToken);
	}

	public XmlPortTableElementSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(XmlPortTableElementKeyword, OpenParenthesisToken, Name, SemicolonToken, SourceTable, CloseParenthesisToken, OpenBraceToken, PropertyList, Schema, Triggers, closeBraceToken);
	}

	public XmlPortTableElementSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public XmlPortTableElementSyntax AddSchema(params XmlPortNodeSyntax[] items)
	{
		return WithSchema(Schema.AddRange(items));
	}

	public XmlPortTableElementSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
